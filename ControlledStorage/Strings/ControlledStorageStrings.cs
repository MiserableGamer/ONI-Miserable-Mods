namespace ControlledStorage
{
    /// <summary>
    /// Localization strings for ControlledStorage mod.
    /// </summary>
    public static class ControlledStorageStrings
    {
        public static class UI
        {
            public static class DELIVERY_CONTROL
            {
                public static LocString TITLE = "Delivery Control";
                
                public static LocString DUPE_DEPOSIT = "Duplicants Deposit";
                public static LocString DUPE_DEPOSIT_TOOLTIP = "Allow Duplicants to deliver items to this storage";
                
                public static LocString DUPE_EXTRACT = "Duplicants Extract";
                public static LocString DUPE_EXTRACT_TOOLTIP = "Allow Duplicants to remove items from this storage";
                
                public static LocString SWEEPER_DEPOSIT = "Auto-Sweeper Deposit";
                public static LocString SWEEPER_DEPOSIT_TOOLTIP = "Allow Auto-Sweepers to deliver items to this storage";
                
                public static LocString SWEEPER_EXTRACT = "Auto-Sweeper Extract";
                public static LocString SWEEPER_EXTRACT_TOOLTIP = "Allow Auto-Sweepers to remove items from this storage";

                public static LocString COPY_DELIVERY_SETTINGS = "Copy Delivery Settings";
                public static LocString COPY_DELIVERY_SETTINGS_TOOLTIP = "Copy checkbox settings to other buildings of the same type. Does not copy filters, priority, or other settings.";
                public static LocString DELIVERY_SETTINGS_APPLIED = "Delivery settings applied";
            }
        }

        public static class BUILDING
        {
            public static class STATUSITEMS
            {
                public static class DELIVERY_RESTRICTED
                {
                    public static LocString NAME = "Delivery Restricted";
                    public static LocString TOOLTIP = "Some delivery options are disabled for this storage";
                }
            }
        }

        /// <summary>
        /// Register all strings with the game.
        /// </summary>
        public static void RegisterStrings()
        {
            // UI strings
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.TITLE", UI.DELIVERY_CONTROL.TITLE);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.DUPE_DEPOSIT", UI.DELIVERY_CONTROL.DUPE_DEPOSIT);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.DUPE_DEPOSIT_TOOLTIP", UI.DELIVERY_CONTROL.DUPE_DEPOSIT_TOOLTIP);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.DUPE_EXTRACT", UI.DELIVERY_CONTROL.DUPE_EXTRACT);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.DUPE_EXTRACT_TOOLTIP", UI.DELIVERY_CONTROL.DUPE_EXTRACT_TOOLTIP);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.SWEEPER_DEPOSIT", UI.DELIVERY_CONTROL.SWEEPER_DEPOSIT);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.SWEEPER_DEPOSIT_TOOLTIP", UI.DELIVERY_CONTROL.SWEEPER_DEPOSIT_TOOLTIP);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.SWEEPER_EXTRACT", UI.DELIVERY_CONTROL.SWEEPER_EXTRACT);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.SWEEPER_EXTRACT_TOOLTIP", UI.DELIVERY_CONTROL.SWEEPER_EXTRACT_TOOLTIP);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.COPY_DELIVERY_SETTINGS", UI.DELIVERY_CONTROL.COPY_DELIVERY_SETTINGS);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.COPY_DELIVERY_SETTINGS_TOOLTIP", UI.DELIVERY_CONTROL.COPY_DELIVERY_SETTINGS_TOOLTIP);
            Strings.Add("STRINGS.UI.UISIDESCREENS.DELIVERYCONTROL.DELIVERY_SETTINGS_APPLIED", UI.DELIVERY_CONTROL.DELIVERY_SETTINGS_APPLIED);

            // Status item strings
            Strings.Add("STRINGS.BUILDING.STATUSITEMS.DELIVERY_RESTRICTED.NAME", BUILDING.STATUSITEMS.DELIVERY_RESTRICTED.NAME);
            Strings.Add("STRINGS.BUILDING.STATUSITEMS.DELIVERY_RESTRICTED.TOOLTIP", BUILDING.STATUSITEMS.DELIVERY_RESTRICTED.TOOLTIP);

        }
    }
}
