using HarmonyLib;

namespace DebugFogOfWar
{
	/// Patches to prevent fog of war from being removed when debug mode is activated
	public static class DebugFogOfWarPatches
	{
		/// Intercepts the debug toggle key press to prevent map discovery.
		/// This patch replaces the original DebugHandler.OnKeyDown behavior for DebugToggle,
		/// handling the UI toggling without triggering map discovery.
		[HarmonyPatch(typeof(DebugHandler), nameof(DebugHandler.OnKeyDown))]
		public static class DebugHandler_OnKeyDown_Patch
		{
			internal static bool Prefix(DebugHandler __instance, KButtonEvent e)
			{
				// Skip processing if debug mode is not enabled - let original method handle it
				if (!DebugHandler.enabled)
				{
					return true;
				}

				// Only intercept the DebugToggle action, let other actions pass through
				bool isDebugToggle = e.TryConsume(global::Action.DebugToggle);
				if (!isDebugToggle)
				{
					return true;
				}

				// Process debug toggle without triggering map discovery
				ProcessDebugToggle();

				// Prevent original method from running to avoid map discovery
				return false;
			}

			/// Handles the debug toggle UI state changes without triggering map discovery
			private static void ProcessDebugToggle()
			{
				// Update camera free mode state
				UpdateCameraState();

				// Update debug UI screens and buttons
				UpdateDebugUI();
			}

			/// Toggles the free camera enabled state
			private static void UpdateCameraState()
			{
				var cameraController = CameraController.Instance;
				if (cameraController != null)
				{
					cameraController.FreeCameraEnabled = !cameraController.FreeCameraEnabled;
				}
			}

			/// Updates all debug UI elements (screens, menus, buttons) to match the toggle state
			private static void UpdateDebugUI()
			{
				var debugScreen = DebugPaintElementScreen.Instance;
				if (debugScreen == null)
				{
					return;
				}

				// Get current active state and toggle it
				bool isCurrentlyActive = debugScreen.gameObject.activeSelf;
				debugScreen.gameObject.SetActive(!isCurrentlyActive);

				// Close element menu if it's open
				CloseElementMenuIfOpen();

				// Update base template button to match screen state
				UpdateBaseTemplateButton(!isCurrentlyActive);
			}

			/// Closes the debug element menu if it's currently open
			private static void CloseElementMenuIfOpen()
			{
				var elementMenu = DebugElementMenu.Instance;
				if (elementMenu != null && elementMenu.root.activeSelf)
				{
					elementMenu.root.SetActive(false);
				}
			}

			/// Updates the debug base template button to match the specified active state
			private static void UpdateBaseTemplateButton(bool shouldBeActive)
			{
				var templateButton = DebugBaseTemplateButton.Instance;
				if (templateButton != null)
				{
					templateButton.gameObject.SetActive(shouldBeActive);
				}
			}
		}
	}
}

