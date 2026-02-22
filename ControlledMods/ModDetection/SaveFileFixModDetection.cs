using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using ControlledMods.Patches.SaveFileFixes;

namespace ControlledMods.ModDetection
{
    /// <summary>Detection for the 9 mods supported by Save File Fixes. Exposes Loaded and Assembly per mod.</summary>
    public static class SaveFileFixModDetection
    {
        public const string DisplayName = "Save File Fixes (target mods)";

        public static bool AdvancedCoolersLoaded => GetAssembly(AdvancedCoolersTypeName) != null;
        public static bool AiImprovementsLoaded => GetAssembly(AiImprovementsTypeName) != null;
        public static bool CustomizableSpeedLoaded => GetAssembly(CustomizableSpeedTypeName) != null;
        public static bool DefaultBuildingSettingsLoaded => GetAssembly(DefaultBuildingSettingsTypeName) != null;
        public static bool DragAreaMoreVisibleLoaded => GetAssembly(DragAreaMoreVisibleTypeName) != null;
        public static bool DrainsLoaded => GetAssembly(DrainsTypeName) != null;
        public static bool NotificationsPauseLoaded => GetAssembly(NotificationsPauseTypeName) != null;
        public static bool ToastControlLoaded => GetAssembly(ToastControlTypeName) != null;
        public static bool WallPumpsLoaded => GetAssembly(WallPumpsTypeName) != null;

        public static Assembly GetAdvancedCoolersAssembly() => GetAssembly(AdvancedCoolersTypeName);
        public static Assembly GetAiImprovementsAssembly() => GetAssembly(AiImprovementsTypeName);
        public static Assembly GetCustomizableSpeedAssembly() => GetAssembly(CustomizableSpeedTypeName);
        public static Assembly GetDefaultBuildingSettingsAssembly() => GetAssembly(DefaultBuildingSettingsTypeName);
        public static Assembly GetDragAreaMoreVisibleAssembly() => GetAssembly(DragAreaMoreVisibleTypeName);
        public static Assembly GetDrainsAssembly() => GetAssembly(DrainsTypeName);
        public static Assembly GetNotificationsPauseAssembly() => GetAssembly(NotificationsPauseTypeName);
        public static Assembly GetToastControlAssembly() => GetAssembly(ToastControlTypeName);
        public static Assembly GetWallPumpsAssembly() => GetAssembly(WallPumpsTypeName);

        public static bool IsLoaded(string modId)
        {
            switch (modId)
            {
                case SaveFileFixPaths.ModIds.AdvancedCoolers: return AdvancedCoolersLoaded;
                case SaveFileFixPaths.ModIds.AiImprovements: return AiImprovementsLoaded;
                case SaveFileFixPaths.ModIds.CustomizableSpeed: return CustomizableSpeedLoaded;
                case SaveFileFixPaths.ModIds.DefaultBuildingSettings: return DefaultBuildingSettingsLoaded;
                case SaveFileFixPaths.ModIds.DragAreaMoreVisible: return DragAreaMoreVisibleLoaded;
                case SaveFileFixPaths.ModIds.Drains: return DrainsLoaded;
                case SaveFileFixPaths.ModIds.NotificationsPause: return NotificationsPauseLoaded;
                case SaveFileFixPaths.ModIds.ToastControl: return ToastControlLoaded;
                case SaveFileFixPaths.ModIds.WallPumps: return WallPumpsLoaded;
                default: return false;
            }
        }

        public static Assembly GetAssemblyForModId(string modId)
        {
            switch (modId)
            {
                case SaveFileFixPaths.ModIds.AdvancedCoolers: return GetAdvancedCoolersAssembly();
                case SaveFileFixPaths.ModIds.AiImprovements: return GetAiImprovementsAssembly();
                case SaveFileFixPaths.ModIds.CustomizableSpeed: return GetCustomizableSpeedAssembly();
                case SaveFileFixPaths.ModIds.DefaultBuildingSettings: return GetDefaultBuildingSettingsAssembly();
                case SaveFileFixPaths.ModIds.DragAreaMoreVisible: return GetDragAreaMoreVisibleAssembly();
                case SaveFileFixPaths.ModIds.Drains: return GetDrainsAssembly();
                case SaveFileFixPaths.ModIds.NotificationsPause: return GetNotificationsPauseAssembly();
                case SaveFileFixPaths.ModIds.ToastControl: return GetToastControlAssembly();
                case SaveFileFixPaths.ModIds.WallPumps: return GetWallPumpsAssembly();
                default: return null;
            }
        }

        /// <summary>Returns the directory containing the mod's assembly (workshop/mod folder), or null.</summary>
        public static string GetModDirectory(string modId)
        {
            Assembly asm = GetAssemblyForModId(modId);
            if (asm == null) return null;
            try
            {
                string loc = asm.Location;
                if (string.IsNullOrEmpty(loc)) return null;
                return Path.GetDirectoryName(loc);
            }
            catch { return null; }
        }

        private static Assembly GetAssembly(string typeName)
        {
            try
            {
                Type type = AccessTools.TypeByName(typeName);
                return type?.Assembly;
            }
            catch { return null; }
        }

        // Type names that uniquely identify each mod's assembly (options or main mod type)
        private const string AdvancedCoolersTypeName = "Advanced_Coolers.Config";
        private const string AiImprovementsTypeName = "PeterHan.AIImprovements.AIImprovementsOptions";
        private const string CustomizableSpeedTypeName = "CustomizableSpeed.SpeedOptions";
        private const string DefaultBuildingSettingsTypeName = "DefaultBuildingSettings.Options";
        private const string DragAreaMoreVisibleTypeName = "DragAreaMoreVisible.MyMod";
        private const string DrainsTypeName = "Drains.DrainOptions";
        private const string NotificationsPauseTypeName = "NotificationsPause.Notification_IsReady_Patch";
        private const string ToastControlTypeName = "PeterHan.ToastControl.ToastControlOptions";
        private const string WallPumpsTypeName = "WallPumps.WallPumpOptions";
    }
}
