using System;
using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;
using PeterHan.PLib.Options;

namespace EmptyStorage
{
	// Custom workable for emptying storage that does NOT register with the priority system on init
	// This avoids the issue where DropAllWorkable's [MyCmpAdd] Prioritizable and Prioritizable.AddRef()
	// interfere with bionic lubricant refill chores
	[AddComponentMenu("KMonoBehaviour/Workable/EmptyStorageWorkable")]
	public class EmptyStorageWorkable : Workable
	{
		[Serialize]
		private bool markedForDrop;
		
		private Chore _chore;
		private Storage[] storages;
		private Guid statusItem;
		private bool isProcessingDropAll; // Guard against double execution
		
		public float dropWorkTime = 0.1f;
		public List<Tag> removeTags;
		public bool resetTargetWorkableOnCompleteWork;
		
		// NOT using [MyCmpAdd] - we don't want automatic Prioritizable
		private Prioritizable _prioritizable;
		
		private Chore Chore
		{
			get { return this._chore; }
			set
			{
				this._chore = value;
				this.markedForDrop = (this._chore != null);
			}
		}
		
		protected EmptyStorageWorkable()
		{
			SetOffsetTable(OffsetGroups.InvertedStandardTable);
		}
		
		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			this.workerStatusItem = Db.Get().DuplicantStatusItems.Emptying;
			this.synchronizeAnims = false;
			this.faceTargetWhenWorking = true;
			SetWorkTime(this.dropWorkTime);
			
			// Use multitool animation (same as EmptyConduitWorkable / pipe emptying)
			this.multitoolContext = "build";
			this.multitoolHitEffectTag = EffectConfigs.BuildSplashId;
			
			// Set skill requirement based on options
			var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
			if (options.RequireSkills && !options.ImmediateEmptying)
			{
				this.requiredSkillPerk = Db.Get().SkillPerks.IncreaseStrengthGroundskeeper.Id;
			}
			
			// NOT calling Prioritizable.AddRef() here - this is the key difference!
		}
		
		protected override void OnSpawn()
		{
			base.OnSpawn();
			
			// Restore the chore if markedForDrop was set AND we don't already have a chore
			if (this.markedForDrop && this._chore == null)
			{
				this.DropAll();
			}
		}
		
		private Storage[] GetStorages()
		{
			if (this.storages == null)
			{
				this.storages = GetComponents<Storage>();
			}
			return this.storages;
		}
		
		public void DropAll()
		{
			// Guard against double execution (can happen due to UI refresh timing)
			if (isProcessingDropAll)
			{
				return;
			}
			
			if (DebugHandler.InstantBuildMode)
			{
				this.OnCompleteWork(null);
			}
			else if (this.Chore == null)
			{
				isProcessingDropAll = true;
				// Only add Prioritizable when actually creating a chore (on-demand)
				EnsurePrioritizable();
				
				// Calculate work time based on options
				CalculateWorkTime();
				
				var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
				ChoreType choreType = Db.Get().ChoreTypes.EmptyStorage;
				
				this.Chore = new WorkChore<EmptyStorageWorkable>(choreType, this, null, true, null, null, null, true, null, false, false, null, false, true, true, PriorityScreen.PriorityClass.basic, 5, false, true);
				
				// Enable skill perk status item if skills are required
				if (options.RequireSkills && !options.ImmediateEmptying)
				{
					SetShouldShowSkillPerkStatusItem(true);
				}
				
				isProcessingDropAll = false;
			}
			else
			{
				this.Chore.Cancel("Cancelled emptying");
				this.Chore = null;
				GetComponent<KSelectable>().RemoveStatusItem(this.workerStatusItem, false);
				ShowProgressBar(false);
				SetShouldShowSkillPerkStatusItem(false);
			}
			this.RefreshStatusItem();
		}
		
		private void EnsurePrioritizable()
		{
			// Only add Prioritizable when needed (when creating a chore)
			if (_prioritizable == null)
			{
				_prioritizable = gameObject.GetComponent<Prioritizable>();
				if (_prioritizable == null)
				{
					Prioritizable.AddRef(gameObject);
					_prioritizable = gameObject.AddOrGet<Prioritizable>();
				}
				// Set a default priority
				if (_prioritizable != null)
				{
					_prioritizable.SetMasterPriority(new PrioritySetting(PriorityScreen.PriorityClass.basic, 5));
				}
			}
		}
		
		// Called to ensure component is ready for use (called after AddOrGet if needed)
		public void EnsureInitialized()
		{
			// Make sure workerStatusItem is set (in case OnPrefabInit hasn't run)
			if (this.workerStatusItem == null)
			{
				this.workerStatusItem = Db.Get().DuplicantStatusItems.Emptying;
			}
			this.synchronizeAnims = false;
			this.faceTargetWhenWorking = true;
			SetWorkTime(this.dropWorkTime);
			
			// Use multitool animation (same as EmptyConduitWorkable / pipe emptying)
			this.multitoolContext = "build";
			this.multitoolHitEffectTag = EffectConfigs.BuildSplashId;
			
			// Set skill requirement based on options
			var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
			if (options.RequireSkills && !options.ImmediateEmptying)
			{
				this.requiredSkillPerk = Db.Get().SkillPerks.IncreaseStrengthGroundskeeper.Id;
			}
			else
			{
				this.requiredSkillPerk = null;
			}
		}
		
		private void CalculateWorkTime()
		{
			var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
			
			if (options.UseWorkTime && !options.ImmediateEmptying)
			{
				Storage[] storageArray = GetStorages();
				if (storageArray != null && storageArray.Length > 0 && storageArray[0] != null)
				{
					float massStored = storageArray[0].MassStored();
					float workTime = (massStored / 100f) * options.WorkTimePer100kg;
					if (workTime < 0.1f) workTime = 0.1f;
					this.dropWorkTime = workTime;
					SetWorkTime(workTime);
				}
			}
			else
			{
				this.dropWorkTime = 0.1f;
				SetWorkTime(0.1f);
			}
		}
		
		protected override void OnCompleteWork(WorkerBase worker)
		{
			Storage[] array = this.GetStorages();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null) continue;
				
				List<GameObject> list = new List<GameObject>(array[i].items);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j] == null) continue;
					
					GameObject gameObject = array[i].Drop(list[j], true);
					if (gameObject != null)
					{
						if (this.removeTags != null)
						{
							foreach (Tag tag in this.removeTags)
							{
								gameObject.RemoveTag(tag);
							}
						}
						gameObject.Trigger(580035959, worker);
						if (this.resetTargetWorkableOnCompleteWork)
						{
							Pickupable component = gameObject.GetComponent<Pickupable>();
							if (component != null)
							{
								component.targetWorkable = component;
								component.SetOffsetTable(OffsetGroups.InvertedStandardTable);
							}
						}
					}
				}
			}
			this.Chore = null;
			SetShouldShowSkillPerkStatusItem(false);
			this.RefreshStatusItem();
			Trigger(-1957399615, null);
		}
		
		private void RefreshStatusItem()
		{
			if (this.Chore != null && this.statusItem == Guid.Empty)
			{
				KSelectable component = GetComponent<KSelectable>();
				this.statusItem = component.AddStatusItem(Db.Get().BuildingStatusItems.AwaitingEmptyBuilding, null);
				return;
			}
			if (this.Chore == null && this.statusItem != Guid.Empty)
			{
				KSelectable component2 = GetComponent<KSelectable>();
				this.statusItem = component2.RemoveStatusItem(this.statusItem, false);
			}
		}
		
		public bool HasActiveChore()
		{
			return this.Chore != null;
		}
	}
}

