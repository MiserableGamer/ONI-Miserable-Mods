using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

namespace ControlledStorage
{
    /// <summary>
    /// Custom workable for emptying storage that does NOT register with the priority system on init.
    /// This avoids issues where DropAllWorkable's automatic Prioritizable registration
    /// interferes with bionic lubricant refill chores.
    /// </summary>
    [AddComponentMenu("KMonoBehaviour/Workable/EmptyStorageWorkable")]
    public class EmptyStorageWorkable : Workable
    {
        [Serialize]
        private bool markedForDrop;

        private Chore _chore;
        private Storage[] storages;
        private Guid statusItem;
        private Guid bionicStatusItem;
        private bool isProcessingDropAll;
        private Prioritizable _prioritizable;
        
        private static StatusItem _bionicBoosterStatusItem;
        private static StatusItem _dupeSkillStatusItem;
        private Guid dupeSkillStatusItem;

        public float dropWorkTime = 0.1f;
        public List<Tag> removeTags;
        public bool resetTargetWorkableOnCompleteWork;

        private Chore Chore
        {
            get => _chore;
            set
            {
                _chore = value;
                markedForDrop = (_chore != null);
            }
        }

        protected EmptyStorageWorkable()
        {
            SetOffsetTable(OffsetGroups.InvertedStandardTable);
        }

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            InitializeWorkable();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            // Restore chore if markedForDrop was saved and we don't have one
            if (markedForDrop && _chore == null)
            {
                DropAll();
            }
        }

        /// <summary>
        /// Initialize or reinitialize workable settings based on current options.
        /// </summary>
        public void InitializeWorkable()
        {
            workerStatusItem = Db.Get().DuplicantStatusItems.Emptying;
            synchronizeAnims = false;
            faceTargetWhenWorking = true;
            SetWorkTime(dropWorkTime);

            // Use multitool animation (same as pipe emptying)
            multitoolContext = "build";
            multitoolHitEffectTag = EffectConfigs.BuildSplashId;

            // Set skill requirement based on options
            var options = ControlledStorageOptions.Instance;
            requiredSkillPerk = (options.RequireSkills && !options.ImmediateEmptying)
                ? Db.Get().SkillPerks.IncreaseStrengthGroundskeeper.Id
                : null;
            
            // Initialize custom status items
            InitializeBionicStatusItem();
            InitializeDupeSkillStatusItem();
        }
        
        private static void InitializeBionicStatusItem()
        {
            if (_bionicBoosterStatusItem != null)
                return;
            
            // Register strings - format to match skill requirement warnings
            Strings.Add("STRINGS.BUILDING.STATUSITEMS.CONTROLLEDSTORAGE_BIONICBOOSTERHINT.NAME", 
                "Bionics Lack Tidying Booster");
            Strings.Add("STRINGS.BUILDING.STATUSITEMS.CONTROLLEDSTORAGE_BIONICBOOSTERHINT.TOOLTIP", 
                "<b>Bionic Duplicants</b> can perform this task by installing the <b>Tidying Booster</b> upgrade.\n\nOpen a Bionic's <b>Upgrades Panel</b> to install the booster.");
            
            // Use status_item_role_required (same icon as skill requirement warnings)
            _bionicBoosterStatusItem = new StatusItem(
                "ControlledStorage_BionicBoosterHint", 
                "BUILDING", 
                "status_item_role_required",  // Hard hat icon
                StatusItem.IconType.Custom,
                NotificationType.BadMinor,    // Red color
                false,
                OverlayModes.None.ID);
        }
        
        private static void InitializeDupeSkillStatusItem()
        {
            if (_dupeSkillStatusItem != null)
                return;
            
            // Register strings - parallel to bionic warning style
            Strings.Add("STRINGS.BUILDING.STATUSITEMS.CONTROLLEDSTORAGE_DUPESKILLHINT.NAME", 
                "Duplicants Lack Improved Strength Skill");
            Strings.Add("STRINGS.BUILDING.STATUSITEMS.CONTROLLEDSTORAGE_DUPESKILLHINT.TOOLTIP", 
                "<b>Duplicants</b> can perform this task by learning the <b>Improved Strength</b> skill.\n\nOpen the <b>Skills Panel</b> to assign the skill.");
            
            // Use status_item_role_required (same icon as skill requirement warnings)
            _dupeSkillStatusItem = new StatusItem(
                "ControlledStorage_DupeSkillHint", 
                "BUILDING", 
                "status_item_role_required",  // Hard hat icon
                StatusItem.IconType.Custom,
                NotificationType.BadMinor,    // Red color
                false,
                OverlayModes.None.ID);
        }
        
        /// <summary>
        /// Check if any non-bionic dupes lack the Groundskeeper skill perk.
        /// </summary>
        private static bool HasDupesWithoutSkill()
        {
            var skillPerkId = Db.Get().SkillPerks.IncreaseStrengthGroundskeeper.Id;
            
            foreach (var minionIdentity in Components.LiveMinionIdentities.Items)
            {
                if (minionIdentity == null) continue;
                
                // Skip bionics - they have separate handling
                var bionicUpgrades = minionIdentity.GetSMI<BionicUpgradesMonitor.Instance>();
                if (bionicUpgrades != null) continue;
                
                // Check if this dupe has the required skill perk
                var resume = minionIdentity.GetComponent<MinionResume>();
                if (resume != null && !resume.HasPerk(skillPerkId))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if there are any bionic dupes in the colony without the Tidying Booster.
        /// </summary>
        private static bool HasBionicsWithoutTidyingBooster()
        {
            Tag tidyingBoosterTag = new Tag("Booster_Tidy1");
            
            foreach (var minionIdentity in Components.LiveMinionIdentities.Items)
            {
                if (minionIdentity == null) continue;
                
                // Use GetSMI for state machine instances, not GetComponent
                var bionicUpgrades = minionIdentity.GetSMI<BionicUpgradesMonitor.Instance>();
                if (bionicUpgrades == null) continue;  // Not a bionic
                
                // This is a bionic - check if it has the Tidying Booster
                if (bionicUpgrades.CountBoosterAssignments(tidyingBoosterTag) == 0)
                {
                    // Found a bionic without the booster
                    return true;
                }
            }
            
            return false;
        }

        private Storage[] GetStorages()
        {
            return storages ??= GetComponents<Storage>();
        }

        /// <summary>
        /// Toggle emptying - creates chore if none exists, cancels if one does.
        /// </summary>
        public void DropAll()
        {
            // Guard against double execution
            if (isProcessingDropAll)
                return;

            if (DebugHandler.InstantBuildMode)
            {
                OnCompleteWork(null);
                return;
            }

            if (Chore == null)
            {
                CreateEmptyChore();
            }
            else
            {
                CancelEmptyChore();
            }

            RefreshStatusItem();
        }

        private void CreateEmptyChore()
        {
            isProcessingDropAll = true;

            try
            {
                // Add Prioritizable on-demand (only when creating chore)
                EnsurePrioritizable();
                CalculateWorkTime();

                var options = ControlledStorageOptions.Instance;
                ChoreType choreType = Db.Get().ChoreTypes.EmptyStorage;

                Chore = new WorkChore<EmptyStorageWorkable>(
                    choreType, this, null, true, null, null, null, true, null,
                    false, false, null, false, true, true,
                    PriorityScreen.PriorityClass.basic, 5, false, true
                );

                // Note: We don't call SetShouldShowSkillPerkStatusItem - our custom 
                // status items in RefreshStatusItem replace the vanilla colony-wide warning
            }
            finally
            {
                isProcessingDropAll = false;
            }
        }

        private void CancelEmptyChore()
        {
            Chore.Cancel("Cancelled emptying");
            Chore = null;
            GetComponent<KSelectable>().RemoveStatusItem(workerStatusItem, false);
            ShowProgressBar(false);
            // Status items are cleaned up in RefreshStatusItem
        }

        private void EnsurePrioritizable()
        {
            if (_prioritizable != null)
                return;

            _prioritizable = gameObject.GetComponent<Prioritizable>();
            if (_prioritizable == null)
            {
                Prioritizable.AddRef(gameObject);
                _prioritizable = gameObject.AddOrGet<Prioritizable>();
            }

            _prioritizable?.SetMasterPriority(new PrioritySetting(PriorityScreen.PriorityClass.basic, 5));
        }

        private void CalculateWorkTime()
        {
            var options = ControlledStorageOptions.Instance;

            if (!options.UseWorkTime || options.ImmediateEmptying)
            {
                dropWorkTime = 0.1f;
                SetWorkTime(0.1f);
                return;
            }

            var storageArray = GetStorages();
            if (storageArray != null && storageArray.Length > 0 && storageArray[0] != null)
            {
                float massStored = storageArray[0].MassStored();
                float workTime = Math.Max(0.1f, (massStored / 100f) * options.WorkTimePer100kg);
                dropWorkTime = workTime;
                SetWorkTime(workTime);
            }
        }

        public override void OnCompleteWork(WorkerBase worker)
        {
            var storageArray = GetStorages();

            foreach (var storage in storageArray)
            {
                if (storage == null) continue;

                var items = new List<GameObject>(storage.items);
                foreach (var item in items)
                {
                    if (item == null) continue;

                    GameObject dropped = storage.Drop(item, true);
                    if (dropped != null)
                    {
                        // Remove specified tags
                        if (removeTags != null)
                        {
                            foreach (Tag tag in removeTags)
                            {
                                dropped.RemoveTag(tag);
                            }
                        }

                        dropped.Trigger(580035959, worker);

                        if (resetTargetWorkableOnCompleteWork)
                        {
                            var pickupable = dropped.GetComponent<Pickupable>();
                            if (pickupable != null)
                            {
                                pickupable.targetWorkable = pickupable;
                                pickupable.SetOffsetTable(OffsetGroups.InvertedStandardTable);
                            }
                        }
                    }
                }
            }

            Chore = null;
            RefreshStatusItem();  // Cleans up all custom status items
            Trigger(-1957399615, null);
        }

        private void RefreshStatusItem()
        {
            var kSelectable = GetComponent<KSelectable>();
            var options = ControlledStorageOptions.Instance;
            bool requiresSkill = options.RequireSkills && !options.ImmediateEmptying;

            if (Chore != null && statusItem == Guid.Empty)
            {
                statusItem = kSelectable.AddStatusItem(Db.Get().BuildingStatusItems.AwaitingEmptyBuilding, null);
                
                if (requiresSkill)
                {
                    // Add bionic booster hint if bionics exist without the booster
                    if (_bionicBoosterStatusItem != null && 
                        bionicStatusItem == Guid.Empty && HasBionicsWithoutTidyingBooster())
                    {
                        bionicStatusItem = kSelectable.AddStatusItem(_bionicBoosterStatusItem, null);
                    }
                    
                    // Add dupe skill hint if non-bionics lack the skill
                    if (_dupeSkillStatusItem != null && 
                        dupeSkillStatusItem == Guid.Empty && HasDupesWithoutSkill())
                    {
                        dupeSkillStatusItem = kSelectable.AddStatusItem(_dupeSkillStatusItem, null);
                    }
                }
            }
            else if (Chore == null && statusItem != Guid.Empty)
            {
                statusItem = kSelectable.RemoveStatusItem(statusItem, false);
                
                // Remove bionic hint
                if (bionicStatusItem != Guid.Empty)
                {
                    bionicStatusItem = kSelectable.RemoveStatusItem(bionicStatusItem, false);
                }
                
                // Remove dupe skill hint
                if (dupeSkillStatusItem != Guid.Empty)
                {
                    dupeSkillStatusItem = kSelectable.RemoveStatusItem(dupeSkillStatusItem, false);
                }
            }
        }

        public bool HasActiveChore() => Chore != null;
        
        /// <summary>
        /// Override the vanilla UpdateStatusItem to prevent showing "Colony Lacks Skill" warning.
        /// We show our own custom status items in RefreshStatusItem instead.
        /// </summary>
        public override void UpdateStatusItem(object data = null)
        {
            // Don't call base - we handle skill status items ourselves in RefreshStatusItem
            // This prevents the vanilla "Local Colony Lacks Required Skill" from appearing
        }
    }
}
