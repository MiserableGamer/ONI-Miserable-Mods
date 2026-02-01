using KSerialization;
using STRINGS;

namespace ControlledStorage
{
    /// <summary>
    /// Component that adds the "Empty Storage" button to the user menu.
    /// Handles both immediate emptying and task-based emptying based on options.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class EmptyStorageSetting : KMonoBehaviour, ISaveLoadable
    {
        private static readonly EventSystem.IntraObjectHandler<EmptyStorageSetting> OnRefreshUserMenuDelegate =
            new EventSystem.IntraObjectHandler<EmptyStorageSetting>(
                (component, data) => component.OnRefreshUserMenu(data)
            );

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(493375141, OnRefreshUserMenuDelegate);
        }

        protected override void OnCleanUp()
        {
            Unsubscribe(493375141);
            base.OnCleanUp();
        }

        private void OnRefreshUserMenu(object data)
        {
            var storage = gameObject.GetComponent<Storage>();
            if (storage == null)
                return;

            var options = ControlledStorageOptions.Instance;

            if (options.ImmediateEmptying)
            {
                AddImmediateEmptyButton(storage);
            }
            else
            {
                AddTaskEmptyButton(storage);
            }
        }

        private void AddImmediateEmptyButton(Storage storage)
        {
            var button = new KIconButtonMenu.ButtonInfo(
                "action_empty_contents",
                STRINGS.UI.USERMENUACTIONS.EMPTYSTORAGE.NAME,
                () => storage.DropAll(false, false, default, true, null),
                Action.NumActions,
                null, null, null,
                STRINGS.UI.USERMENUACTIONS.EMPTYSTORAGE.TOOLTIP,
                true
            );

            Game.Instance.userMenu.AddButton(gameObject, button, 1f);
        }

        private void AddTaskEmptyButton(Storage storage)
        {
            var workable = gameObject.GetComponent<EmptyStorageWorkable>();
            bool hasChore = workable != null && workable.HasActiveChore();

            string buttonText = hasChore
                ? STRINGS.UI.USERMENUACTIONS.EMPTYSTORAGE.NAME_OFF
                : STRINGS.UI.USERMENUACTIONS.EMPTYSTORAGE.NAME;

            string buttonTooltip = hasChore
                ? STRINGS.UI.USERMENUACTIONS.EMPTYSTORAGE.TOOLTIP_OFF
                : STRINGS.UI.USERMENUACTIONS.EMPTYSTORAGE.TOOLTIP;

            var button = new KIconButtonMenu.ButtonInfo(
                "action_empty_contents",
                buttonText,
                () => OnEmptyButtonPressed(storage),
                Action.NumActions,
                null, null, null,
                buttonTooltip,
                true
            );

            Game.Instance.userMenu.AddButton(gameObject, button, 1f);
        }

        private void OnEmptyButtonPressed(Storage storage)
        {
            // Get or create workable on-demand
            var workable = gameObject.GetComponent<EmptyStorageWorkable>();
            if (workable == null)
            {
                workable = gameObject.AddOrGet<EmptyStorageWorkable>();
                workable.InitializeWorkable();
            }

            if (workable.HasActiveChore())
            {
                // Toggle off - cancel the chore
                workable.DropAll();
            }
            else if (storage.items.Count > 0)
            {
                // Create new chore if there are items to empty
                workable.enabled = true;
                workable.InitializeWorkable();
                workable.DropAll();
            }
        }
    }
}
