using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ControlledMods.ResourceSensor.UI
{
    /// <summary>
    /// Sidescreen for the original ResourceSensor mod (Berkay's). Uses reflection to read/write their LogicResourceSensor.
    /// Same UI as ResourceSensorFIXED but no Global option (Distance and Room only).
    /// </summary>
    public class ResourceSensorSideScreen : SideScreenContent
    {
        private object _targetSensor; // ResourceSensor.LogicResourceSensor
        private static Type _sensorType;

        public KToggle countDistanceToggle;
        private GameObject distanceSliderContainer;
        public KSlider distanceSlider;
        public LocText distanceText;
        public KToggle countRoomToggle;
        private GameObject countStorageContainer;
        public KToggle countStorageToggle;
        public KImage distanceCheckmark;
        public KImage roomCheckmark;
        public KImage countStorageCheckmark;
        private GameObject dividerMargin;
        private GameObject divider;

        private static Type SensorType => _sensorType ??= AccessTools.TypeByName("ResourceSensor.LogicResourceSensor");

        // NOTE: This sidescreen is no longer registered; keep as non-override to avoid
        // signature/access mismatches across different Assembly-CSharp references.
        public new void OnPrefabInit()
        {
            base.OnPrefabInit();
            SetElements();
        }

        private void SetElements()
        {
            try
            {
                GameObject distanceContainer = transform.Find("Contents/CheckboxGroup").gameObject;

                Transform checkBoxGroup = distanceContainer.transform.Find("CrittersCheckBox");
                checkBoxGroup.name = "CountDistanceCheckBox";
                distanceContainer.transform.Find("Label").gameObject.GetComponent<LocText>().SetText("Distance Mode");
                countDistanceToggle = checkBoxGroup.GetComponent<KToggle>();
                distanceCheckmark = countDistanceToggle.transform.Find("CheckMark").GetComponent<KImage>();

                GameObject roomContainer = distanceContainer.transform.Find("Contents/CheckboxGroup/EggsCheckBox").parent.gameObject;
                checkBoxGroup = roomContainer.transform.Find("EggsCheckBox");
                checkBoxGroup.name = "CountRoomCheckBox";
                roomContainer.transform.Find("Label").gameObject.GetComponent<LocText>().SetText("Room Mode");
                countRoomToggle = checkBoxGroup.GetComponent<KToggle>();
                roomCheckmark = countRoomToggle.transform.Find("CheckMark").GetComponent<KImage>();

                countStorageContainer = GameObject.Instantiate(distanceContainer, distanceContainer.transform.parent);
                countStorageContainer.transform.Find("Label").gameObject.GetComponent<LocText>().SetText("Include Storage Buildings");
                checkBoxGroup = countStorageContainer.transform.Find("CountDistanceCheckBox");
                checkBoxGroup.name = "IncludeStorageCheckBox";
                countStorageToggle = checkBoxGroup.GetComponent<KToggle>();
                countStorageCheckmark = countStorageToggle.transform.Find("CheckMark").GetComponent<KImage>();
                countStorageContainer.GetComponent<RectTransform>().localScale = new Vector3(0.94f, 0.94f, 1f);

                dividerMargin = new GameObject("Margin");
                dividerMargin.transform.SetParent(distanceContainer.transform.parent);
                dividerMargin.transform.SetSiblingIndex(4);
                dividerMargin.AddComponent<LayoutElement>();
                dividerMargin.transform.localScale = Vector3.one;
                dividerMargin.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 4f);

                divider = new GameObject("Divider");
                divider.transform.SetParent(distanceContainer.transform.parent);
                divider.transform.SetSiblingIndex(5);
                var le = divider.AddComponent<LayoutElement>();
                le.minWidth = 240f;
                le.preferredWidth = 240f;
                le.minHeight = 1f;
                le.preferredHeight = 1f;
                le.flexibleWidth = 1;
                divider.transform.localScale = Vector3.one;
                divider.transform.localPosition = new Vector3(-122, -55, 0);
                var img = divider.AddComponent<Image>();
                img.color = new Color(0.7f, 0.7f, 0.7f);

                // Slider: this sidescreen is no longer registered (we patch Berkay's UI directly),
                // but keep a safe fallback here so the project still compiles.
                GameObject sliderSource = null;
                if (sliderSource != null)
                {
                    distanceSliderContainer = GameObject.Instantiate(sliderSource, distanceContainer.transform.parent);
                }
                else
                {
                    distanceSliderContainer = new GameObject("DistanceSlider");
                    distanceSliderContainer.transform.SetParent(distanceContainer.transform.parent);
                }
                distanceSliderContainer.name = "DistanceSlider";
                distanceSliderContainer.transform.SetSiblingIndex(2);

                distanceText = distanceSliderContainer.transform.Find("Max/Label")?.GetComponent<LocText>();
                if (distanceText != null)
                    distanceText.SetText("Distance: 3");
                var unitsLabel = distanceSliderContainer.transform.Find("Max/UnitsLabel")?.GetComponent<LocText>();
                if (unitsLabel != null)
                    unitsLabel.SetText("Tiles");

                distanceSlider = distanceSliderContainer.transform.Find("SliderContainer/Slider")?.GetComponent<KSlider>();
                if (distanceSlider != null)
                {
                    distanceSlider.wholeNumbers = true;
                    distanceSlider.minValue = 0f;
                    distanceSlider.value = 3;
                    distanceSlider.maxValue = 20f;
                    distanceSlider.SetTooltipText(string.Format(UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP, 3));
                }
            }
            catch (Exception e)
            {
                ControlledModsMod.LogWarning($"ResourceSensor SetElements: {e.Message}");
            }
        }

        // NOTE: This sidescreen is no longer registered; keep as non-override to avoid
        // signature/access mismatches across different Assembly-CSharp references.
        public new void OnSpawn()
        {
            base.OnSpawn();
            if (countDistanceToggle != null) countDistanceToggle.onClick += ToggleDistance;
            if (countRoomToggle != null) countRoomToggle.onClick += ToggleRoom;
            if (countStorageToggle != null) countStorageToggle.onClick += ToggleCountStorage;
            if (distanceSlider != null)
            {
                distanceSlider.SetTooltipText(string.Format(UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP, 0));
                distanceSlider.onDrag += updateDistance;
                distanceSlider.onPointerDown += updateDistance;
                distanceSlider.onMove += updateDistance;
            }
        }

        public override bool IsValidForTarget(GameObject target)
        {
            if (target == null || SensorType == null) return false;
            var c = target.GetComponent(SensorType);
            return c != null && !((UnityEngine.Object)c).IsNullOrDestroyed();
        }

        public override void SetTarget(GameObject target)
        {
            gameObject.SetActive(true);
            if (target == null) return;
            if (distanceCheckmark == null) SetElements();

            _targetSensor = target.GetComponent(SensorType);
            if (_targetSensor == null) return;

            int dist = GetDistance();
            if (distanceSlider != null) distanceSlider.value = dist;
            if (distanceText != null) distanceText.SetText($"Distance: {dist}");
            if (distanceSlider != null) distanceSlider.SetTooltipText(string.Format(UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP, dist));
            if (countStorageCheckmark != null) countStorageCheckmark.enabled = GetIncludeStorage();

            int mode = GetMode();
            if (distanceCheckmark != null) distanceCheckmark.enabled = (mode == 0);
            if (roomCheckmark != null) roomCheckmark.enabled = (mode == 1);
            if (distanceSliderContainer != null) distanceSliderContainer.SetActive(mode == 0);
            if (countStorageContainer != null) countStorageContainer.SetActive(true);
            if (dividerMargin != null) dividerMargin.SetActive(true);
            if (divider != null) divider.SetActive(true);
        }

        private int GetMode()
        {
            try
            {
                var prop = AccessTools.Property(SensorType, "Mode");
                var v = prop?.GetValue(_targetSensor);
                return v != null ? (int)v : 0;
            }
            catch { return 0; }
        }

        private void SetMode(int value)
        {
            try
            {
                var prop = AccessTools.Property(SensorType, "Mode");
                var enumType = SensorType.Assembly.GetType("ResourceSensor.LogicResourceSensor+SensorMode");
                if (enumType != null && prop != null)
                {
                    var enumVal = Enum.ToObject(enumType, value);
                    prop.SetValue(_targetSensor, enumVal);
                }
            }
            catch { }
        }

        private int GetDistance()
        {
            try
            {
                var prop = AccessTools.Property(SensorType, "Distance");
                var v = prop?.GetValue(_targetSensor);
                return v != null ? (int)v : 3;
            }
            catch { return 3; }
        }

        private void SetDistance(int value)
        {
            try
            {
                var prop = AccessTools.Property(SensorType, "Distance");
                prop?.SetValue(_targetSensor, value);
            }
            catch { }
        }

        private bool GetIncludeStorage()
        {
            try
            {
                var prop = AccessTools.Property(SensorType, "IncludeStorage");
                var v = prop?.GetValue(_targetSensor);
                return v is bool b && b;
            }
            catch { return false; }
        }

        private void SetIncludeStorage(bool value)
        {
            try
            {
                var prop = AccessTools.Property(SensorType, "IncludeStorage");
                prop?.SetValue(_targetSensor, value);
            }
            catch { }
        }

        private void ToggleDistance()
        {
            SetMode(0);
            if (distanceCheckmark != null) distanceCheckmark.enabled = true;
            if (roomCheckmark != null) roomCheckmark.enabled = false;
            if (countStorageContainer != null) countStorageContainer.SetActive(true);
            if (distanceSliderContainer != null) distanceSliderContainer.SetActive(true);
            if (dividerMargin != null) dividerMargin.SetActive(true);
            if (divider != null) divider.SetActive(true);
        }

        private void ToggleRoom()
        {
            SetMode(1);
            if (distanceCheckmark != null) distanceCheckmark.enabled = false;
            if (roomCheckmark != null) roomCheckmark.enabled = true;
            if (countStorageContainer != null) countStorageContainer.SetActive(true);
            if (distanceSliderContainer != null) distanceSliderContainer.SetActive(false);
            if (dividerMargin != null) dividerMargin.SetActive(true);
            if (divider != null) divider.SetActive(true);
        }

        private void ToggleCountStorage()
        {
            bool next = !GetIncludeStorage();
            SetIncludeStorage(next);
            if (countStorageCheckmark != null) countStorageCheckmark.enabled = next;
        }

        private void updateDistance()
        {
            if (distanceSlider == null) return;
            int v = (int)distanceSlider.value;
            SetDistance(v);
            if (distanceText != null) distanceText.SetText($"Distance: {v}");
            distanceSlider.SetTooltipText(string.Format(UISIDESCREENS.RESOURCE_SENSOR_SIDE_SCREEN.SLIDER_TOOLTIP, v));
        }
    }
}
