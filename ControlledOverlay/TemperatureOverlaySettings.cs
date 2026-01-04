using System;
using KSerialization;
using UnityEngine;

namespace ControlledOverlay
{
	// Per-colony settings for the temperature overlay
	// Attached to SaveGame.Instance.gameObject so it serializes with the save file
	// If the mod is removed, ONI's deserializer will gracefully ignore this component
	[SerializationConfig(MemberSerialization.OptIn)]
	public class TemperatureOverlaySettings : KMonoBehaviour
	{
		// Singleton instance for easy access
		public static TemperatureOverlaySettings Instance { get; private set; }

		// Hard limits (in Kelvin)
		public const float AbsoluteMinTemperature = 0f;        // 0K = -273.15°C
		public const float AbsoluteMaxTemperature = 3000f;     // 3000K = 2726.85°C
		public const float MinSliderGap = 100f;                // Minimum 100K range for slider
		public const float MinDisplayWindow = 50f;             // Minimum 50K display window
		public const float MaxExpandValue = 300f;              // Maximum expand amount

		// Default values (matching vanilla behavior)
		public const float DefaultSliderMin = 100f;            // 100K
		public const float DefaultSliderMax = 800f;            // 800K
		public const float DefaultOffsetMin = -100f;           // -100K below center
		public const float DefaultOffsetMax = 100f;            // +100K above center
		public const float DefaultExpandValue = 0f;            // No expansion
		public const float DefaultSliderPosition = 294.15f;    // 21°C (room temp)

		// Serialized per-colony values
		[Serialize] private float sliderMin = DefaultSliderMin;
		[Serialize] private float sliderMax = DefaultSliderMax;
		[Serialize] private float offsetMin = DefaultOffsetMin;
		[Serialize] private float offsetMax = DefaultOffsetMax;
		[Serialize] private float expandValue = DefaultExpandValue;

		// Event for other mods and UI to react to changes
		public event System.Action OnSettingsChanged;

		// Public properties with validation
		public float SliderMin
		{
			get => sliderMin;
			set => SetSliderMin(value);
		}

		public float SliderMax
		{
			get => sliderMax;
			set => SetSliderMax(value);
		}

		public float OffsetMin
		{
			get => offsetMin;
			set => SetOffsetMin(value);
		}

		public float OffsetMax
		{
			get => offsetMax;
			set => SetOffsetMax(value);
		}

		public float ExpandValue
		{
			get => expandValue;
			set => SetExpandValue(value);
		}

		// Computed properties for the effective display range
		public float EffectiveOffsetMin => offsetMin - expandValue;
		public float EffectiveOffsetMax => offsetMax + expandValue;
		public float EffectiveDisplayWindow => EffectiveOffsetMax - EffectiveOffsetMin;

		// Computed property for slider range
		public float SliderRange => sliderMax - sliderMin;

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			Instance = this;
		}

		protected override void OnCleanUp()
		{
			if (Instance == this)
			{
				Instance = null;
			}
			base.OnCleanUp();
		}

		// Validation methods
		private void SetSliderMin(float value)
		{
			// Clamp to absolute limits
			value = Mathf.Clamp(value, AbsoluteMinTemperature, AbsoluteMaxTemperature - MinSliderGap);
			
			// Ensure min is at least MinSliderGap below max
			if (value > sliderMax - MinSliderGap)
			{
				value = sliderMax - MinSliderGap;
			}

			if (Math.Abs(sliderMin - value) > 0.001f)
			{
				sliderMin = value;
				NotifySettingsChanged();
			}
		}

		private void SetSliderMax(float value)
		{
			// Clamp to absolute limits
			value = Mathf.Clamp(value, AbsoluteMinTemperature + MinSliderGap, AbsoluteMaxTemperature);
			
			// Ensure max is at least MinSliderGap above min
			if (value < sliderMin + MinSliderGap)
			{
				value = sliderMin + MinSliderGap;
			}

			if (Math.Abs(sliderMax - value) > 0.001f)
			{
				sliderMax = value;
				NotifySettingsChanged();
			}
		}

		private void SetOffsetMin(float value)
		{
			// Offset min should be negative or zero (below center)
			value = Mathf.Clamp(value, -1500f, 0f);

			if (Math.Abs(offsetMin - value) > 0.001f)
			{
				offsetMin = value;
				NotifySettingsChanged();
			}
		}

		private void SetOffsetMax(float value)
		{
			// Offset max should be positive or zero (above center)
			value = Mathf.Clamp(value, 0f, 1500f);

			if (Math.Abs(offsetMax - value) > 0.001f)
			{
				offsetMax = value;
				NotifySettingsChanged();
			}
		}

		private void SetExpandValue(float value)
		{
			value = Mathf.Clamp(value, 0f, MaxExpandValue);

			if (Math.Abs(expandValue - value) > 0.001f)
			{
				expandValue = value;
				NotifySettingsChanged();
			}
		}

		// Reset all values to defaults
		public void ResetToDefaults()
		{
			sliderMin = DefaultSliderMin;
			sliderMax = DefaultSliderMax;
			offsetMin = DefaultOffsetMin;
			offsetMax = DefaultOffsetMax;
			expandValue = DefaultExpandValue;
			NotifySettingsChanged();
		}

		private void NotifySettingsChanged()
		{
			OnSettingsChanged?.Invoke();
		}

		// Helper methods for converting between scroll percentage and temperature
		public float KelvinToScrollPercentage(float kelvin)
		{
			float range = sliderMax - sliderMin;
			if (range <= 0) return 0f;
			return Mathf.Clamp01((kelvin - sliderMin) / range);
		}

		public float ScrollPercentageToKelvin(float percentage)
		{
			return sliderMin + (sliderMax - sliderMin) * Mathf.Clamp01(percentage);
		}

		// Validate that the effective display window meets minimum requirements
		public bool IsDisplayWindowValid()
		{
			return EffectiveDisplayWindow >= MinDisplayWindow;
		}
	}
}

