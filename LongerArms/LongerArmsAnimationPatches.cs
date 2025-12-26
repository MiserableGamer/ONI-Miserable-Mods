using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PeterHan.PLib.Options;
using UnityEngine;

namespace LongerArms
{
	// Separate file for animation range extension patches.
	// This allows working on animation patches without affecting the core reach functionality.
	public static class LongerArmsAnimationPatches
	{
		private const bool ENABLE_ANIMATION_PATCH = false; // Change this to enable/disable the patch

		// Animation update range - used to extend animation handler update range beyond vanilla 4 cells.
		// Returns vanilla value (4) if animation patch is disabled.
		// RESTORE POINT: To restore vanilla behavior, set this to return 4 or remove the AnimEventHandlerManager patch entirely.
		internal static int GetAnimationUpdateRange()
		{
			if (!ENABLE_ANIMATION_PATCH)
			{
				return 4; // Return vanilla value if patch is disabled
			}

			// Read current reach settings to calculate appropriate animation update range
			var options = POptions.ReadSettings<LongerArmsOptions>() ?? new LongerArmsOptions();
			int maxReach = Math.Max(options.HorizontalReach, options.VerticalReach);

			// Vanilla uses 4, we extend based on max reach (vanilla base 3 + additional reach + safety buffer)
			// Add buffer to ensure animations work at maximum reach
			int animationRange = 3 + maxReach + 2; // Base 3 + max additional + 2 cell buffer

			// Cap at reasonable maximum to avoid performance issues
			return Math.Min(animationRange, 15);
		}

		// Patch to extend animation handler update range beyond vanilla 4 cells.
		// The game's AnimEventHandlerManager.LateUpdate() only updates handlers within 4 cells,
		// which causes animations to stop working beyond that distance. This patch extends
		// the range to match extended reach capabilities.
		// RESTORE POINT: To restore vanilla behavior, comment out or remove this entire patch class.
		[HarmonyPatch(typeof(AnimEventHandlerManager), "LateUpdate")]
		public static class AnimEventHandlerManager_LateUpdate_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				// Check if animation patch is enabled - if not, return original instructions unchanged
				if (!ENABLE_ANIMATION_PATCH)
				{
					return instructions; // Return original instructions if patch is disabled
				}

				// Get the method we want to call - validate it exists first
				MethodInfo getRangeMethod = typeof(LongerArmsAnimationPatches).GetMethod("GetAnimationUpdateRange", BindingFlags.NonPublic | BindingFlags.Static);

				if (getRangeMethod == null)
				{
					Debug.LogError("[LongerArms] AnimEventHandlerManager.LateUpdate transpiler: CRITICAL ERROR - GetAnimationUpdateRange method not found! Animation patch will not work.");
					return instructions; // Return original instructions if method lookup failed
				}

				// Get the method we're looking for in the IL (GetVisibleCellRangeInActiveWorld)
				MethodInfo getVisibleRangeMethod = typeof(Grid).GetMethod("GetVisibleCellRangeInActiveWorld", BindingFlags.Public | BindingFlags.Static);

				if (getVisibleRangeMethod == null)
				{
					Debug.LogWarning("[LongerArms] AnimEventHandlerManager.LateUpdate transpiler: Could not find Grid.GetVisibleCellRangeInActiveWorld method for pattern matching. Will use fallback pattern.");
				}

				var codes = instructions.ToList();
				bool found = false;

				for (int i = 0; i < codes.Count - 2; i++) // Need at least 2 more instructions ahead
				{
					// Check if we've already patched this (transpiler may run multiple times)
					if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo existingCall && existingCall == getRangeMethod)
					{
						// Already patched - skip to avoid re-patching
						found = true;
						break;
					}

					// Look for the pattern: ldc.i4.4 followed by a float load, then Call GetVisibleCellRangeInActiveWorld
					// This ensures we're matching the specific ldc.i4.4 used as the third argument to GetVisibleCellRangeInActiveWorld
					if (codes[i].opcode == OpCodes.Ldc_I4_4)
					{
						// Check if this is followed by a float load (ldc.r4 or ldc.r8 for the 1.5f parameter)
						bool hasFloatLoad = (codes[i + 1].opcode == OpCodes.Ldc_R4 || codes[i + 1].opcode == OpCodes.Ldc_R8);

						// Check if that's followed by a Call instruction
						if (hasFloatLoad && i + 2 < codes.Count && codes[i + 2].opcode == OpCodes.Call)
						{
							// Verify it's calling GetVisibleCellRangeInActiveWorld
							// If method lookup failed, just check if operand is a MethodInfo (fallback)
							bool matchesTarget = false;
							if (getVisibleRangeMethod != null && codes[i + 2].operand is MethodInfo callTarget)
							{
								matchesTarget = (callTarget == getVisibleRangeMethod || callTarget.Name == "GetVisibleCellRangeInActiveWorld");
							}
							else if (getVisibleRangeMethod == null && codes[i + 2].operand is MethodInfo callTarget2)
							{
								// Fallback: just check the method name
								matchesTarget = (callTarget2.Name == "GetVisibleCellRangeInActiveWorld" && callTarget2.DeclaringType == typeof(Grid));
							}

							if (matchesTarget)
							{
								// This is the ldc.i4.4 we want to replace!
								codes[i] = new CodeInstruction(OpCodes.Call, getRangeMethod);
								found = true;

								// Always log this critical patch operation (not just when debug logs enabled)
								Debug.Log("[LongerArms] AnimEventHandlerManager.LateUpdate transpiler: Successfully replaced hardcoded range 4 with dynamic GetAnimationUpdateRange() call");

								break; // Only replace the first occurrence (the one we want)
							}
						}
					}
				}

				if (!found)
				{
					Debug.LogError("[LongerArms] AnimEventHandlerManager.LateUpdate transpiler: CRITICAL ERROR - Could not find ldc.i4.4 instruction (for GetVisibleCellRangeInActiveWorld) to patch. Animation patch failed!");
				}

				return codes;
			}
		}
	}
}

