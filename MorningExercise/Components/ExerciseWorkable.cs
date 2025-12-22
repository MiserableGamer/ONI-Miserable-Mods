// Toggle this flag to enable/disable experience debug logging
// Comment out or remove the #define to disable logging
#define DEBUG_EXPERIENCE

using System.Collections.Generic;
using System.Reflection;
using Database;
using Klei.AI;
using PeterHan.PLib.Options;
using UnityEngine;

namespace MorningExercise
{
    public class ExerciseWorkable : Workable
    {
        // Registry of all exercise workables for quick lookup
        public static HashSet<ExerciseWorkable> AllExerciseWorkables { get; } = new HashSet<ExerciseWorkable>();

        private Chore chore;

        // Only allow this chore on Manual Exerciser buildings (not regular generators)
        private static readonly Chore.Precondition IsManualExerciserPrecondition = new Chore.Precondition
        {
            id = "MorningExercise.IsManualExerciser",
            description = "Not a Manual Exerciser",
            fn = delegate(ref Chore.Precondition.Context context, object data)
            {
                var workable = data as ExerciseWorkable;
                if (workable == null) return false;
                
                var prefabId = workable.GetComponent<KPrefabID>();
                return prefabId != null && prefabId.PrefabTag.Name == ManualExerciserConfig.ID;
            }
        };

        // Don't allow exercise if dupe type is disabled
        private static readonly Chore.Precondition ExerciseEnabledForDupeType = new Chore.Precondition
        {
            id = "MorningExercise.ExerciseEnabledForDupeType",
            description = "Exercise disabled for this dupe type",
            fn = delegate(ref Chore.Precondition.Context context, object data)
            {
                var consumer = context.consumerState.consumer;
                if (consumer == null) return false;
                
                // Get the GameObject from ChoreConsumer (which is a Component)
                GameObject dupe = null;
                
                // ChoreConsumer is a Component, so it has a gameObject property
                if (consumer is Component comp)
                {
                    dupe = comp.gameObject;
                }
                // Fallback: try to get gameObject via reflection
                else
                {
                    var gameObjectProp = consumer.GetType().GetProperty("gameObject", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (gameObjectProp != null)
                    {
                        dupe = gameObjectProp.GetValue(consumer) as GameObject;
                    }
                }
                
                if (dupe == null) return false;
                
                bool isBionic = IsBionicDupe(dupe);
                var options = POptions.ReadSettings<Options.MorningExerciseOptions>() ?? new Options.MorningExerciseOptions();
                
                return (isBionic && options.EnableBionicExercise) || (!isBionic && options.EnableStandardExercise);
            }
        };

        // Don't allow exercise if dupe already has the buff
        private static readonly Chore.Precondition DoesNotHaveWarmUpBuff = new Chore.Precondition
        {
            id = "MorningExercise.DoesNotHaveWarmUpBuff",
            description = "Already warmed up",
            fn = delegate(ref Chore.Precondition.Context context, object data)
            {
                var effects = context.consumerState.consumer.GetComponent<Effects>();
                if (effects == null) return true;
                
                return !effects.HasEffect(MorningExercisePatches.WARMUP_EFFECT_ID) &&
                       !effects.HasEffect(MorningExercisePatches.BIONIC_WARMUP_EFFECT_ID);
            }
        };

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            
            // Use the original interaction animation
            var anim = Assets.GetAnim("anim_interacts_generatormanual_kanim");
            if (anim != null)
            {
                overrideAnims = new KAnimFile[] { anim };
            }
            synchronizeAnims = false;
            
            // Get exercise duration from options
            var options = POptions.ReadSettings<Options.MorningExerciseOptions>() ?? new Options.MorningExerciseOptions();
            SetWorkTime(options.ExerciseDuration);
            resetProgressOnStop = false;
            showProgressBar = true;
            lightEfficiencyBonus = false;
            
            // Try to set attribute experience multiplier via reflection (like ManualGenerator)
            if (options.EnableExperienceGain)
            {
                try
                {
                    var attributeExperienceField = typeof(Workable).GetField("attributeExperienceMultiplier", 
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (attributeExperienceField != null)
                    {
                        attributeExperienceField.SetValue(this, options.ExperienceMultiplier);
                    }
                    
                    // Also try setting the attribute ID for experience
                    var attributeExperienceIdField = typeof(Workable).GetField("attributeExperienceId", 
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (attributeExperienceIdField != null)
                    {
                        attributeExperienceIdField.SetValue(this, Db.Get().Attributes.Athletics.Id);
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[MorningExercise] Could not set Workable experience properties: {ex.Message}");
                }
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            AllExerciseWorkables.Add(this);
            CreateChore();
        }

        protected override void OnCleanUp()
        {
            AllExerciseWorkables.Remove(this);
            CancelChore();
            base.OnCleanUp();
        }

        private void CreateChore()
        {
            if (chore != null) return;
            if (MorningExercisePatches.ExerciseChoreType == null) return;
            if (MorningExercisePatches.ExerciseBlock == null) return;

            chore = new WorkChore<ExerciseWorkable>(
                MorningExercisePatches.ExerciseChoreType,
                this,
                null,
                true,
                null, null, null,
                false,
                MorningExercisePatches.ExerciseBlock,
                false,
                true,
                null,
                false,
                true,
                true,
                PriorityScreen.PriorityClass.personalNeeds,
                5,
                false,
                true
            );
            
            chore.AddPrecondition(IsManualExerciserPrecondition, this);
            chore.AddPrecondition(ExerciseEnabledForDupeType, null);
            chore.AddPrecondition(DoesNotHaveWarmUpBuff, null);
        }

        private void CancelChore()
        {
            if (chore != null)
            {
                chore.Cancel("Cleanup");
                chore = null;
            }
        }

        protected override void OnStartWork(WorkerBase worker)
        {
            base.OnStartWork(worker);
            
            // Activate the building so it shows as operational
            var operational = GetComponent<Operational>();
            if (operational != null)
            {
                operational.SetActive(true, false);
            }
            
            // Play the working animation
            var kbac = GetComponent<KBatchedAnimController>();
            if (kbac != null)
            {
                if (kbac.HasAnimation("working"))
                {
                    kbac.Play("working", KAnim.PlayMode.Loop);
                }
                else if (kbac.HasAnimation("working_loop"))
                {
                    kbac.Play("working_loop", KAnim.PlayMode.Loop);
                }
                else if (kbac.HasAnimation("on"))
                {
                    kbac.Play("on", KAnim.PlayMode.Loop);
                }
                else
                {
                    kbac.Play("working", KAnim.PlayMode.Loop);
                }
            }
        }

        protected override void OnStopWork(WorkerBase worker)
        {
            base.OnStopWork(worker);
            
            var operational = GetComponent<Operational>();
            if (operational != null)
            {
                operational.SetActive(false, false);
            }
            
            var kbac = GetComponent<KBatchedAnimController>();
            if (kbac != null)
            {
                kbac.Play("off", KAnim.PlayMode.Once);
            }
        }

        protected override void OnCompleteWork(WorkerBase worker)
        {
            base.OnCompleteWork(worker);
            
            var options = POptions.ReadSettings<Options.MorningExerciseOptions>() ?? new Options.MorningExerciseOptions();
            
            // Grant Athletics experience if enabled (same as Manual Generator: 1 exp per second)
            if (options.EnableExperienceGain)
            {
                // Use AttributeLevels component to add experience (this is the correct way)
                var attributeLevels = worker.gameObject.GetComponent<Klei.AI.AttributeLevels>();
                if (attributeLevels != null)
                {
                    float timeSpent = options.ExerciseDuration;
                    float multiplier = options.ExperienceMultiplier;
                    
                    // Get the Athletics attribute
                    var athleticsAttr = Db.Get().Attributes.Athletics;
                    string attributeId = athleticsAttr.Id.ToString();
                    
#if DEBUG_EXPERIENCE
                    UnityEngine.Debug.Log($"[MorningExercise] Attempting to grant experience. Attribute ID: {attributeId}, IsTrainable: {athleticsAttr.IsTrainable}, MaxAttributeLevel: {attributeLevels.maxAttributeLevel}");
#endif
                    
                    // Check if the attribute level exists
                    var attributeLevel = attributeLevels.GetAttributeLevel(attributeId);
                    if (attributeLevel == null)
                    {
#if DEBUG_EXPERIENCE
                        UnityEngine.Debug.LogWarning($"[MorningExercise] Athletics attribute level not found. Listing all attribute levels:");
                        foreach (var level in attributeLevels)
                        {
                            UnityEngine.Debug.LogWarning($"[MorningExercise]   - {level.attribute.Attribute.Id} (trainable: {level.attribute.Attribute.IsTrainable})");
                        }
#endif
                    }
                    else
                    {
#if DEBUG_EXPERIENCE
                        float experienceBefore = attributeLevel.experience;
                        float experienceToAdd = timeSpent * multiplier;
                        
                        UnityEngine.Debug.Log($"[MorningExercise] Found Athletics attribute level. Current level: {attributeLevel.GetLevel()}, Experience: {experienceBefore}, Max level: {attributeLevel.maxGainedLevel}");
                        UnityEngine.Debug.Log($"[MorningExercise] Granting {experienceToAdd} experience ({timeSpent}s * {multiplier})");
#endif
                        
                        // AddExperience takes: attribute_id, time_spent, multiplier
                        // Note: Returns true only if level up occurred, but experience is always added
                        bool leveledUp = attributeLevels.AddExperience(attributeId, timeSpent, multiplier);
                        
#if DEBUG_EXPERIENCE
                        float experienceAfter = attributeLevel.experience;
                        float actualExperienceAdded = experienceAfter - experienceBefore;
                        
                        if (leveledUp)
                        {
                            UnityEngine.Debug.Log($"[MorningExercise] Level up! New level: {attributeLevel.GetLevel()}, Experience: {experienceAfter}");
                        }
                        else
                        {
                            UnityEngine.Debug.Log($"[MorningExercise] Experience added: {actualExperienceAdded} (total: {experienceAfter}). Level up will occur at {attributeLevel.GetExperienceForNextLevel()} experience.");
                        }
#endif
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("[MorningExercise] Could not find AttributeLevels component on worker");
                }
            }
            
            // Give the warm up buff
            var effects = worker.GetComponent<Effects>();
            if (effects != null)
            {
                bool isBionic = IsBionicDupe(worker.gameObject);
                string effectId = isBionic ? 
                    MorningExercisePatches.BIONIC_WARMUP_EFFECT_ID : 
                    MorningExercisePatches.WARMUP_EFFECT_ID;
                
                effects.Add(effectId, true);
            }

            chore = null;
            CreateChore();
        }

        public bool IsManualExerciser()
        {
            var prefabId = GetComponent<KPrefabID>();
            return prefabId != null && prefabId.PrefabTag.Name == ManualExerciserConfig.ID;
        }

        private static bool IsBionicDupe(GameObject dupe)
        {
            if (dupe == null) return false;
            
            var smc = dupe.GetComponent<StateMachineController>();
            if (smc != null)
            {
                if (smc.GetSMI<BionicUpgradesMonitor.Instance>() != null) return true;
                if (smc.GetSMI<BionicBatteryMonitor.Instance>() != null) return true;
            }
            
            var prefabId = dupe.GetComponent<KPrefabID>();
            if (prefabId != null && prefabId.PrefabTag.Name.Contains("Bionic"))
            {
                return true;
            }
            
            return false;
        }
    }
}
