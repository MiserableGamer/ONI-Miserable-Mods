using HarmonyLib;
using UnityEngine;

namespace ControlledSublimation.Patches
{
	// Main patch for controlling sublimation
	[HarmonyPatch(typeof(Sublimates), nameof(Sublimates.Sim200ms))]
	public static class Sublimates_Sim200ms_Patch
	{
		// Prefix runs before the original method
		public static bool Prefix(Sublimates __instance, ref float dt)
		{
			if (__instance == null) return true;

			var primaryElement = __instance.GetComponent<PrimaryElement>();
			if (primaryElement == null) return true;

			var elementId = primaryElement.ElementID;
			var options = ControlledSublimationOptions.Instance;

			// Get settings for this element - returns null if not a controlled element
			ElementSettings? settings = GetElementSettings(options, elementId);

			// If not a controlled element, allow normal behavior
			if (!settings.HasValue)
				return true;

			var s = settings.Value;

			// If rate multiplier is 0, completely disable sublimation
			if (s.RateMultiplier <= 0f)
				return false;

			// Check if item is in storage (has Stored tag)
			bool isStored = __instance.HasTag(GameTags.Stored);

			if (isStored)
			{
				// Item is in storage
				if (!s.AllowInStorage)
				{
					// Check if it's in an excluded storage type that should always emit
					var pickupable = __instance.GetComponent<Pickupable>();
					if (pickupable != null && pickupable.storage != null)
					{
						var storagePrefabId = pickupable.storage.PrefabID();
						// Oxysconce (Bleach Stone Sconce) is designed to emit - always allow
						if (storagePrefabId == (Tag)"Oxysconce")
						{
							// Apply rate multiplier via dt and allow
							dt *= s.RateMultiplier;
							return true;
						}
					}
					return false;
				}
			}
			else
			{
				// Item is debris (not in storage)
				if (!s.AllowAsDebris)
					return false;
			}

			// Apply rate multiplier by scaling dt
			// Since rate calculation is: sublimationRate * dt
			// Scaling dt effectively scales the emission rate
			if (s.RateMultiplier != 1.0f)
			{
				dt *= s.RateMultiplier;
			}

			// Allow sublimation to proceed with (potentially modified) dt
			return true;
		}

		// Returns null for elements we don't control, allowing them to behave normally
		private static ElementSettings? GetElementSettings(ControlledSublimationOptions options, SimHashes elementId)
		{
			switch (elementId)
			{
				case SimHashes.BleachStone:
					return new ElementSettings(
						options.BleachStoneDebris,
						options.BleachStoneInStorage,
						options.BleachStoneMultiplier);

				case SimHashes.OxyRock: // Oxylite
					return new ElementSettings(
						options.OxyliteDebris,
						options.OxyliteInStorage,
						options.OxyliteMultiplier);

				case SimHashes.ToxicSand: // Polluted Dirt
					return new ElementSettings(
						options.PollutedDirtDebris,
						options.PollutedDirtInStorage,
						options.PollutedDirtMultiplier);

				case SimHashes.SlimeMold: // Slime
					return new ElementSettings(
						options.SlimeDebris,
						options.SlimeInStorage,
						options.SlimeMultiplier);

				case SimHashes.DirtyWater: // Polluted Water
					return new ElementSettings(
						options.PollutedWaterDebris,
						options.PollutedWaterInStorage,
						options.PollutedWaterMultiplier);

				case SimHashes.ToxicMud: // Polluted Mud (Spaced Out DLC)
					return new ElementSettings(
						options.PollutedMudDebris,
						options.PollutedMudInStorage,
						options.PollutedMudMultiplier);

				default:
					// Unknown/uncontrolled element - return null to allow normal behavior
					return null;
			}
		}
	}
}
