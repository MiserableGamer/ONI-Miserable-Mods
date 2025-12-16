using PeterHan.PLib.Core;

namespace CopyMaterials
{
    public static class CopyMaterialsStrings
    {
        public static class UI
        {
            public static class COPY_MATERIALS
            {
                public const string BUTTON_TEXT = "Copy Materials";
                public const string BUTTON_TOOLTIP = "Copy building material settings to other structures.";
                public const string MODE_ACTIVE = "Copy mode active";
                public const string APPLIED_SUCCESS = "Materials applied successfully";
            }
        }

        public static void Register()
        {
            LocString.CreateLocStringKeys(typeof(UI));
        }
    }
}