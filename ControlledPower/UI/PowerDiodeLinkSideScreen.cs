using ControlledPower.Buildings;
using ControlledPower.Components;
using PeterHan.PLib.UI;
using UnityEngine;

namespace ControlledPower.UI
{
    internal sealed class PowerDiodeLinkSideScreen : SideScreenContent
    {
        private const string Title = "Power Diode Logic";
        private const string Label = "Enable backward logic sharing";
        private const string Tooltip = "When enabled, upstream logic readouts include downstream current and potential demand through this diode.";

        private PowerDiodeLogicLink _target;
        private GameObject _checkbox;

        public override string GetTitle() => Title;

        public override bool IsValidForTarget(GameObject target)
        {
            if (target == null)
                return false;
            var building = target.GetComponent<Building>();
            return target.GetComponent<PowerDiodeLogicLink>() != null && building?.Def?.PrefabID == PowerDiodeConfig.ID;
        }

        public override void SetTarget(GameObject target)
        {
            _target = target?.GetComponent<PowerDiodeLogicLink>();
            UpdateState();
        }

        public override void OnPrefabInit()
        {
            var margin = new RectOffset(4, 4, 4, 4);
            var baseLayout = gameObject.GetComponent<BoxLayoutGroup>();
            if (baseLayout != null)
            {
                baseLayout.Params = new BoxLayoutParams
                {
                    Alignment = TextAnchor.MiddleLeft,
                    Margin = margin
                };
            }

            var panel = new PPanel("PowerDiodeLinkPanel")
            {
                Direction = PanelDirection.Horizontal,
                Margin = margin,
                Spacing = 4,
                FlexSize = Vector2.right
            };

            var checkboxField = new PCheckBox("PowerDiodeLogicLinkCheckbox")
            {
                Text = Label,
                ToolTip = Tooltip,
                OnChecked = OnCheck,
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            checkboxField.AddOnRealize(obj => _checkbox = obj);

            panel.AddChild(checkboxField);
            panel.AddTo(gameObject);

            ContentContainer = gameObject;
            base.OnPrefabInit();
            UpdateState();
        }

        private void OnCheck(GameObject source, int state)
        {
            if (_target == null || _checkbox == null)
                return;

            int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            PCheckBox.SetCheckState(_checkbox, newState);
            _target.IsLogicLinkEnabled = newState == PCheckBox.STATE_CHECKED;
            PlaySound(GlobalAssets.GetSound("HUD_Click"));
        }

        private void UpdateState()
        {
            if (_target == null || _checkbox == null)
                return;
            PCheckBox.SetCheckState(_checkbox, _target.IsLogicLinkEnabled ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
        }
    }
}
