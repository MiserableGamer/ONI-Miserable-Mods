using System;
using Klei.AI;
using PeterHan.PLib.Options;
using UnityEngine;

namespace MorningExercise
{
    // Monitors each dupe to see if they need a waiting chore when no exercise equipment is available
    public class ExerciseMonitor : KMonoBehaviour, ISim200ms
    {
        private bool IsBionicDupe()
        {
            if (gameObject == null) return false;
            
            var smc = gameObject.GetComponent<StateMachineController>();
            if (smc != null)
            {
                if (smc.GetSMI<BionicUpgradesMonitor.Instance>() != null) return true;
                if (smc.GetSMI<BionicBatteryMonitor.Instance>() != null) return true;
            }
            
            return false;
        }
        private Schedulable schedulable;
        private Effects effects;
        private ChoreDriver choreDriver;
        private ChoreProvider choreProvider;
        private WaitingForExerciseChore waitingChore;
        
        // Cache world ID and equipment check results to avoid repeated lookups
        private int cachedWorldId = -1;
        private bool lastEquipmentCheckResult = false;
        private float lastEquipmentCheckTime = -1f;
        private const float EQUIPMENT_CHECK_CACHE_TIME = 1.0f; // Increased from 0.5s to reduce checks
        
        // Track if we've already canceled idle chore for this recreation period
        private bool hasCanceledIdleForRecreation = false;
        private bool wasInRecreationTime = false;
        private float lastIdleCancelTime = -1f;
        private const float IDLE_CANCEL_COOLDOWN = 3f; // Increased from 2s to reduce jitter
        
        // Debounce waiting chore creation/cancellation to prevent rapid state changes
        private bool lastEquipmentState = false;
        private float lastEquipmentStateChangeTime = -1f;
        private const float EQUIPMENT_STATE_DEBOUNCE_TIME = 1.5f; // Wait 1.5s before creating/cancelling waiting chore
        
        // Cache current chore state to avoid repeated lookups
        private ChoreType cachedCurrentChoreType = null;
        private float lastChoreCheckTime = -1f;
        private const float CHORE_CHECK_CACHE_TIME = 0.4f; // Cache chore type for 400ms

        protected override void OnSpawn()
        {
            base.OnSpawn();
            TryGetComponent(out schedulable);
            TryGetComponent(out effects);
            TryGetComponent(out choreDriver);
            TryGetComponent(out choreProvider);
            cachedWorldId = this.GetMyWorldId();
        }

        protected override void OnCleanUp()
        {
            CancelWaitingChore();
            base.OnCleanUp();
        }

        private ChoreType GetCurrentChoreTypeCached()
        {
            float currentTime = GameClock.Instance != null ? GameClock.Instance.GetTime() : 0f;
            
            // Refresh cache if needed
            if (lastChoreCheckTime < 0f || (currentTime - lastChoreCheckTime) >= CHORE_CHECK_CACHE_TIME)
            {
                if (choreDriver != null)
                {
                    var currentChore = choreDriver.GetCurrentChore();
                    cachedCurrentChoreType = currentChore?.choreType;
                }
                else
                {
                    cachedCurrentChoreType = null;
                }
                lastChoreCheckTime = currentTime;
            }
            
            return cachedCurrentChoreType;
        }

        public void Sim200ms(float dt)
        {
            if (schedulable == null) return;

            // Check if exercise is enabled for this dupe type
            var options = POptions.ReadSettings<Options.MorningExerciseOptions>() ?? new Options.MorningExerciseOptions();
            bool isBionic = IsBionicDupe();
            if ((isBionic && !options.EnableBionicExercise) || (!isBionic && !options.EnableStandardExercise))
            {
                if (waitingChore != null && !waitingChore.isComplete)
                {
                    CancelWaitingChore();
                }
                // Even if exercise is disabled, if they have the buff, allow them to go to recreation
                if (HasWarmUpBuff())
                {
                    AllowRelaxationChores();
                }
                return;
            }

            // Check if they have the buff - if so, allow recreation (works even if not in exercise time)
            bool hasWarmUp = HasWarmUpBuff();
            if (hasWarmUp)
            {
                if (waitingChore != null && !waitingChore.isComplete)
                {
                    CancelWaitingChore();
                }
                
                // Use cached chore type to avoid repeated lookups
                var currentChoreType = GetCurrentChoreTypeCached();
                bool isCurrentlyRelaxing = currentChoreType == Db.Get().ChoreTypes.Relax;
                bool isCurrentlyExercising = currentChoreType == MorningExercisePatches.ExerciseChoreType;
                
                // If they're currently relaxing, let them continue
                if (isCurrentlyRelaxing)
                {
                    return;
                }
                
                // If they're currently exercising, let them continue
                if (isCurrentlyExercising)
                {
                    return;
                }
                
                // If in exercise time and they have the buff but aren't doing anything,
                // allow them to go to relaxation (if available) or idle
                if (IsInExerciseTime())
                {
                    AllowRelaxationChores();
                    // If they're not going to relax, let them idle (they've already exercised)
                    return;
                }
                
                // If not in exercise time, we're done - they should go to recreation or idle naturally
                return;
            }

            // Check if it's exercise time
            bool isExerciseTime = IsInExerciseTime();
            if (!isExerciseTime)
            {
                if (waitingChore != null && !waitingChore.isComplete)
                {
                    CancelWaitingChore();
                }
                // Reset debounce state when not in exercise time
                lastEquipmentState = false;
                lastEquipmentStateChangeTime = -1f;
                return;
            }

            // Use cached chore type
            var choreType = GetCurrentChoreTypeCached();
            bool isExercising = choreType == MorningExercisePatches.ExerciseChoreType;

            if (isExercising)
            {
                if (waitingChore != null && !waitingChore.isComplete)
                {
                    CancelWaitingChore();
                }
                // Reset debounce state when exercising
                lastEquipmentState = false;
                lastEquipmentStateChangeTime = -1f;
                return;
            }

            // Don't create exercise chores if they already have the buff
            // They've already exercised, so let them do other things (like relax or idle)
            if (HasWarmUpBuff())
            {
                if (waitingChore != null && !waitingChore.isComplete)
                {
                    CancelWaitingChore();
                }
                return;
            }
            
            // Check if equipment is available (cached)
            bool hasAvailableGenerator = HasAvailableExerciseEquipmentCached();
            float currentTime = GameClock.Instance != null ? GameClock.Instance.GetTime() : 0f;
            
            // Debounce equipment state changes to prevent rapid chore creation/cancellation
            if (hasAvailableGenerator != lastEquipmentState)
            {
                // State changed - start debounce timer
                if (lastEquipmentStateChangeTime < 0f)
                {
                    lastEquipmentStateChangeTime = currentTime;
                }
                
                // Only apply state change after debounce period
                float timeSinceChange = currentTime - lastEquipmentStateChangeTime;
                if (timeSinceChange >= EQUIPMENT_STATE_DEBOUNCE_TIME)
                {
                    lastEquipmentState = hasAvailableGenerator;
                    lastEquipmentStateChangeTime = -1f;
                }
                else
                {
                    // Still in debounce - use previous state
                    hasAvailableGenerator = lastEquipmentState;
                }
            }
            else
            {
                // State hasn't changed - reset debounce timer
                lastEquipmentStateChangeTime = -1f;
            }

            if (!hasAvailableGenerator)
            {
                // No equipment - create waiting chore (only if debounced)
                if (waitingChore == null || waitingChore.isComplete)
                {
                    // Only create if we've confirmed no equipment for debounce period
                    if (lastEquipmentStateChangeTime < 0f)
                    {
                        CreateWaitingChore();
                    }
                }
            }
            else
            {
                // Equipment available - cancel waiting chore (only if debounced)
                if (waitingChore != null && !waitingChore.isComplete)
                {
                    // Only cancel if we've confirmed equipment available for debounce period
                    if (lastEquipmentStateChangeTime < 0f)
                    {
                        CancelWaitingChore();
                    }
                }
                
                // Cancel idle chore so they can pick up the exercise chore (with cooldown)
                if (choreDriver != null)
                {
                    var currentChore = choreDriver.GetCurrentChore();
                    if (currentChore != null && currentChore.choreType == Db.Get().ChoreTypes.Idle)
                    {
                        float timeSinceLastCancel = currentTime - lastIdleCancelTime;
                        if (timeSinceLastCancel >= IDLE_CANCEL_COOLDOWN || lastIdleCancelTime < 0f)
                        {
                            currentChore.Cancel("Equipment available for exercise");
                            lastIdleCancelTime = currentTime;
                        }
                    }
                }
            }
        }

        private void CreateWaitingChore()
        {
            if (waitingChore != null && !waitingChore.isComplete) 
            {
                return;
            }
            
            if (choreProvider == null)
            {
                Debug.LogWarning("[MorningExercise] Cannot create waiting chore - no chore provider");
                return;
            }
            if (MorningExercisePatches.WaitingForExerciseChoreType == null)
            {
                Debug.LogWarning("[MorningExercise] Cannot create waiting chore - chore type is null");
                return;
            }
            if (MorningExercisePatches.ExerciseBlock == null)
            {
                Debug.LogWarning("[MorningExercise] Cannot create waiting chore - exercise block is null");
                return;
            }

            try
            {
                waitingChore = new WaitingForExerciseChore(this);
                
                if (waitingChore != null && choreDriver != null)
                {
                    var currentChore = choreDriver.GetCurrentChore();
                    if (currentChore != null && currentChore.choreType == Db.Get().ChoreTypes.Idle)
                    {
                        currentChore.Cancel("Waiting for exercise equipment");
                    }
                }
                else if (waitingChore == null)
                {
                    Debug.LogError("[MorningExercise] Waiting chore creation returned null!");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MorningExercise] Failed to create waiting chore: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void CancelWaitingChore()
        {
            if (waitingChore != null && !waitingChore.isComplete)
            {
                waitingChore.Cancel("No longer waiting");
            }
            waitingChore = null;
        }

        // Cache equipment check results to avoid checking every frame
        private bool HasAvailableExerciseEquipmentCached()
        {
            float currentTime = GameClock.Instance.GetTime();
            
            if (lastEquipmentCheckTime >= 0f && (currentTime - lastEquipmentCheckTime) < EQUIPMENT_CHECK_CACHE_TIME)
            {
                return lastEquipmentCheckResult;
            }
            
            lastEquipmentCheckResult = HasAvailableExerciseEquipment();
            lastEquipmentCheckTime = currentTime;
            return lastEquipmentCheckResult;
        }

        // Check if there's an available Manual Exerciser in this world
        public bool HasAvailableExerciseEquipment()
        {
            int myWorld = cachedWorldId;
            if (myWorld < 0)
            {
                myWorld = this.GetMyWorldId();
                cachedWorldId = myWorld;
            }

            foreach (var exerciseWorkable in ExerciseWorkable.AllExerciseWorkables)
            {
                if (exerciseWorkable == null) continue;
                if (exerciseWorkable.GetMyWorldId() != myWorld) continue;
                if (!exerciseWorkable.IsManualExerciser()) continue;
                if (exerciseWorkable.worker != null) continue; // Already in use

                var operational = exerciseWorkable.GetComponent<Operational>();
                if (operational == null || !operational.IsOperational) continue;

                return true;
            }

            return false;
        }

        public bool IsInExerciseTime()
        {
            if (schedulable == null) return false;
            return MorningExercisePatches.ExerciseBlock != null && 
                   schedulable.IsAllowed(MorningExercisePatches.ExerciseBlock);
        }

        public bool HasWarmUpBuff()
        {
            if (effects == null) return false;
            return effects.HasEffect(MorningExercisePatches.WARMUP_EFFECT_ID) ||
                   effects.HasEffect(MorningExercisePatches.BIONIC_WARMUP_EFFECT_ID);
        }

        private bool IsInRelaxationTime()
        {
            if (schedulable == null) return false;
            var recreationBlock = Db.Get().ScheduleBlockTypes.Recreation;
            return recreationBlock != null && schedulable.IsAllowed(recreationBlock);
        }

        private void AllowRelaxationChores()
        {
            // Only act during the Morning Exercise block - don't interfere outside of it
            if (!IsInExerciseTime())
            {
                // Reset flags when not in exercise time
                hasCanceledIdleForRecreation = false;
                lastIdleCancelTime = -1f;
                return;
            }
            
            if (choreDriver == null) return;
            
            var currentChore = choreDriver.GetCurrentChore();
            if (currentChore == null) return;
            
            // Check what they're doing
            bool isIdle = currentChore.choreType == Db.Get().ChoreTypes.Idle;
            bool isExercise = currentChore.choreType == MorningExercisePatches.ExerciseChoreType;
            bool isRelax = currentChore.choreType == Db.Get().ChoreTypes.Relax;
            
            // If they're already relaxing, don't interfere - let them continue
            if (isRelax)
            {
                hasCanceledIdleForRecreation = false;
                lastIdleCancelTime = -1f;
                return;
            }
            
            // If they're doing something that's not idle or exercise, reset flags
            if (!isIdle && !isExercise)
            {
                hasCanceledIdleForRecreation = false;
                lastIdleCancelTime = -1f;
                return;
            }
            
            // If they're idle, they've likely finished relaxing
            // Let them stay idle - they've already exercised and relaxed
            if (isIdle)
            {
                hasCanceledIdleForRecreation = false;
                lastIdleCancelTime = -1f;
                return;
            }
            
            // If they're exercising, let them continue
            if (isExercise)
            {
                return;
            }
        }
    }

    // Chore that makes dupes wait when no exercise equipment is available
    public class WaitingForExerciseChore : Chore<WaitingForExerciseChore.StatesInstance>
    {
        public WaitingForExerciseChore(IStateMachineTarget target) : base(
            MorningExercisePatches.WaitingForExerciseChoreType,
            target,
            target.GetComponent<ChoreProvider>(),
            false,
            null, null, null,
            PriorityScreen.PriorityClass.personalNeeds,
            5,
            false,
            true,
            0,
            false,
            ReportManager.ReportType.PersonalTime)
        {
            showAvailabilityInHoverText = false;
            AddPrecondition(ChorePreconditions.instance.IsScheduledTime, MorningExercisePatches.ExerciseBlock);
            smi = new StatesInstance(this, target.gameObject);
            smi.StartSM();
        }

        public class States : GameStateMachine<States, StatesInstance, WaitingForExerciseChore>
        {
            public State waiting;
            public State complete;

            public override void InitializeStates(out BaseState default_state)
            {
                default_state = waiting;

                waiting
                    .ToggleAnims("anim_idle_distracted_kanim")
                    .PlayAnim("idle_default", KAnim.PlayMode.Loop)
                    .Transition(complete, (smi) => !smi.IsInExerciseTime(), UpdateRate.SIM_1000ms)
                    .Transition(complete, (smi) => smi.HasWarmUpBuff(), UpdateRate.SIM_1000ms)
                    .Transition(complete, (smi) => smi.HasAvailableEquipment(), UpdateRate.SIM_1000ms);

                complete
                    .ReturnSuccess();
            }
        }

        public class StatesInstance : GameStateMachine<States, StatesInstance, WaitingForExerciseChore, object>.GameInstance
        {
            private ExerciseMonitor monitor;

            public StatesInstance(WaitingForExerciseChore master, GameObject duplicant) : base(master)
            {
                duplicant.TryGetComponent(out monitor);
            }

            public bool IsInExerciseTime()
            {
                return monitor != null && monitor.IsInExerciseTime();
            }

            public bool HasWarmUpBuff()
            {
                return monitor != null && monitor.HasWarmUpBuff();
            }

            public bool HasAvailableEquipment()
            {
                return monitor != null && monitor.HasAvailableExerciseEquipment();
            }
        }
    }
}
