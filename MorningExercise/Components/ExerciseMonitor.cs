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
        private const float EQUIPMENT_CHECK_CACHE_TIME = 0.5f;

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
                return;
            }

            // Skip if already has the buff
            bool hasWarmUp = HasWarmUpBuff();
            if (hasWarmUp)
            {
                if (waitingChore != null && !waitingChore.isComplete)
                {
                    CancelWaitingChore();
                }
                return;
            }

            // Skip if already exercising
            bool isExercising = false;
            if (choreDriver != null)
            {
                var currentChore = choreDriver.GetCurrentChore();
                if (currentChore != null && currentChore.choreType == MorningExercisePatches.ExerciseChoreType)
                {
                    isExercising = true;
                }
            }

            if (isExercising)
            {
                if (waitingChore != null && !waitingChore.isComplete)
                {
                    CancelWaitingChore();
                }
                return;
            }

            // Check if equipment is available
            bool hasAvailableGenerator = HasAvailableExerciseEquipmentCached();

            if (!hasAvailableGenerator)
            {
                // No equipment - create waiting chore
                if (waitingChore == null || waitingChore.isComplete)
                {
                    CreateWaitingChore();
                }
            }
            else
            {
                // Equipment available - cancel waiting chore and let them exercise
                if (waitingChore != null && !waitingChore.isComplete)
                {
                    CancelWaitingChore();
                }
                
                // Cancel idle chore so they can pick up the exercise chore
                if (choreDriver != null)
                {
                    var currentChore = choreDriver.GetCurrentChore();
                    if (currentChore != null && currentChore.choreType == Db.Get().ChoreTypes.Idle)
                    {
                        currentChore.Cancel("Equipment available for exercise");
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
