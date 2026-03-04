using ControlledConduits.Components;
using PeterHan.PLib.UI;
using STRINGS;
using UnityEngine;

namespace ControlledConduits.UI
{
    // Custom sidescreen so "Ignore Empty" appears at bottom of Config tab (GetSideScreenSortOrder) instead of vanilla SingleCheckboxSideScreen.
    public class ConduitVolumeSensorSideScreen : SideScreenContent
    {
        private GameObject checkboxWidget;
        private ConduitVolumeSensor targetSensor;

        public override void OnPrefabInit()
        {
            titleKey = "STRINGS.CONTROLLEDCONDUITS.VOLUME_SENSOR_OPTIONS_TITLE";

            var margin = new RectOffset(4, 4, 4, 4);
            var baseLayout = gameObject.GetComponent<PeterHan.PLib.UI.BoxLayoutGroup>();
            if (baseLayout != null)
                baseLayout.Params = new BoxLayoutParams { Alignment = TextAnchor.MiddleLeft, Margin = margin };

            var panel = new PPanel("IgnoreEmptyPanel")
            {
                Direction = PanelDirection.Horizontal,
                Margin = margin,
                Spacing = 4,
                FlexSize = Vector2.right
            };

            var check = new PCheckBox("IgnoreEmptyCheckbox")
            {
                Text = CONTROLLEDCONDUITS.IGNORE_EMPTY_LABEL,
                ToolTip = CONTROLLEDCONDUITS.IGNORE_EMPTY_TOOLTIP,
                OnChecked = OnIgnoreEmptyChanged,
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            check.AddOnRealize(obj => { checkboxWidget = obj; });
            panel.AddChild(check);
            panel.AddTo(gameObject);
            ContentContainer = gameObject;
            base.OnPrefabInit();
        }

        public override bool IsValidForTarget(GameObject target) =>
            target?.GetComponent<ConduitVolumeSensor>() != null;

        public override void SetTarget(GameObject target)
        {
            base.SetTarget(target);
            targetSensor = target?.GetComponent<ConduitVolumeSensor>();
            RefreshCheckbox();
        }

        public override int GetSideScreenSortOrder() => 100;

        private void OnIgnoreEmptyChanged(GameObject source, int state)
        {
            if (targetSensor == null) return;
            // PLib passes current state on click; new state is the opposite.
            bool newChecked = (state == PCheckBox.STATE_UNCHECKED);
            targetSensor.ignoreEmpty = newChecked;
            PCheckBox.SetCheckState(source, newChecked ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
        }

        private void RefreshCheckbox()
        {
            if (targetSensor == null || checkboxWidget == null) return;
            PCheckBox.SetCheckState(checkboxWidget,
                targetSensor.ignoreEmpty ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
        }
    }
}
