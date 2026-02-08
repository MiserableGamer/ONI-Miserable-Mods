using System;
using System.Collections.Generic;
using System.Linq;
using PeterHan.PLib.UI;
using ControlledExtraction.Options;
using UnityEngine;

namespace ControlledExtraction.UI
{
    // Modal dialog for configuring which element types the Ice Kettle can melt.
    // Opened from the mod options button. Dynamically discovers liquefiable elements
    // when in-game (via ElementLoader), falls back to a known list at the main menu.
    //
    // Vanilla/DLC elements get individual checkboxes.
    // Modded elements (from Ronivan's Legacy, etc.) share a single toggle.
    public static class IceKettleMeltablesDialog
    {
        private const string DIALOG_OK = "ok";
        private const string MODDED_KEY = "__EnableModded";

        // Tracks current checkbox states during the dialog session
        private static Dictionary<string, bool> checkStates;

        // Checkbox GameObjects keyed by their state key, for Select All / Select None
        private static Dictionary<string, GameObject> checkboxObjects;

        public static void Show(object _)
        {
            var opts = ControlledExtractionOptions.Instance.IceKettleMeltables
                ?? new IceKettleMeltableOptions();

            checkStates = new Dictionary<string, bool>();
            checkboxObjects = new Dictionary<string, GameObject>();

            var vanillaElements = new List<(string key, string displayName)>();
            bool hasModdedElements = false;

            if (ElementLoader.elements != null && ElementLoader.elements.Count > 0)
            {
                // In-game: discover all Liquifiable solid elements
                var liquefiable = ElementLoader.FindElements(
                    e => e.IsSolid && e.HasTag(GameTags.Liquifiable));

                foreach (var el in liquefiable)
                {
                    if (IsVanillaElement(el))
                    {
                        string key = el.id.ToString();
                        string name = el.tag.ProperName();
                        vanillaElements.Add((key, name));
                        checkStates[key] = opts.IsElementEnabled(key);
                    }
                    else
                    {
                        hasModdedElements = true;
                    }
                }

                vanillaElements.Sort((a, b) => string.Compare(a.displayName, b.displayName, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                // Main menu: use hardcoded fallback for common elements
                vanillaElements = GetFallbackList();
                foreach (var (key, _) in vanillaElements)
                    checkStates[key] = opts.IsElementEnabled(key);
            }

            // Always show modded toggle (other mods may add elements)
            checkStates[MODDED_KEY] = opts.EnableModdedMeltables;

            var dialog = new PDialog("IceKettleMeltables")
            {
                Title = "Meltable Types",
                Size = new Vector2(340, 500),
                DialogClosed = OnDialogClosed,
                SortKey = 300f
            };

            dialog.Body.Spacing = 4;
            dialog.Body.Direction = PanelDirection.Vertical;
            dialog.Body.Alignment = TextAnchor.UpperLeft;

            AddLabel(dialog, "Vanilla / DLC Elements");

            foreach (var (key, displayName) in vanillaElements)
                AddCheckbox(dialog, key, displayName);

            if (vanillaElements.Count == 0)
                AddLabel(dialog, "(no elements found)");

            AddLabel(dialog, "");
            AddLabel(dialog, "Modded Elements");

            string moddedTooltip = hasModdedElements
                ? "Enable all liquefiable elements added by other mods."
                : "Enable all liquefiable elements added by other mods.\nNo modded elements currently detected.";
            AddCheckbox(dialog, MODDED_KEY, "Enable modded meltables", moddedTooltip);

            if (ElementLoader.elements == null || ElementLoader.elements.Count == 0)
                AddLabel(dialog, "(Configure in-game for the full list)");

            // Select All / Select None buttons
            AddLabel(dialog, "");
            var buttonRow = new PPanel("SelectButtons")
            {
                Direction = PanelDirection.Horizontal,
                Alignment = TextAnchor.MiddleCenter,
                Spacing = 10
            };
            buttonRow.AddChild(new PButton("SelectAll")
            {
                Text = "Select All",
                OnClick = _ => SetAllCheckboxes(true)
            }.SetKleiBlueStyle());
            buttonRow.AddChild(new PButton("SelectNone")
            {
                Text = "Select None",
                OnClick = _ => SetAllCheckboxes(false)
            }.SetKleiBlueStyle());
            dialog.Body.AddChild(buttonRow);

            dialog.AddButton(DIALOG_OK, STRINGS.UI.CONFIRMDIALOG.OK, null);
            dialog.AddButton(PDialog.DIALOG_KEY_CLOSE, STRINGS.UI.CONFIRMDIALOG.CANCEL, null);
            dialog.Show();
        }

        private static void AddLabel(PDialog dialog, string text)
        {
            var label = new PLabel("Label_" + text.GetHashCode())
            {
                Text = text,
                TextStyle = PUITuning.Fonts.TextLightStyle,
                TextAlignment = TextAnchor.MiddleLeft
            };
            dialog.Body.AddChild(label);
        }

        private static void AddCheckbox(PDialog dialog, string key, string label, string tooltip = null)
        {
            bool initial = checkStates.ContainsKey(key) && checkStates[key];
            var capturedKey = key;
            var cb = new PCheckBox(key)
            {
                Text = label,
                ToolTip = tooltip ?? label,
                TextStyle = PUITuning.Fonts.TextLightStyle,
                InitialState = initial ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED,
                OnChecked = (source, state) =>
                {
                    bool nowChecked = state == PCheckBox.STATE_UNCHECKED;
                    checkStates[capturedKey] = nowChecked;
                    PCheckBox.SetCheckState(source,
                        nowChecked ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
                },
                CheckSize = new Vector2(26, 26)
            };
            cb.AddOnRealize(go => checkboxObjects[capturedKey] = go);
            dialog.Body.AddChild(cb);
        }

        private static void SetAllCheckboxes(bool check)
        {
            if (checkStates == null || checkboxObjects == null) return;

            int newState = check ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED;

            foreach (var kvp in checkboxObjects)
            {
                checkStates[kvp.Key] = check;
                if (kvp.Value != null)
                    PCheckBox.SetCheckState(kvp.Value, newState);
            }
        }

        private static void OnDialogClosed(string key)
        {
            if (key != DIALOG_OK || checkStates == null)
                return;

            var m = new IceKettleMeltableOptions();

            m.EnableModdedMeltables = GetCheck(MODDED_KEY, false);

            foreach (var kvp in checkStates)
            {
                if (kvp.Key == MODDED_KEY) continue;
                m.SetElementEnabled(kvp.Key, kvp.Value);
            }

            // Save to its own file so PLib's main dialog can't overwrite it
            m.Save();
            checkStates = null;
            checkboxObjects = null;
        }

        private static bool GetCheck(string key, bool fallback)
        {
            return checkStates.TryGetValue(key, out bool val) ? val : fallback;
        }

        // Checks if an element is from the base game or DLC (vs. a mod-added element).
        // Vanilla/DLC elements have named members in the SimHashes enum.
        private static bool IsVanillaElement(Element el)
        {
            return Enum.IsDefined(typeof(SimHashes), el.id);
        }

        // Fallback list of common liquefiable elements shown when ElementLoader
        // isn't available (main menu). Uses SimHashes names as keys.
        private static List<(string key, string displayName)> GetFallbackList()
        {
            var list = new List<(string key, string displayName)>
            {
                ("Ice", "Ice"),
                ("Snow", "Snow"),
                ("DirtyIce", "Polluted Ice"),
                ("BrineIce", "Brine Ice"),
                ("SolidCarbonDioxide", "Solid Carbon Dioxide"),
                ("SolidChlorine", "Solid Chlorine"),
                ("SolidHydrogen", "Solid Hydrogen"),
                ("SolidMethane", "Solid Methane"),
                ("SolidOxygen", "Solid Oxygen"),
            };

            if (DlcManager.IsContentSubscribed(DlcManager.EXPANSION1_ID))
                list.Add(("SolidResin", "Solid Resin"));

            if (DlcManager.IsContentSubscribed(DlcManager.DLC3_ID))
            {
                list.Add(("Gunk", "Gunk"));
                list.Add(("FrozenPhytoOil", "Frozen Phyto Oil"));
            }

            return list;
        }
    }
}
