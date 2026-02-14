using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PeterHan.PLib.UI;
using ControlledExtraction.Components;

namespace ControlledExtraction.UI
{
    // Sidescreen for selecting which meltable types an individual Ice Kettle accepts.
    // Groups elements into Vanilla/DLC and Modded with group-level checkboxes
    // (checked/unchecked/partial) and collapsible child lists.
    public class IceKettleSelectionSideScreen : SideScreenContent
    {
        private IceKettleController target;
        private Dictionary<Tag, GameObject> checkboxObjects = new Dictionary<Tag, GameObject>();
        private bool uiBuilt;

        // Group headers and their child panels for collapse/expand
        private readonly List<GroupInfo> groups = new List<GroupInfo>();

        public override string GetTitle() => "Accepted Meltables";

        public override bool IsValidForTarget(GameObject go) =>
            go != null && go.GetComponent<IceKettleController>() != null
            && IceKettleController.HasMultipleOptions();

        public override int GetSideScreenSortOrder() => 20;

        public override void SetTarget(GameObject targetGo)
        {
            target = targetGo?.GetComponent<IceKettleController>();
            if (uiBuilt) RefreshUI();
        }

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            BuildUI();
            uiBuilt = true;
            RefreshUI();
        }

        private void BuildUI()
        {
            var margin = new RectOffset(12, 4, 4, 4);
            var baseLayout = gameObject.GetComponent<BoxLayoutGroup>();
            if (baseLayout != null)
                baseLayout.Params = new BoxLayoutParams()
                {
                    Alignment = TextAnchor.MiddleLeft,
                    Margin = margin,
                };

            var panel = new PPanel("MeltableSelectionContents")
            {
                Direction = PanelDirection.Vertical,
                Alignment = TextAnchor.UpperLeft,
                Margin = margin,
                Spacing = 2,
                FlexSize = Vector2.right,
                BackColor = PUITuning.Colors.BackgroundLight
            };

            var ores = IceKettleController.GetEnabledIceOres();
            if (ores != null)
            {
                var vanillaOres = new List<Element>();
                var moddedOres = new List<Element>();

                foreach (var ore in ores)
                {
                    if (IceKettleController.IsVanillaElement(ore))
                        vanillaOres.Add(ore);
                    else
                        moddedOres.Add(ore);
                }

                if (vanillaOres.Count > 0)
                    AddGroup(panel, "Vanilla / DLC", vanillaOres);

                if (moddedOres.Count > 0)
                    AddGroup(panel, "Modded", moddedOres);
            }

            ContentContainer = panel.AddTo(gameObject, 0);
        }

        private void AddGroup(PPanel parent, string headerText, List<Element> elements)
        {
            var group = new GroupInfo { Name = headerText, Tags = new List<Tag>() };
            foreach (var ore in elements)
                group.Tags.Add(ore.tag);

            // Header row: group checkbox + expand arrow + label
            var headerRow = new PPanel("GroupHeader_" + headerText)
            {
                Direction = PanelDirection.Horizontal,
                Alignment = TextAnchor.MiddleLeft,
                Spacing = 4,
                FlexSize = Vector2.right
            };

            // Group-level checkbox (toggles all children)
            var groupCb = new PCheckBox("GroupCb_" + headerText)
            {
                InitialState = PCheckBox.STATE_CHECKED,
                OnChecked = (source, state) => OnGroupToggled(group, source, state),
                CheckSize = new Vector2(26, 26)
            };
            groupCb.AddOnRealize(go => group.CheckboxGo = go);
            headerRow.AddChild(groupCb);

            // Expand/collapse arrow button
            var arrow = new PLabel("Arrow_" + headerText)
            {
                Text = "\u25BC",
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            arrow.AddOnRealize(go =>
            {
                group.ArrowGo = go;
                var btn = go.AddComponent<UnityEngine.UI.Button>();
                btn.onClick.AddListener(() => ToggleGroupExpand(group));
            });
            headerRow.AddChild(arrow);

            // Group label (also clickable to expand/collapse)
            var label = new PLabel("GroupLabel_" + headerText)
            {
                Text = headerText,
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                TextAlignment = TextAnchor.MiddleLeft
            };
            label.AddOnRealize(go =>
            {
                var btn = go.AddComponent<UnityEngine.UI.Button>();
                btn.onClick.AddListener(() => ToggleGroupExpand(group));
            });
            headerRow.AddChild(label);

            parent.AddChild(headerRow);

            // Child panel (indented, collapsible)
            var childPanel = new PPanel("GroupChildren_" + headerText)
            {
                Direction = PanelDirection.Vertical,
                Alignment = TextAnchor.UpperLeft,
                Spacing = 2,
                Margin = new RectOffset(26, 0, 0, 4),
                FlexSize = Vector2.right
            };

            foreach (var ore in elements)
            {
                var tag = ore.tag;
                var outputName = ore.highTempTransition?.tag.ProperName() ?? "?";
                var tooltip = $"{ore.tag.ProperName()} \u2192 {outputName}";

                var cb = new PCheckBox(tag.ToString())
                {
                    Text = ore.tag.ProperName(),
                    ToolTip = tooltip,
                    TextStyle = PUITuning.Fonts.TextDarkStyle,
                    InitialState = PCheckBox.STATE_CHECKED,
                    OnChecked = (source, state) => OnItemToggled(tag, source, state),
                    CheckSize = new Vector2(26, 26)
                };
                cb.AddOnRealize(go => checkboxObjects[tag] = go);
                childPanel.AddChild(cb);
            }

            childPanel.AddOnRealize(go => group.ChildPanelGo = go);
            parent.AddChild(childPanel);

            group.Expanded = true;
            groups.Add(group);
        }

        private void ToggleGroupExpand(GroupInfo group)
        {
            if (group.ChildPanelGo == null) return;
            group.Expanded = !group.Expanded;
            group.ChildPanelGo.SetActive(group.Expanded);

            if (group.ArrowGo != null)
            {
                var label = group.ArrowGo.GetComponentInChildren<LocText>();
                if (label != null)
                    label.text = group.Expanded ? "\u25BC" : "\u25B6";
            }
        }

        private void OnGroupToggled(GroupInfo group, GameObject source, int currentState)
        {
            if (target == null) return;

            var selected = new HashSet<Tag>(target.GetSelectedIces());

            // If all or some are checked, uncheck all. If none are checked, check all.
            bool anyChecked = group.Tags.Any(t => selected.Contains(t));
            bool allChecked = group.Tags.All(t => selected.Contains(t));

            if (allChecked || anyChecked)
            {
                foreach (var tag in group.Tags)
                    selected.Remove(tag);
            }
            else
            {
                // Check all in this group
                foreach (var tag in group.Tags)
                    selected.Add(tag);
            }

            target.SetSelectedIces(selected.ToArray());
            RefreshUI();
        }

        private void OnItemToggled(Tag tag, GameObject source, int currentState)
        {
            if (target == null) return;

            var selected = new HashSet<Tag>(target.GetSelectedIces());
            bool isCurrentlyChecked = selected.Contains(tag);

            if (isCurrentlyChecked)
            {
                selected.Remove(tag);
            }
            else
            {
                selected.Add(tag);
            }

            target.SetSelectedIces(selected.ToArray());
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (target == null) return;
            var selected = new HashSet<Tag>(target.GetSelectedIces());

            // Update individual checkboxes
            foreach (var kvp in checkboxObjects)
            {
                if (kvp.Value != null)
                {
                    int newState = selected.Contains(kvp.Key)
                        ? PCheckBox.STATE_CHECKED
                        : PCheckBox.STATE_UNCHECKED;
                    PCheckBox.SetCheckState(kvp.Value, newState);
                }
            }

            // Update group checkboxes (checked / unchecked / partial)
            foreach (var group in groups)
            {
                if (group.CheckboxGo == null) continue;

                int checkedCount = group.Tags.Count(t => selected.Contains(t));
                int groupState;

                if (checkedCount == 0)
                    groupState = PCheckBox.STATE_UNCHECKED;
                else if (checkedCount == group.Tags.Count)
                    groupState = PCheckBox.STATE_CHECKED;
                else
                    groupState = PCheckBox.STATE_PARTIAL;

                PCheckBox.SetCheckState(group.CheckboxGo, groupState);
            }
        }

        private class GroupInfo
        {
            public string Name;
            public List<Tag> Tags;
            public GameObject CheckboxGo;
            public GameObject ArrowGo;
            public GameObject ChildPanelGo;
            public bool Expanded;
        }
    }
}
