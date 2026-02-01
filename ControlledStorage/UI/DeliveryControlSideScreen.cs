using UnityEngine;
using UnityEngine.UI;
using PeterHan.PLib.UI;
using ControlledStorage.Patches;

namespace ControlledStorage.UI
{
    // Sidescreen for controlling dupe and auto-sweeper access to storage buildings.
    // Matches StorageRefrigeratorThresholds ThresholdsSideScreen pattern.
    public class DeliveryControlSideScreen : SideScreenContent
    {
        private StorageDeliveryControl target;
        private GameObject dupeDepositCheckbox;
        private GameObject dupeExtractCheckbox;
        private GameObject sweeperDepositCheckbox;
        private GameObject sweeperExtractCheckbox;

        public override string GetTitle() => ControlledStorageStrings.UI.DELIVERY_CONTROL.TITLE;

        public override bool IsValidForTarget(GameObject go) =>
            go != null && go.GetComponent<StorageDeliveryControl>() != null;

        public override int GetSideScreenSortOrder() => 21;

        protected override void OnPrefabInit()
        {
            var margin = new RectOffset(12, 4, 4, 4);
            var baseLayout = gameObject.GetComponent<BoxLayoutGroup>();
            if (baseLayout != null)
                baseLayout.Params = new BoxLayoutParams()
                {
                    Alignment = TextAnchor.MiddleLeft,
                    Margin = margin,
                };

            var panel = new PPanel("DeliveryControlContents")
            {
                Direction = PanelDirection.Vertical,
                Alignment = TextAnchor.UpperLeft,
                Margin = margin,
                Spacing = 4,
                FlexSize = Vector2.right,
                BackColor = PUITuning.Colors.BackgroundLight
            };

            var copyButton = new PButton("DeliveryControl_CopyButton")
            {
                Text = ControlledStorageStrings.UI.DELIVERY_CONTROL.COPY_DELIVERY_SETTINGS,
                ToolTip = ControlledStorageStrings.UI.DELIVERY_CONTROL.COPY_DELIVERY_SETTINGS_TOOLTIP,
                OnClick = OnCopyDeliverySettingsClicked,
                Margin = new RectOffset(0, 0, 0, 4)
            };
            copyButton.AddOnRealize((obj) =>
            {
                var layout = obj.GetComponent<LayoutGroup>();
                if (layout != null)
                    layout.padding = new RectOffset(12, 12, 6, 6);
            });
            panel.AddChild(copyButton);

            var dupeDeposit = new PCheckBox("DeliveryControl_DupeDeposit")
            {
                Text = ControlledStorageStrings.UI.DELIVERY_CONTROL.DUPE_DEPOSIT,
                ToolTip = ControlledStorageStrings.UI.DELIVERY_CONTROL.DUPE_DEPOSIT_TOOLTIP,
                OnChecked = OnDupeDepositChecked,
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                CheckSize = new Vector2(22f, 22f)
            };
            dupeDeposit.AddOnRealize((obj) => dupeDepositCheckbox = obj);
            panel.AddChild(dupeDeposit);

            var dupeExtract = new PCheckBox("DeliveryControl_DupeExtract")
            {
                Text = ControlledStorageStrings.UI.DELIVERY_CONTROL.DUPE_EXTRACT,
                ToolTip = ControlledStorageStrings.UI.DELIVERY_CONTROL.DUPE_EXTRACT_TOOLTIP,
                OnChecked = OnDupeExtractChecked,
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                CheckSize = new Vector2(22f, 22f)
            };
            dupeExtract.AddOnRealize((obj) => dupeExtractCheckbox = obj);
            panel.AddChild(dupeExtract);

            var sweeperDeposit = new PCheckBox("DeliveryControl_SweeperDeposit")
            {
                Text = ControlledStorageStrings.UI.DELIVERY_CONTROL.SWEEPER_DEPOSIT,
                ToolTip = ControlledStorageStrings.UI.DELIVERY_CONTROL.SWEEPER_DEPOSIT_TOOLTIP,
                OnChecked = OnSweeperDepositChecked,
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                CheckSize = new Vector2(22f, 22f)
            };
            sweeperDeposit.AddOnRealize((obj) => sweeperDepositCheckbox = obj);
            panel.AddChild(sweeperDeposit);

            var sweeperExtract = new PCheckBox("DeliveryControl_SweeperExtract")
            {
                Text = ControlledStorageStrings.UI.DELIVERY_CONTROL.SWEEPER_EXTRACT,
                ToolTip = ControlledStorageStrings.UI.DELIVERY_CONTROL.SWEEPER_EXTRACT_TOOLTIP,
                OnChecked = OnSweeperExtractChecked,
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                CheckSize = new Vector2(22f, 22f)
            };
            sweeperExtract.AddOnRealize((obj) => sweeperExtractCheckbox = obj);
            panel.AddChild(sweeperExtract);

            panel.AddTo(gameObject);

            foreach (var img in gameObject.GetComponentsInChildren<Image>(true))
            {
                if (img.GetComponentInParent<MultiToggle>(true) == null)
                    img.raycastTarget = false;
            }

            ContentContainer = gameObject;
            base.OnPrefabInit();
            if (target != null) RefreshUI();
        }

        private void OnCopyDeliverySettingsClicked(GameObject _)
        {
            if (target == null) return;
            var go = target.gameObject;
            DeliveryControlCopyPatches.StartCopyDeliveryControl(go);
            PlaySound(GlobalAssets.GetSound("HUD_Click"));
            PlayerController.Instance.ActivateTool(CopySettingsTool.Instance);
        }

        private void OnDupeDepositChecked(GameObject source, int state)
        {
            int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            if (dupeDepositCheckbox != null) PCheckBox.SetCheckState(dupeDepositCheckbox, newState);
            PlaySound(GlobalAssets.GetSound("HUD_Click"));
            if (target != null) target.AllowDupeDeposit = newState == PCheckBox.STATE_CHECKED;
        }

        private void OnDupeExtractChecked(GameObject source, int state)
        {
            int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            if (dupeExtractCheckbox != null) PCheckBox.SetCheckState(dupeExtractCheckbox, newState);
            PlaySound(GlobalAssets.GetSound("HUD_Click"));
            if (target != null) target.AllowDupeExtract = newState == PCheckBox.STATE_CHECKED;
        }

        private void OnSweeperDepositChecked(GameObject source, int state)
        {
            int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            if (sweeperDepositCheckbox != null) PCheckBox.SetCheckState(sweeperDepositCheckbox, newState);
            PlaySound(GlobalAssets.GetSound("HUD_Click"));
            if (target != null) target.AllowSweeperDeposit = newState == PCheckBox.STATE_CHECKED;
        }

        private void OnSweeperExtractChecked(GameObject source, int state)
        {
            int newState = state == PCheckBox.STATE_CHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            if (sweeperExtractCheckbox != null) PCheckBox.SetCheckState(sweeperExtractCheckbox, newState);
            PlaySound(GlobalAssets.GetSound("HUD_Click"));
            if (target != null) target.AllowSweeperExtract = newState == PCheckBox.STATE_CHECKED;
        }

        public override void SetTarget(GameObject newTarget)
        {
            target = newTarget != null ? newTarget.GetComponent<StorageDeliveryControl>() : null;
            RefreshUI();
        }

        public override void ClearTarget() => target = null;

        private void RefreshUI()
        {
            if (target == null) return;
            if (dupeDepositCheckbox != null)
                PCheckBox.SetCheckState(dupeDepositCheckbox, target.AllowDupeDeposit ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
            if (dupeExtractCheckbox != null)
                PCheckBox.SetCheckState(dupeExtractCheckbox, target.AllowDupeExtract ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
            if (sweeperDepositCheckbox != null)
                PCheckBox.SetCheckState(sweeperDepositCheckbox, target.AllowSweeperDeposit ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
            if (sweeperExtractCheckbox != null)
                PCheckBox.SetCheckState(sweeperExtractCheckbox, target.AllowSweeperExtract ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED);
        }
    }
}
