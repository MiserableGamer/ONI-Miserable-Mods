using Klei.AI;
using PeterHan.PLib.Options;
using UnityEngine;

namespace MorningExercise
{
    // Monitors dupes during exercise time and creates appropriate chores
    public class ExerciseMonitor : KMonoBehaviour, ISim200ms
    {
        private Schedulable schedulable;
        private Effects effects;
        private ChoreDriver choreDriver;
        private ChoreProvider choreProvider;
        
        private WaitingForExerciseChore waitingChore;
        
        private int cachedWorldId = -1;

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
            
            // Not in exercise time - cancel all our chores
            if (!IsInExerciseTime())
            {
                CancelWaitingChore();
                return;
            }

            // Check if exercise is enabled for this dupe type
            var options = POptions.ReadSettings<Options.MorningExerciseOptions>() ?? new Options.MorningExerciseOptions();
            bool isBionic = IsBionicDupe();
            if ((isBionic && !options.EnableBionicExercise) || (!isBionic && !options.EnableStandardExercise))
            {
                CancelWaitingChore();
                return;
            }

            // Currently exercising - let them continue
            if (IsCurrentlyExercising())
            {
                CancelWaitingChore();
                return;
            }

            bool hasBuff = HasWarmUpBuff();
            bool hasAnyEquipment = HasAnyExerciseEquipment();
            bool hasAvailableEquipment = hasAnyEquipment && HasAvailableExerciseEquipment();

            // If has buff OR no equipment, let Relax chores handle it (precondition patch allows them)
            // No need to create our own chore - game will naturally pick recreation buildings
            if (hasBuff || !hasAnyEquipment)
            {
                CancelWaitingChore();
                // Nudge from idle so they pick up Relax chores if available
                NudgeFromIdle();
                return;
            }

            // Equipment exists but busy - create waiting chore
            if (!hasAvailableEquipment)
            {
                CreateWaitingChore();
                return;
            }

            // Equipment available - cancel waiting chore, let Exercise chore take over
            CancelWaitingChore();
            NudgeFromIdle();
        }

        private void NudgeFromIdle()
        {
            if (choreDriver == null) return;
            var currentChore = choreDriver.GetCurrentChore();
            if (currentChore != null && currentChore.choreType == Db.Get().ChoreTypes.Idle)
            {
                currentChore.Cancel("Exercise equipment available");
            }
        }

        private void CreateWaitingChore()
        {
            if (waitingChore != null && !waitingChore.isComplete) return;
            if (choreProvider == null) return;
            if (MorningExercisePatches.WaitingForExerciseChoreType == null) return;
            if (MorningExercisePatches.ExerciseBlock == null) return;

            waitingChore = new WaitingForExerciseChore(this);
            NudgeFromIdle();
        }

        private void CancelWaitingChore()
        {
            if (waitingChore != null && !waitingChore.isComplete)
            {
                waitingChore.Cancel("No longer waiting");
            }
            waitingChore = null;
        }

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

        private bool IsCurrentlyExercising()
        {
            if (choreDriver == null) return false;
            var currentChore = choreDriver.GetCurrentChore();
            return currentChore != null && currentChore.choreType == MorningExercisePatches.ExerciseChoreType;
        }

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
                if (exerciseWorkable.worker != null) continue;

                var operational = exerciseWorkable.GetComponent<Operational>();
                if (operational == null || !operational.IsOperational) continue;

                return true;
            }
            return false;
        }

        public bool HasAnyExerciseEquipment()
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

    // Priority 8: Waiting for equipment to become available
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
                    .ToggleAnims("anim_react_concern_kanim")
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

            public bool IsInExerciseTime() => monitor != null && monitor.IsInExerciseTime();
            public bool HasWarmUpBuff() => monitor != null && monitor.HasWarmUpBuff();
            public bool HasAvailableEquipment() => monitor != null && monitor.HasAvailableExerciseEquipment();
        }
    }
}
