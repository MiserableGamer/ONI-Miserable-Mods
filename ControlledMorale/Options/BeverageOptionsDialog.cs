using PeterHan.PLib.Options;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ControlledMorale
{
    internal static class BeverageOptionsDialog
    {
        public static void Show(string title, BeverageModifiers modifiers)
        {
            var options = ControlledMoraleOptions.Instance;

            var defs       = new List<AttributeDef>(GetAttributeDefs(modifiers));
            var checkboxGOs = new List<GameObject>(new GameObject[defs.Count]);
            var sliderGOs   = new List<GameObject>(new GameObject[defs.Count]);
            var labelGOs    = new List<GameObject>(new GameObject[defs.Count]);

            var grid = new PGridPanel("Attributes") { FlexSize = Vector2.right };
            // Column 0: checkbox+label (auto); Column 1: slider (flexible); Column 2: value (fixed)
            grid.AddColumn(new GridColumnSpec()).AddColumn(new GridColumnSpec(flex: 1.0f))
                .AddColumn(new GridColumnSpec(width: 38f));

            for (int i = 0; i < defs.Count; i++)
            {
                int idx = i;
                var def = defs[idx];

                grid.AddRow(new GridRowSpec());

                var checkbox = new PCheckBox("check_" + def.Id)
                {
                    Text = def.Label,
                    CheckSize = new Vector2(16f, 16f),
                    InitialState = def.GetEnabled()
                        ? PCheckBox.STATE_CHECKED
                        : PCheckBox.STATE_UNCHECKED,
                    OnChecked = (source, state) =>
                    {
                        bool nowEnabled = state == PCheckBox.STATE_UNCHECKED;
                        defs[idx].SetEnabled(nowEnabled);
                        PCheckBox.SetCheckState(source, nowEnabled
                            ? PCheckBox.STATE_CHECKED
                            : PCheckBox.STATE_UNCHECKED);
                    }
                };
                if (def.Tooltip != null)
                    checkbox.ToolTip = def.Tooltip;
                checkbox.AddOnRealize(go => checkboxGOs[idx] = go);

                grid.AddChild(checkbox, new GridComponentSpec(idx, 0) { Margin = new RectOffset(2, 8, 2, 2), Alignment = TextAnchor.MiddleLeft });

                var valueLabel = new PLabel("val_" + def.Id)
                {
                    Text = def.GetValue().ToString(),
                    TextAlignment = TextAnchor.MiddleCenter
                };
                valueLabel.AddOnRealize(go => labelGOs[idx] = go);

                grid.AddChild(
                    new PSliderSingle("slider_" + def.Id)
                    {
                        MinValue = -10f,
                        MaxValue = 10f,
                        InitialValue = (float)def.GetValue(),
                        IntegersOnly = true,
                        FlexSize = Vector2.right,
                        OnValueChanged = (_, val) =>
                        {
                            int intVal = (int)Math.Round(val);
                            defs[idx].SetValue(intVal);
                            if (labelGOs[idx] != null)
                                labelGOs[idx].GetComponentInChildren<LocText>().text = intVal.ToString();
                        }
                    }.AddOnRealize(go => sliderGOs[idx] = go),
                    new GridComponentSpec(idx, 1) { Margin = new RectOffset(2, 2, 2, 2) });

                grid.AddChild(valueLabel, new GridComponentSpec(idx, 2) { Margin = new RectOffset(2, 4, 2, 2) });
            }

            // ── Button row ────────────────────────────────────────────────────────
            var btnRow = new PPanel("BtnRow")
            {
                Direction = PanelDirection.Horizontal,
                FlexSize  = Vector2.right,
                Spacing   = 4,
                Margin    = new RectOffset(2, 2, 4, 4)
            };
            btnRow.AddChild(new PButton("allOn")
            {
                Text      = "All On",
                FlexSize  = Vector2.right,
                OnClick   = _ =>
                {
                    for (int i = 0; i < defs.Count; i++)
                    {
                        defs[i].SetEnabled(true);
                        if (checkboxGOs[i] != null)
                            PCheckBox.SetCheckState(checkboxGOs[i], PCheckBox.STATE_CHECKED);
                    }
                }
            });
            btnRow.AddChild(new PButton("allOff")
            {
                Text     = "All Off",
                FlexSize = Vector2.right,
                OnClick  = _ =>
                {
                    for (int i = 0; i < defs.Count; i++)
                    {
                        defs[i].SetEnabled(false);
                        if (checkboxGOs[i] != null)
                            PCheckBox.SetCheckState(checkboxGOs[i], PCheckBox.STATE_UNCHECKED);
                    }
                }
            });
            btnRow.AddChild(new PButton("reset")
            {
                Text     = "Reset to Zero",
                FlexSize = Vector2.right,
                OnClick  = _ =>
                {
                    for (int i = 0; i < defs.Count; i++)
                    {
                        defs[i].SetValue(0);
                        if (labelGOs[i] != null)
                            labelGOs[i].GetComponentInChildren<LocText>().text = "0";
                        if (sliderGOs[i] != null)
                        {
                            var slider = sliderGOs[i].GetComponentInChildren<Slider>();
                            if (slider != null) slider.value = 0f;
                        }
                    }
                }
            });

            var dialog = new PDialog("BeverageModifiers")
            {
                Title = title,
                Size  = new Vector2(420f, 600f),
                DialogClosed = _ =>
                {
                    POptions.WriteSettings(options);
                    DbInitializePatches.Db_Initialize_EffectsAndTech.ReapplyBeverageModifiers();
                }
            };
            dialog.Body.AddChild(btnRow);
            dialog.Body.AddChild(new PScrollPane("Scroll")
            {
                Child          = grid,
                ScrollHorizontal = false,
                ScrollVertical   = true,
                FlexSize         = Vector2.one,
                TrackSize        = 8
            });
            dialog.AddButton(PDialog.DIALOG_KEY_CLOSE, STRINGS.UI.CONFIRMDIALOG.OK, null);
            dialog.Show();
        }

        private static IEnumerable<AttributeDef> GetAttributeDefs(BeverageModifiers m)
        {
            yield return new AttributeDef("art", "Art",
                () => m.ArtEnabled, v => m.ArtEnabled = v,
                () => m.ArtValue,   v => m.ArtValue = v);
            yield return new AttributeDef("athletics", "Athletics",
                () => m.AthleticsEnabled, v => m.AthleticsEnabled = v,
                () => m.AthleticsValue,   v => m.AthleticsValue = v);
            yield return new AttributeDef("botanist", "Botanist",
                () => m.BotanistEnabled, v => m.BotanistEnabled = v,
                () => m.BotanistValue,   v => m.BotanistValue = v);
            yield return new AttributeDef("caring", "Caring",
                () => m.CaringEnabled, v => m.CaringEnabled = v,
                () => m.CaringValue,   v => m.CaringValue = v);
            yield return new AttributeDef("carryamount", "Carry Amount",
                () => m.CarryAmountEnabled, v => m.CarryAmountEnabled = v,
                () => m.CarryAmountValue,   v => m.CarryAmountValue = v,
                tooltip: "Each step = 18 kg. Range: -180 kg to +180 kg.");
            yield return new AttributeDef("construction", "Construction",
                () => m.ConstructionEnabled, v => m.ConstructionEnabled = v,
                () => m.ConstructionValue,   v => m.ConstructionValue = v);
            yield return new AttributeDef("cooking", "Cooking",
                () => m.CookingEnabled, v => m.CookingEnabled = v,
                () => m.CookingValue,   v => m.CookingValue = v);
            yield return new AttributeDef("diseasecurespeed", "Disease Cure Speed",
                () => m.DiseaseCureSpeedEnabled, v => m.DiseaseCureSpeedEnabled = v,
                () => m.DiseaseCureSpeedValue,   v => m.DiseaseCureSpeedValue = v,
                tooltip: "Each step = 5%. Range: -50% to +50%.");
            yield return new AttributeDef("digging", "Digging",
                () => m.DiggingEnabled, v => m.DiggingEnabled = v,
                () => m.DiggingValue,   v => m.DiggingValue = v);
            yield return new AttributeDef("germresistance", "Germ Resistance",
                () => m.GermResistanceEnabled, v => m.GermResistanceEnabled = v,
                () => m.GermResistanceValue,   v => m.GermResistanceValue = v);
            yield return new AttributeDef("learning", "Learning",
                () => m.LearningEnabled, v => m.LearningEnabled = v,
                () => m.LearningValue,   v => m.LearningValue = v);
            yield return new AttributeDef("machinery", "Machinery",
                () => m.MachineryEnabled, v => m.MachineryEnabled = v,
                () => m.MachineryValue,   v => m.MachineryValue = v);
            yield return new AttributeDef("qualityoflife", "Morale",
                () => m.QualityOfLifeEnabled, v => m.QualityOfLifeEnabled = v,
                () => m.QualityOfLifeValue,   v => m.QualityOfLifeValue = v);
            yield return new AttributeDef("ranching", "Ranching",
                () => m.RanchingEnabled, v => m.RanchingEnabled = v,
                () => m.RanchingValue,   v => m.RanchingValue = v);
            yield return new AttributeDef("sneezyness", "Sneezyness",
                () => m.SneezynessEnabled, v => m.SneezynessEnabled = v,
                () => m.SneezynessValue,   v => m.SneezynessValue = v);
            yield return new AttributeDef("spacenavigation", "Space Navigation",
                () => m.SpaceNavigationEnabled, v => m.SpaceNavigationEnabled = v,
                () => m.SpaceNavigationValue,   v => m.SpaceNavigationValue = v);
            yield return new AttributeDef("strength", "Strength",
                () => m.StrengthEnabled, v => m.StrengthEnabled = v,
                () => m.StrengthValue,   v => m.StrengthValue = v);
        }

        private struct AttributeDef
        {
            public string       Id;
            public string       Label;
            public string       Tooltip;
            public Func<bool>   GetEnabled;
            public Action<bool> SetEnabled;
            public Func<int>    GetValue;
            public Action<int>  SetValue;

            public AttributeDef(
                string id, string label,
                Func<bool> getEnabled, Action<bool> setEnabled,
                Func<int> getValue,    Action<int>  setValue,
                string tooltip = null)
            {
                Id         = id;
                Label      = label;
                Tooltip    = tooltip;
                GetEnabled = getEnabled;
                SetEnabled = setEnabled;
                GetValue   = getValue;
                SetValue   = setValue;
            }
        }
    }
}
