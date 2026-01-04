using System;
using System.Collections;
using System.Reflection;
using KSerialization;
using STRINGS;
using UnityEngine;
using PeterHan.PLib.Options;

namespace EmptyStorage
{
	[SerializationConfig(MemberSerialization.OptIn)]
	public sealed class EmptyStorageSetting : KMonoBehaviour, ISaveLoadable
	{
		protected override void OnCleanUp()
		{
			base.Unsubscribe(493375141);
			base.OnCleanUp();
		}

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			base.Subscribe<EmptyStorageSetting>(493375141, EmptyStorageSetting.OnRefreshUserMenuDelegate);
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
		}

		private void OnRefreshUserMenu(object data)
		{
			Storage storage = base.gameObject.GetComponent<Storage>();
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
				// Task-based emptying - use our custom workable
				EmptyStorageWorkable workable = base.gameObject.GetComponent<EmptyStorageWorkable>();
				bool hasChore = workable != null && workable.HasActiveChore();
				
				// Button text and action depend on whether there's already a chore
				string buttonText = hasChore ? UI.USERMENUACTIONS.EMPTYSTORAGE.NAME_OFF : UI.USERMENUACTIONS.EMPTYSTORAGE.NAME;
				string buttonTooltip = hasChore ? UI.USERMENUACTIONS.EMPTYSTORAGE.TOOLTIP_OFF : UI.USERMENUACTIONS.EMPTYSTORAGE.TOOLTIP;
				
				KIconButtonMenu.ButtonInfo button = new KIconButtonMenu.ButtonInfo("action_empty_contents", buttonText, delegate()
				{
					// Get or create the workable on-demand
					EmptyStorageWorkable w = base.gameObject.GetComponent<EmptyStorageWorkable>();
					if (w == null)
					{
						w = base.gameObject.AddOrGet<EmptyStorageWorkable>();
						// Ensure the workable is initialized properly
						w.EnsureInitialized();
					}
					
					if (w != null && storage != null)
					{
						// Check if there's already a chore - if so, cancel it
						if (w.HasActiveChore())
						{
							w.DropAll(); // DropAll() toggles - if chore exists, it cancels it
						}
						else
						{
							// Check if storage has items to empty
							int itemCount = storage.items.Count;
							if (itemCount > 0)
							{
								w.enabled = true;
								w.EnsureInitialized(); // Ensure initialized before creating chore
								w.DropAll();
							}
						}
					}
				}, Action.NumActions, null, null, null, buttonTooltip, true);
				Game.Instance.userMenu.AddButton(base.gameObject, button, 1f);
			}
		}

		private static readonly EventSystem.IntraObjectHandler<EmptyStorageSetting> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<EmptyStorageSetting>(delegate(EmptyStorageSetting component, object data)
		{
			component.OnRefreshUserMenu(data);
		});
	}
}
