using System;
using System.Collections.Generic;
using UnityEngine;
using PeterHan.PLib.UI;

namespace ControlledStorage.NoSweepZone
{
    public sealed class NoSweepZoneToolMenu : KMonoBehaviour
    {
        public static NoSweepZoneToolMenu Instance { get; private set; }

        internal event Action<NoSweepZoneMode> OnSettingChanged;

        internal NoSweepZoneMode SelectedKey { get; private set; }

        private GameObject choiceList;
        private GameObject content;
        private readonly IDictionary<NoSweepZoneMode, MenuOption> options = new Dictionary<NoSweepZoneMode, MenuOption>();

        public static void CreateInstance()
        {
            DestroyInstance();
            var parameterMenu = new GameObject("NoSweepZoneSettingsChangeParams");
            var originalMenu = ToolMenu.Instance?.toolParameterMenu;
            if (originalMenu != null)
                parameterMenu.transform.SetParent(originalMenu.transform.parent);

            Instance = parameterMenu.AddComponent<NoSweepZoneToolMenu>();
            parameterMenu.SetActive(true);
            parameterMenu.SetActive(false);
        }

        public static void DestroyInstance()
        {
            var inst = Instance;
            if (inst != null)
            {
                inst.ClearMenu();
                UnityEngine.Object.Destroy(inst.gameObject);
            }
            Instance = null;
        }

        public bool HasOptions => options.Count > 0;

        protected override void OnCleanUp()
        {
            ClearMenu();
            if (content != null)
                UnityEngine.Object.Destroy(content);
            base.OnCleanUp();
        }

        protected override void OnPrefabInit()
        {
            var menu = ToolMenu.Instance?.toolParameterMenu;
            if (menu == null) return;

            var baseContent = menu.content;
            base.OnPrefabInit();
            content = Util.KInstantiateUI(baseContent, baseContent.GetParent(), false);
            var transform = content.rectTransform();
            if (transform.childCount > 1)
                choiceList = transform.GetChild(1).gameObject;
            transform.offsetMax = new Vector2(0f, 300f);
            transform.SetAsFirstSibling();
            HideMenu();
        }

        internal void PopulateMenu()
        {
            ClearMenu();
            CreateOption("Mark Zone", NoSweepZoneMode.Set, ToolParameterMenu.ToggleState.On);
            CreateOption("Clear Zone", NoSweepZoneMode.Clear);
        }

        private void CreateOption(string text, NoSweepZoneMode mode, ToolParameterMenu.ToggleState state = ToolParameterMenu.ToggleState.Off)
        {
            if (choiceList == null || ToolMenu.Instance?.toolParameterMenu?.widgetPrefab == null) return;

            var widgetObj = Util.KInstantiateUI(ToolMenu.Instance.toolParameterMenu.widgetPrefab, choiceList, true);
            PUIElements.SetText(widgetObj, text);
            var toggle = widgetObj.GetComponentInChildren<MultiToggle>();
            if (toggle != null)
            {
                var checkbox = toggle.gameObject;
                var option = new MenuOption(mode, checkbox);
                PCheckBox.SetCheckState(checkbox, PCheckBox.STATE_PARTIAL);
                option.State = state;
                options.Add(mode, option);
                toggle.onClick += () => OnClick(checkbox);
            }
        }

        private void OnClick(GameObject target)
        {
            foreach (var option in options.Values)
            {
                if (option.Checkbox == target && option.State == ToolParameterMenu.ToggleState.Off)
                {
                    foreach (var other in options.Values)
                        if (other != option) other.State = ToolParameterMenu.ToggleState.Off;
                    option.State = ToolParameterMenu.ToggleState.On;
                    SelectedKey = option.Mode;
                    OnChange();
                    OnSettingChanged?.Invoke(SelectedKey);
                    break;
                }
            }
        }

        private void OnChange()
        {
            foreach (var option in options.Values)
            {
                var checkbox = option.Checkbox;
                PCheckBox.SetCheckState(checkbox,
                    option.State == ToolParameterMenu.ToggleState.On ? PCheckBox.STATE_CHECKED :
                    option.State == ToolParameterMenu.ToggleState.Off ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_PARTIAL);
            }
        }

        internal void SetOption(NoSweepZoneMode mode)
        {
            foreach (var option in options.Values)
            {
                option.State = option.Mode == mode ? ToolParameterMenu.ToggleState.On : ToolParameterMenu.ToggleState.Off;
                if (option.Mode == mode) SelectedKey = mode;
            }
            OnChange();
            OnSettingChanged?.Invoke(SelectedKey);
        }

        internal ToolParameterMenu.ToggleState GetState(NoSweepZoneMode key)
        {
            return options.TryGetValue(key, out var opt) ? opt.State : ToolParameterMenu.ToggleState.Off;
        }

        public void HideMenu() => content?.SetActive(false);

        public void ShowMenu()
        {
            content?.SetActive(true);
            OnChange();
        }

        private void ClearMenu()
        {
            HideMenu();
            foreach (var option in options)
                UnityEngine.Object.Destroy(option.Value.Checkbox);
            options.Clear();
            SelectedKey = NoSweepZoneMode.Set;
        }

        private sealed class MenuOption
        {
            public GameObject Checkbox { get; }
            public ToolParameterMenu.ToggleState State { get; set; }
            public NoSweepZoneMode Mode { get; }

            public MenuOption(NoSweepZoneMode mode, GameObject checkbox)
            {
                Checkbox = checkbox;
                Mode = mode;
                State = ToolParameterMenu.ToggleState.Off;
            }
        }
    }
}
