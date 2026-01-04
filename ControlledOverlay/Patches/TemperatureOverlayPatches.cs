using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ControlledOverlay.Patches
{
	// Adds advanced temperature controls to the Temperature Overlay's relative temperature widget
	// Phase 1: Text inputs for slider range, offset range, expand slider, and target temperature
	[HarmonyPatch(typeof(TemperatureOverlayThresholdAdjustmentWidget), "OnSpawn")]
	public static class TemperatureOverlayThresholdAdjustmentWidget_OnSpawn_Patch
	{
		// Reference to the widget for the Reset button patch
		private static TemperatureOverlayThresholdAdjustmentWidget currentWidget;
		private static Scrollbar mainScrollbar;

		// UI element references for updates
		private static TMP_InputField sliderMinInput;
		private static TMP_InputField sliderMaxInput;
		private static TMP_InputField offsetMinInput;
		private static TMP_InputField offsetMaxInput;
		private static Scrollbar expandSlider;
		private static TMP_Text expandValueText;
		private static TMP_Text effectiveRangeText;
		private static TMP_InputField targetInput;
		
		// Track if we've already created our UI for this widget
		private static GameObject advancedControlsContainer;
		private static GameObject targetInputContainer;

		// Default values (matching vanilla) for fallback
		private const float DefaultSliderMin = 100f;
		private const float DefaultSliderMax = 800f;
		private const float DefaultOffsetMin = -100f;
		private const float DefaultOffsetMax = 100f;

		public static void Postfix(TemperatureOverlayThresholdAdjustmentWidget __instance)
		{
			// Clean up any existing UI from previous runs
			CleanupExistingUI();

			// Get the scrollbar and center text via reflection
			var scrollbarField = AccessTools.Field(typeof(TemperatureOverlayThresholdAdjustmentWidget), "scrollbar");
			var centerTextField = AccessTools.Field(typeof(TemperatureOverlayThresholdAdjustmentWidget), "scrollBarRangeCenterText");
			var defaultButtonField = AccessTools.Field(typeof(TemperatureOverlayThresholdAdjustmentWidget), "defaultButton");

			if (scrollbarField == null || centerTextField == null) return;

			var scrollbar = scrollbarField.GetValue(__instance) as Scrollbar;
			var centerText = centerTextField.GetValue(__instance) as LocText;

			if (scrollbar == null || centerText == null) return;

			currentWidget = __instance;
			mainScrollbar = scrollbar;

			// Create the advanced controls UI below the main slider
			CreateAdvancedControlsUI(__instance, scrollbar, centerText);

			// Create the target temperature input (replacing center text)
			CreateTargetTemperatureInput(__instance, scrollbar, centerText);

			// Hook up the default button to reset all values
			var defaultButton = defaultButtonField?.GetValue(__instance) as KButton;
			if (defaultButton != null)
			{
				defaultButton.onClick += OnResetAllPressed;
			}

			// Subscribe to settings changes if available
			var settings = TemperatureOverlaySettings.Instance;
			if (settings != null)
			{
				settings.OnSettingsChanged += OnSettingsChanged;
			}

			// Initial UI refresh with delay to ensure settings are loaded
			__instance.StartCoroutine(DelayedRefresh());
		}

		private static void CleanupExistingUI()
		{
			// Destroy existing advanced controls if they exist
			if (advancedControlsContainer != null)
			{
				Object.Destroy(advancedControlsContainer);
				advancedControlsContainer = null;
			}
			
			// Destroy existing target input if it exists
			if (targetInputContainer != null)
			{
				Object.Destroy(targetInputContainer);
				targetInputContainer = null;
			}
			
			// Clear references
			sliderMinInput = null;
			sliderMaxInput = null;
			offsetMinInput = null;
			offsetMaxInput = null;
			expandSlider = null;
			expandValueText = null;
			effectiveRangeText = null;
			targetInput = null;
		}

		private static System.Collections.IEnumerator DelayedRefresh()
		{
			// Wait a frame for everything to initialize
			yield return null;
			RefreshUIFromSettings();
			
			// Force layout rebuild to make scroll rect recognize new content
			ForceLayoutRebuild();
			
			// Wait another frame and rebuild again (sometimes needed for nested layouts)
			yield return null;
			ForceLayoutRebuild();
		}

		private static void ForceLayoutRebuild()
		{
			if (currentWidget == null) return;

			// Walk up the hierarchy and rebuild all layout groups
			Transform current = currentWidget.transform;
			while (current != null)
			{
				var rectTransform = current.GetComponent<RectTransform>();
				if (rectTransform != null)
				{
					// Rebuild any layout group at this level
					var layoutGroup = current.GetComponent<LayoutGroup>();
					if (layoutGroup != null)
					{
						layoutGroup.CalculateLayoutInputHorizontal();
						layoutGroup.CalculateLayoutInputVertical();
						layoutGroup.SetLayoutHorizontal();
						layoutGroup.SetLayoutVertical();
					}
					
					// Rebuild content size fitters
					var sizeFitter = current.GetComponent<ContentSizeFitter>();
					if (sizeFitter != null)
					{
						sizeFitter.SetLayoutHorizontal();
						sizeFitter.SetLayoutVertical();
					}
					
					LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
				}
				current = current.parent;
			}

			// Find ScrollRect and rebuild
			var scrollRect = currentWidget.GetComponentInParent<ScrollRect>();
			if (scrollRect != null)
			{
				if (scrollRect.content != null)
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
				}
				
				// Force scroll rect to recalculate
				scrollRect.enabled = false;
				scrollRect.enabled = true;
			}

			// Find OverlayLegend and trigger rebuild
			var overlayLegend = OverlayLegend.Instance;
			if (overlayLegend != null)
			{
				var legendRect = overlayLegend.GetComponent<RectTransform>();
				if (legendRect != null)
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(legendRect);
				}
				
				// Try to call the legend's layout method via reflection
				var configMethod = AccessTools.Method(typeof(OverlayLegend), "ConfigureUIHeight");
				if (configMethod != null)
				{
					try { configMethod.Invoke(overlayLegend, null); } catch { }
				}
			}
		}

		private static void CreateAdvancedControlsUI(
			TemperatureOverlayThresholdAdjustmentWidget widget,
			Scrollbar scrollbar,
			LocText centerText)
		{
			// The widget is inside the OverlayLegend's diagram area
			// We need to add our controls after the widget in the same parent
			var widgetParent = widget.transform.parent;
			if (widgetParent == null) return;

			// Create a container for our advanced controls as a sibling of the widget
			advancedControlsContainer = new GameObject("AdvancedControls");
			advancedControlsContainer.transform.SetParent(widgetParent, false);
			
			// Position after the temperature widget
			int widgetIndex = widget.transform.GetSiblingIndex();
			advancedControlsContainer.transform.SetSiblingIndex(widgetIndex + 1);

			var containerRect = advancedControlsContainer.AddComponent<RectTransform>();
			containerRect.anchorMin = new Vector2(0f, 1f);
			containerRect.anchorMax = new Vector2(1f, 1f);
			containerRect.pivot = new Vector2(0.5f, 1f);
			containerRect.sizeDelta = new Vector2(0f, 75f);

			// Add LayoutElement so this container participates in parent layout
			var layoutElement = advancedControlsContainer.AddComponent<LayoutElement>();
			layoutElement.preferredHeight = 75f;
			layoutElement.minHeight = 75f;
			layoutElement.flexibleWidth = 0f;
			layoutElement.flexibleHeight = 0f;

			// Add vertical layout group
			var vertLayout = advancedControlsContainer.AddComponent<VerticalLayoutGroup>();
			vertLayout.spacing = 1f;
			vertLayout.childAlignment = TextAnchor.UpperCenter;
			vertLayout.childControlWidth = true;
			vertLayout.childControlHeight = true;
			vertLayout.childForceExpandWidth = false;
			vertLayout.childForceExpandHeight = false;
			vertLayout.padding = new RectOffset(5, 5, 3, 3);

			// Create Slider Range row
			CreateRangeRow(advancedControlsContainer.transform, "Slider:", 
				out sliderMinInput, out sliderMaxInput,
				OnSliderMinChanged, OnSliderMaxChanged);

			// Create Display Offset row
			CreateRangeRow(advancedControlsContainer.transform, "Offset:",
				out offsetMinInput, out offsetMaxInput,
				OnOffsetMinChanged, OnOffsetMaxChanged);

			// Create Expand slider row
			CreateExpandRow(advancedControlsContainer.transform);

			// Set initial display values (use defaults if settings not ready)
			SetInitialDisplayValues();
		}

		private static void SetInitialDisplayValues()
		{
			var settings = TemperatureOverlaySettings.Instance;
			
			float sMin = settings?.SliderMin ?? DefaultSliderMin;
			float sMax = settings?.SliderMax ?? DefaultSliderMax;
			float oMin = settings?.OffsetMin ?? DefaultOffsetMin;
			float oMax = settings?.OffsetMax ?? DefaultOffsetMax;
			float expand = settings?.ExpandValue ?? 0f;

			if (sliderMinInput != null)
				sliderMinInput.text = FormatTemperature(sMin);
			if (sliderMaxInput != null)
				sliderMaxInput.text = FormatTemperature(sMax);
			if (offsetMinInput != null)
				offsetMinInput.text = FormatDelta(oMin);
			if (offsetMaxInput != null)
				offsetMaxInput.text = FormatDelta(oMax);
			if (expandSlider != null)
				expandSlider.value = expand / TemperatureOverlaySettings.MaxExpandValue;
			
			UpdateExpandDisplay();
			UpdateEffectiveRangeDisplay();
		}

		private static void CreateRangeRow(Transform parent, string label,
			out TMP_InputField minInput, out TMP_InputField maxInput,
			UnityEngine.Events.UnityAction<string> onMinChanged,
			UnityEngine.Events.UnityAction<string> onMaxChanged)
		{
			var rowGO = new GameObject(label.Replace(":", "") + "Row");
			rowGO.transform.SetParent(parent, false);
			
			var rowRect = rowGO.AddComponent<RectTransform>();
			rowRect.sizeDelta = new Vector2(0f, 22f);

			var horizLayout = rowGO.AddComponent<HorizontalLayoutGroup>();
			horizLayout.spacing = 4f;
			horizLayout.childAlignment = TextAnchor.MiddleCenter;
			horizLayout.childControlWidth = false;
			horizLayout.childControlHeight = true;
			horizLayout.childForceExpandWidth = false;
			horizLayout.childForceExpandHeight = false;

			// Label
			CreateLabel(rowGO.transform, label, 50f, TextAnchor.MiddleRight);

			// Min input
			minInput = CreateInputField(rowGO.transform, "Min", 55f);
			minInput.onEndEdit.AddListener(onMinChanged);

			// Spacer/indicator
			CreateLabel(rowGO.transform, "to", 18f, TextAnchor.MiddleCenter);

			// Max input
			maxInput = CreateInputField(rowGO.transform, "Max", 55f);
			maxInput.onEndEdit.AddListener(onMaxChanged);

			// Unit suffix
			CreateLabel(rowGO.transform, GetTemperatureUnitSuffix(), 20f, TextAnchor.MiddleLeft);
		}

		private static void CreateExpandRow(Transform parent)
		{
			var rowGO = new GameObject("ExpandRow");
			rowGO.transform.SetParent(parent, false);
			
			var rowRect = rowGO.AddComponent<RectTransform>();
			rowRect.sizeDelta = new Vector2(0f, 22f);

			var horizLayout = rowGO.AddComponent<HorizontalLayoutGroup>();
			horizLayout.spacing = 4f;
			horizLayout.childAlignment = TextAnchor.MiddleCenter;
			horizLayout.childControlWidth = false;
			horizLayout.childControlHeight = true;
			horizLayout.childForceExpandWidth = false;
			horizLayout.childForceExpandHeight = false;

			// Label
			CreateLabel(rowGO.transform, "Expand:", 50f, TextAnchor.MiddleRight);

			// Expand slider
			var sliderGO = new GameObject("ExpandSlider");
			sliderGO.transform.SetParent(rowGO.transform, false);
			var sliderRect = sliderGO.AddComponent<RectTransform>();
			sliderRect.sizeDelta = new Vector2(80f, 18f);
			var sliderLayout = sliderGO.AddComponent<LayoutElement>();
			sliderLayout.preferredWidth = 80f;
			sliderLayout.preferredHeight = 18f;

			// Slider background
			var bgImage = sliderGO.AddComponent<Image>();
			bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

			// Sliding area
			var slidingAreaGO = new GameObject("SlidingArea");
			slidingAreaGO.transform.SetParent(sliderGO.transform, false);
			var slidingRect = slidingAreaGO.AddComponent<RectTransform>();
			slidingRect.anchorMin = Vector2.zero;
			slidingRect.anchorMax = Vector2.one;
			slidingRect.offsetMin = new Vector2(5f, 2f);
			slidingRect.offsetMax = new Vector2(-5f, -2f);

			// Handle
			var handleGO = new GameObject("Handle");
			handleGO.transform.SetParent(slidingAreaGO.transform, false);
			var handleRect = handleGO.AddComponent<RectTransform>();
			handleRect.sizeDelta = new Vector2(10f, 0f);
			var handleImage = handleGO.AddComponent<Image>();
			handleImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);

			expandSlider = sliderGO.AddComponent<Scrollbar>();
			expandSlider.handleRect = handleRect;
			expandSlider.direction = Scrollbar.Direction.LeftToRight;
			expandSlider.onValueChanged.AddListener(OnExpandChanged);

			// Value text
			var valueTextGO = CreateLabel(rowGO.transform, "+0", 35f, TextAnchor.MiddleLeft);
			expandValueText = valueTextGO.GetComponent<TMP_Text>();

			// Effective range label and text
			CreateLabel(rowGO.transform, "Range:", 40f, TextAnchor.MiddleRight);
			var rangeTextGO = CreateLabel(rowGO.transform, "-100 to +100", 75f, TextAnchor.MiddleLeft);
			effectiveRangeText = rangeTextGO.GetComponent<TMP_Text>();
		}

		private static GameObject CreateLabel(Transform parent, string text, float width, TextAnchor alignment)
		{
			var labelGO = new GameObject("Label");
			labelGO.transform.SetParent(parent, false);
			
			var labelRect = labelGO.AddComponent<RectTransform>();
			labelRect.sizeDelta = new Vector2(width, 20f);
			
			var layout = labelGO.AddComponent<LayoutElement>();
			layout.preferredWidth = width;
			layout.preferredHeight = 20f;

			var textComponent = labelGO.AddComponent<TextMeshProUGUI>();
			textComponent.text = text;
			textComponent.fontSize = 10f;
			textComponent.color = Color.white;
			textComponent.alignment = ConvertAlignment(alignment);
			textComponent.enableWordWrapping = false;
			textComponent.overflowMode = TextOverflowModes.Overflow;

			return labelGO;
		}

		private static TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
		{
			switch (anchor)
			{
				case TextAnchor.MiddleLeft: return TextAlignmentOptions.MidlineLeft;
				case TextAnchor.MiddleRight: return TextAlignmentOptions.MidlineRight;
				case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
				default: return TextAlignmentOptions.Center;
			}
		}

		private static TMP_InputField CreateInputField(Transform parent, string placeholder, float width)
		{
			var inputGO = new GameObject("InputField_" + placeholder);
			inputGO.transform.SetParent(parent, false);

			var rectTransform = inputGO.AddComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(width, 18f);

			var layout = inputGO.AddComponent<LayoutElement>();
			layout.preferredWidth = width;
			layout.preferredHeight = 18f;

			// Background
			var image = inputGO.AddComponent<Image>();
			image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

			// Text area
			var textAreaGO = new GameObject("TextArea");
			textAreaGO.transform.SetParent(inputGO.transform, false);
			var textAreaRect = textAreaGO.AddComponent<RectTransform>();
			textAreaRect.anchorMin = Vector2.zero;
			textAreaRect.anchorMax = Vector2.one;
			textAreaRect.offsetMin = new Vector2(3f, 1f);
			textAreaRect.offsetMax = new Vector2(-3f, -1f);

			// Add RectMask2D for proper text clipping
			textAreaGO.AddComponent<RectMask2D>();

			// Text component
			var textGO = new GameObject("Text");
			textGO.transform.SetParent(textAreaGO.transform, false);
			var textRect = textGO.AddComponent<RectTransform>();
			textRect.anchorMin = Vector2.zero;
			textRect.anchorMax = Vector2.one;
			textRect.offsetMin = Vector2.zero;
			textRect.offsetMax = Vector2.zero;

			var textComponent = textGO.AddComponent<TextMeshProUGUI>();
			textComponent.fontSize = 10f;
			textComponent.color = Color.white;
			textComponent.alignment = TextAlignmentOptions.Center;
			textComponent.enableWordWrapping = false;

			// Input field component
			var inputField = inputGO.AddComponent<TMP_InputField>();
			inputField.textViewport = textAreaRect;
			inputField.textComponent = textComponent;
			inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
			inputField.characterLimit = 7;
			inputField.caretColor = Color.white;
			inputField.caretWidth = 1;
			inputField.selectionColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);

			return inputField;
		}

		private static void CreateTargetTemperatureInput(
			TemperatureOverlayThresholdAdjustmentWidget widget,
			Scrollbar scrollbar,
			LocText centerText)
		{
			// Create a new GameObject for our input field
			targetInputContainer = new GameObject("TargetTemperatureInput");
			targetInputContainer.transform.SetParent(centerText.transform.parent, false);
			var inputGO = targetInputContainer;

			// Add RectTransform and position it where center text was
			var rectTransform = inputGO.AddComponent<RectTransform>();
			var centerRect = centerText.GetComponent<RectTransform>();
			rectTransform.anchorMin = centerRect.anchorMin;
			rectTransform.anchorMax = centerRect.anchorMax;
			rectTransform.pivot = centerRect.pivot;
			rectTransform.anchoredPosition = centerRect.anchoredPosition;
			rectTransform.sizeDelta = new Vector2(70f, 22f);

			// Background
			var image = inputGO.AddComponent<Image>();
			image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

			// Text area
			var textAreaGO = new GameObject("TextArea");
			textAreaGO.transform.SetParent(inputGO.transform, false);
			var textAreaRect = textAreaGO.AddComponent<RectTransform>();
			textAreaRect.anchorMin = Vector2.zero;
			textAreaRect.anchorMax = Vector2.one;
			textAreaRect.offsetMin = new Vector2(4f, 2f);
			textAreaRect.offsetMax = new Vector2(-4f, -2f);

			// Add RectMask2D
			textAreaGO.AddComponent<RectMask2D>();

			// Text component
			var textGO = new GameObject("Text");
			textGO.transform.SetParent(textAreaGO.transform, false);
			var textRect = textGO.AddComponent<RectTransform>();
			textRect.anchorMin = Vector2.zero;
			textRect.anchorMax = Vector2.one;
			textRect.offsetMin = Vector2.zero;
			textRect.offsetMax = Vector2.zero;

			var textComponent = textGO.AddComponent<TextMeshProUGUI>();
			textComponent.fontSize = 11f;
			textComponent.color = Color.white;
			textComponent.alignment = TextAlignmentOptions.Center;
			textComponent.enableWordWrapping = false;

			// Input field
			targetInput = inputGO.AddComponent<TMP_InputField>();
			targetInput.textViewport = textAreaRect;
			targetInput.textComponent = textComponent;
			targetInput.contentType = TMP_InputField.ContentType.DecimalNumber;
			targetInput.characterLimit = 7;
			targetInput.caretColor = Color.white;
			targetInput.caretWidth = 1;
			targetInput.selectionColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);

			// Set initial value
			UpdateTargetInputFromScrollbar(scrollbar.value);

			// Subscribe to input changes
			targetInput.onEndEdit.AddListener(OnTargetTemperatureChanged);

			// Update input when scrollbar changes
			scrollbar.onValueChanged.AddListener((float value) =>
			{
				if (targetInput != null && !targetInput.isFocused)
				{
					UpdateTargetInputFromScrollbar(value);
				}
			});

			// Hide original center text
			centerText.gameObject.SetActive(false);
		}

		private static void UpdateTargetInputFromScrollbar(float scrollValue)
		{
			var settings = TemperatureOverlaySettings.Instance;
			float kelvin;
			
			if (settings != null)
			{
				kelvin = settings.ScrollPercentageToKelvin(scrollValue);
			}
			else
			{
				// Fallback to vanilla calculation
				kelvin = DefaultSliderMin + (DefaultSliderMax - DefaultSliderMin) * scrollValue;
			}

			if (targetInput != null)
			{
				targetInput.text = FormatTemperature(kelvin);
			}
		}

		// Format absolute temperature in user's preferred units
		private static string FormatTemperature(float kelvin)
		{
			float converted = GameUtil.GetConvertedTemperature(kelvin, false);
			return converted.ToString("F0");
		}

		// Format temperature delta in user's preferred units
		private static string FormatDelta(float kelvinDelta)
		{
			float converted = ConvertKelvinDeltaToDisplay(kelvinDelta);
			return converted.ToString("F0");
		}

		// Input change handlers
		private static void OnSliderMinChanged(string value)
		{
			var settings = TemperatureOverlaySettings.Instance;
			if (settings == null) return;

			if (float.TryParse(value, out float temp))
			{
				float kelvin = GameUtil.GetTemperatureConvertedToKelvin(temp);
				settings.SliderMin = kelvin;
			}
			RefreshUIFromSettings();
		}

		private static void OnSliderMaxChanged(string value)
		{
			var settings = TemperatureOverlaySettings.Instance;
			if (settings == null) return;

			if (float.TryParse(value, out float temp))
			{
				float kelvin = GameUtil.GetTemperatureConvertedToKelvin(temp);
				settings.SliderMax = kelvin;
			}
			RefreshUIFromSettings();
		}

		private static void OnOffsetMinChanged(string value)
		{
			var settings = TemperatureOverlaySettings.Instance;
			if (settings == null) return;

			if (float.TryParse(value, out float temp))
			{
				// Offset is a delta, so we need to convert the magnitude
				float kelvinDelta = ConvertDeltaToKelvin(temp);
				settings.OffsetMin = kelvinDelta;
			}
			RefreshUIFromSettings();
		}

		private static void OnOffsetMaxChanged(string value)
		{
			var settings = TemperatureOverlaySettings.Instance;
			if (settings == null) return;

			if (float.TryParse(value, out float temp))
			{
				float kelvinDelta = ConvertDeltaToKelvin(temp);
				settings.OffsetMax = kelvinDelta;
			}
			RefreshUIFromSettings();
		}

		private static void OnExpandChanged(float value)
		{
			var settings = TemperatureOverlaySettings.Instance;
			if (settings == null) return;

			// Map 0-1 slider to 0-MaxExpandValue
			float expandKelvin = value * TemperatureOverlaySettings.MaxExpandValue;
			settings.ExpandValue = expandKelvin;
			
			UpdateExpandDisplay();
			UpdateEffectiveRangeDisplay();
		}

		private static void OnTargetTemperatureChanged(string value)
		{
			if (string.IsNullOrEmpty(value)) return;
			if (mainScrollbar == null) return;

			var settings = TemperatureOverlaySettings.Instance;

			if (float.TryParse(value, out float temperature))
			{
				float kelvin = GameUtil.GetTemperatureConvertedToKelvin(temperature);

				// Clamp to current slider range
				float minKelvin = settings?.SliderMin ?? DefaultSliderMin;
				float maxKelvin = settings?.SliderMax ?? DefaultSliderMax;

				kelvin = Mathf.Clamp(kelvin, minKelvin, maxKelvin);

				// Convert to scroll percentage
				float scrollPercentage;
				if (settings != null)
				{
					scrollPercentage = settings.KelvinToScrollPercentage(kelvin);
				}
				else
				{
					scrollPercentage = Mathf.Clamp01((kelvin - minKelvin) / (maxKelvin - minKelvin));
				}

				mainScrollbar.value = scrollPercentage;
			}
		}

		private static void OnResetAllPressed()
		{
			var settings = TemperatureOverlaySettings.Instance;
			if (settings != null)
			{
				settings.ResetToDefaults();
			}

			// Also reset the main slider to default position
			if (mainScrollbar != null)
			{
				float defaultKelvin = TemperatureOverlaySettings.DefaultSliderPosition;
				var s = TemperatureOverlaySettings.Instance;
				float scrollPercentage;
				
				if (s != null)
				{
					scrollPercentage = s.KelvinToScrollPercentage(defaultKelvin);
				}
				else
				{
					scrollPercentage = Mathf.Clamp01((defaultKelvin - DefaultSliderMin) / (DefaultSliderMax - DefaultSliderMin));
				}
				
				mainScrollbar.value = scrollPercentage;
			}

			RefreshUIFromSettings();
		}

		private static void OnSettingsChanged()
		{
			RefreshUIFromSettings();
		}

		private static void RefreshUIFromSettings()
		{
			var settings = TemperatureOverlaySettings.Instance;
			
			// Use settings if available, otherwise use defaults
			float sMin = settings?.SliderMin ?? DefaultSliderMin;
			float sMax = settings?.SliderMax ?? DefaultSliderMax;
			float oMin = settings?.OffsetMin ?? DefaultOffsetMin;
			float oMax = settings?.OffsetMax ?? DefaultOffsetMax;
			float expand = settings?.ExpandValue ?? 0f;

			// Update slider range inputs
			if (sliderMinInput != null && !sliderMinInput.isFocused)
			{
				sliderMinInput.text = FormatTemperature(sMin);
			}
			if (sliderMaxInput != null && !sliderMaxInput.isFocused)
			{
				sliderMaxInput.text = FormatTemperature(sMax);
			}

			// Update offset inputs
			if (offsetMinInput != null && !offsetMinInput.isFocused)
			{
				offsetMinInput.text = FormatDelta(oMin);
			}
			if (offsetMaxInput != null && !offsetMaxInput.isFocused)
			{
				offsetMaxInput.text = FormatDelta(oMax);
			}

			// Update expand slider
			if (expandSlider != null)
			{
				expandSlider.value = expand / TemperatureOverlaySettings.MaxExpandValue;
			}

			UpdateExpandDisplay();
			UpdateEffectiveRangeDisplay();
		}

		private static void UpdateExpandDisplay()
		{
			if (expandValueText == null) return;

			var settings = TemperatureOverlaySettings.Instance;
			float expand = settings?.ExpandValue ?? 0f;
			float displayExpand = ConvertKelvinDeltaToDisplay(expand);
			string suffix = GetTemperatureUnitSuffix();
			expandValueText.text = $"+{displayExpand:F0}{suffix}";
		}

		private static void UpdateEffectiveRangeDisplay()
		{
			if (effectiveRangeText == null) return;

			var settings = TemperatureOverlaySettings.Instance;
			float oMin = settings?.OffsetMin ?? DefaultOffsetMin;
			float oMax = settings?.OffsetMax ?? DefaultOffsetMax;
			float expand = settings?.ExpandValue ?? 0f;

			float effectiveMin = oMin - expand;
			float effectiveMax = oMax + expand;

			float minDisplay = ConvertKelvinDeltaToDisplay(effectiveMin);
			float maxDisplay = ConvertKelvinDeltaToDisplay(effectiveMax);
			effectiveRangeText.text = $"{minDisplay:F0} to +{maxDisplay:F0}";
		}

		// Helper methods for temperature conversion
		private static string GetTemperatureUnitSuffix()
		{
			return GameUtil.GetTemperatureUnitSuffix();
		}

		private static float ConvertDeltaToKelvin(float delta)
		{
			// For temperature deltas, C and K have the same magnitude
			// F needs conversion: ΔF = ΔK * 9/5
			var unit = GameUtil.temperatureUnit;
			switch (unit)
			{
				case GameUtil.TemperatureUnit.Fahrenheit:
					return delta * 5f / 9f; // Convert ΔF to ΔK
				default:
					return delta; // C and K deltas are the same
			}
		}

		private static float ConvertKelvinDeltaToDisplay(float kelvinDelta)
		{
			var unit = GameUtil.temperatureUnit;
			switch (unit)
			{
				case GameUtil.TemperatureUnit.Fahrenheit:
					return kelvinDelta * 9f / 5f; // Convert ΔK to ΔF
				default:
					return kelvinDelta; // K and C deltas are the same
			}
		}
	}
}
