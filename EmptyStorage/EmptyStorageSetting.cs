using System;
using System.Collections;
using System.Reflection;
using KSerialization;
using STRINGS;
using UnityEngine;
using PeterHan.PLib.Options;

namespace EmptyStorage
{
	// Token: 0x02000004 RID: 4
	[SerializationConfig(MemberSerialization.OptIn)]
	public sealed class EmptyStorageSetting : KMonoBehaviour, ISaveLoadable
	{
		// Token: 0x06000003 RID: 3 RVA: 0x00002058 File Offset: 0x00000258
		protected override void OnCleanUp()
		{
			base.Unsubscribe(493375141);
			base.OnCleanUp();
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000206B File Offset: 0x0000026B
		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			base.Subscribe<EmptyStorageSetting>(493375141, EmptyStorageSetting.OnRefreshUserMenuDelegate);
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			// Control DropAllWorkable based on the option
			UpdateWorkableState();
		}

		private void UpdateWorkableState()
		{
			var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
			DropAllWorkable workable = base.gameObject.GetComponent<DropAllWorkable>();
			if (workable != null)
			{
				// Disable DropAllWorkable if immediate emptying is enabled
				// Enable it if immediate emptying is disabled
				workable.enabled = !options.ImmediateEmptying;
			}
		}


		private void OnRefreshUserMenu(object data)
		{
			Storage storage = base.gameObject.AddOrGet<Storage>();
			if (storage == null)
			{
				return;
			}

			// Check the option to determine if we should empty immediately or create a task
			var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
			if (options.ImmediateEmptying)
			{
				// Immediate emptying - just add the button
				KIconButtonMenu.ButtonInfo button = new KIconButtonMenu.ButtonInfo("action_empty_contents", UI.USERMENUACTIONS.EMPTYSTORAGE.NAME, delegate()
				{
					storage.DropAll(false, false, default(Vector3), true, null);
				}, Action.NumActions, null, null, null, UI.USERMENUACTIONS.EMPTYSTORAGE.TOOLTIP, true);
				Game.Instance.userMenu.AddButton(base.gameObject, button, 1f);
			}
			else
			{
				// Task-based emptying - check if there's already a chore
				DropAllWorkable workable = base.gameObject.GetComponent<DropAllWorkable>();
				if (workable == null)
				{
					Prioritizable.AddRef(base.gameObject);
					workable = base.gameObject.AddOrGet<DropAllWorkable>();
				}
				
				// Check if there's an active chore
				var choreProperty = typeof(DropAllWorkable).GetProperty("Chore", BindingFlags.NonPublic | BindingFlags.Instance);
				var chore = choreProperty?.GetValue(workable);
				bool hasChore = chore != null;
				
				// Button text and action depend on whether there's already a chore
				string buttonText = hasChore ? UI.USERMENUACTIONS.EMPTYSTORAGE.NAME_OFF : UI.USERMENUACTIONS.EMPTYSTORAGE.NAME;
				string buttonTooltip = hasChore ? UI.USERMENUACTIONS.EMPTYSTORAGE.TOOLTIP_OFF : UI.USERMENUACTIONS.EMPTYSTORAGE.TOOLTIP;
				
				KIconButtonMenu.ButtonInfo button = new KIconButtonMenu.ButtonInfo("action_empty_contents", buttonText, delegate()
				{
					if (workable == null)
					{
						Prioritizable.AddRef(base.gameObject);
						workable = base.gameObject.AddOrGet<DropAllWorkable>();
					}
					
					if (workable != null && storage != null)
					{
						// Check if there's already a chore - if so, cancel it
						var existingChore = choreProperty?.GetValue(workable);
						if (existingChore != null)
						{
							// Cancel the existing chore
							// The DropAll patch will disable shouldShowSkillPerkStatusItem and refresh the status item
							workable.DropAll(); // DropAll() toggles - if chore exists, it cancels it
						}
						else
						{
							// Check if storage has items to empty
							int itemCount = storage.items.Count;
							if (itemCount > 0)
							{
								// Enable the workable (skill requirement already set at prefab init)
								workable.enabled = true;
								
								// Ensure Prioritizable exists and set a default priority
								Prioritizable prioritizable = base.gameObject.GetComponent<Prioritizable>();
								if (prioritizable == null)
								{
									Prioritizable.AddRef(base.gameObject);
									prioritizable = base.gameObject.AddOrGet<Prioritizable>();
								}
								if (prioritizable != null)
								{
									prioritizable.SetMasterPriority(new PrioritySetting(PriorityScreen.PriorityClass.basic, 5));
								}
								
								// Create the work chore (DropAll() handles creating/canceling)
								// The DropAll patch will enable shouldShowSkillPerkStatusItem and refresh the status item
								workable.DropAll();
							}
						}
					}
				}, Action.NumActions, null, null, null, buttonTooltip, true);
				Game.Instance.userMenu.AddButton(base.gameObject, button, 1f);
			}
		}


		// Token: 0x04000001 RID: 1
		private static readonly EventSystem.IntraObjectHandler<EmptyStorageSetting> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<EmptyStorageSetting>(delegate(EmptyStorageSetting component, object data)
		{
			component.OnRefreshUserMenu(data);
		});
	}
}

