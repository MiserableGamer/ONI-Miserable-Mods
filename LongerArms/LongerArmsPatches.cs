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
	public static class LongerArmsPatches
	{
	private static bool OffsetsExpanded = false;
	private static bool EnableDebugLogs = false;

	private static List<CellOffset[]> GenerateVerticalReachOffsets(int additionalReach, int vanillaMax)
		{
			var paths = new List<CellOffset[]>();
			
			if (additionalReach <= 0)
				return paths;

			// Generate a path for each additional cell we want to reach vertically
			for (int i = 1; i <= additionalReach; i++)
			{
				int targetCell = vanillaMax + i;
				var offsets = new List<CellOffset>();
				
				// Create path from target cell back to cell 1 (descending order)
				for (int cellNumber = targetCell; cellNumber >= 1; cellNumber--)
				{
					offsets.Add(new CellOffset(0, -cellNumber)); 
				}
				
				paths.Add(offsets.ToArray());
			}
			
			return paths;
		}

		/// Generates horizontal cell offset paths for additional reach beyond vanilla.
		/// CRITICAL: The game's offset table system does NOT validate line-of-sight through walls.
		/// CURRENT APPROACH: Only diagonal/elevated paths are capped (they combine vertical+horizontal movement).
		/// Pure horizontal (Y=0) and pure vertical paths are allowed up to 10, as they don't seem to cause reach-through-walls.
		/// 
		/// RESTORE POINT: If reach-through-walls issues persist, restore the previous implementation that capped
		/// ALL horizontal reach at 2 when safe mode is enabled. Look for "RESTORE POINT" comments below.
		private static List<CellOffset[]> GenerateHorizontalReachOffsets(int additionalReach, int vanillaMax, int totalVerticalReach, bool safeMode)
		{
			var paths = new List<CellOffset[]>();
			
			if (additionalReach <= 0)
				return paths;

			// NEW APPROACH: Don't cap pure horizontal reach - only limit diagonal/elevated paths
			// Pure horizontal paths (Y=0, same level) and pure vertical paths (X=0) don't cause reach-through-walls issues
			// Only diagonal paths (combining vertical+horizontal) are problematic
			// RESTORE POINT: To restore previous behavior, uncomment this line and use effectiveHorizontalReach below:
			// int effectiveHorizontalReach = safeMode ? Math.Min(additionalReach, 2) : additionalReach;
			int effectiveHorizontalReach = additionalReach; // Allow full horizontal reach for same-level paths
			
			if (EnableDebugLogs && !safeMode && additionalReach > 2)
			{
				Debug.LogWarning($"[LongerArms] Safe mode disabled: Using full horizontal reach of {additionalReach} with unlimited diagonal paths - reach-through-walls may occur");
			}

			// Generate horizontal paths at same level (Y=0) - CRITICAL for ladder reachability
			// Pattern based on DuplicantReacharound: O(target,0), O(1,0), O(2,0), ... O(target-1,0)
			// Each path includes the target cell and all intermediate horizontal steps
			// These are PURE HORIZONTAL paths (Y=0) and don't cause reach-through-walls issues
			for (int i = 1; i <= effectiveHorizontalReach; i++)
			{
				int targetCell = vanillaMax + i; // Extend beyond vanilla max
				var offsets = new List<CellOffset>();
				
				// Target cell first (furthest horizontal)
				offsets.Add(new CellOffset(targetCell, 0));
				
				// Then include ALL intermediate cells in sequence (required for proper validation)
				// This pattern ensures each step is explicitly defined
				offsets.Add(new CellOffset(1, 0));
				if (targetCell >= 3)
				{
					for (int j = 2; j < targetCell; j++)
					{
						offsets.Add(new CellOffset(j, 0));
					}
				}
				
				paths.Add(offsets.ToArray());
			}

			// Generate horizontal paths at elevated levels (reach up/down then horizontal)
			// Pattern based on DuplicantReacharound: O(X, -Y), O(1,0), O(2,0), ..., O(X,0), then vertical steps back
			// SAFE MODE: Limit diagonal distance in the first offset to prevent reach-through-walls
			// UNSAFE MODE: Allow full diagonal reach (may cause reach-through-walls)
			if (safeMode)
			{
				// Safe mode: Limit diagonal distance to prevent reach-through-walls
				// Only generate paths where max(i, verticalOffset) <= maxSafeDiagonal
				// This prevents large diagonal paths like O(10, -5) that cause reach-through-walls
				// while still allowing small diagonal paths like O(3, -2) for ladder reachability
				int maxSafeDiagonal = 3; // Safe diagonal distance: max(i, verticalOffset) <= this value
				
				// Limit vertical offset loop to safe diagonal range
				int maxSafeVerticalOffset = Math.Min(totalVerticalReach, maxSafeDiagonal);
				
				for (int verticalOffset = 1; verticalOffset <= maxSafeVerticalOffset; verticalOffset++)
				{
					// For each vertical offset, limit horizontal to ensure max(i, verticalOffset) <= maxSafeDiagonal
					int maxHorizontalAtThisLevel = maxSafeDiagonal;
					
					for (int i = 1; i <= maxHorizontalAtThisLevel; i++)
					{
						// Ensure diagonal distance constraint: max(i, verticalOffset) <= maxSafeDiagonal
						if (Math.Max(i, verticalOffset) > maxSafeDiagonal)
							continue;
						
						var offsets = new List<CellOffset>();
						
						// Go to elevated position first (diagonal offset - limited in safe mode)
						offsets.Add(new CellOffset(i, -verticalOffset));
						
						// Then horizontal progression at that level - include ALL intermediate steps
						offsets.Add(new CellOffset(1, 0));
						if (i >= 2)
						{
							for (int j = 2; j <= i; j++)
							{
								offsets.Add(new CellOffset(j, 0));
							}
						}
						
						// Then intermediate vertical steps back down - include ALL steps
						for (int step = 1; step < verticalOffset; step++)
						{
							offsets.Add(new CellOffset(i, -step));
						}
						
						paths.Add(offsets.ToArray());
					}
				}
			}
			else
			{
				// Unsafe mode: Allow full diagonal reach - no limits on diagonal distance
				// This allows reaching through walls but gives maximum reach capability
				for (int verticalOffset = 1; verticalOffset <= totalVerticalReach; verticalOffset++)
				{
					for (int i = 1; i <= effectiveHorizontalReach; i++)
					{
						var offsets = new List<CellOffset>();
						
						// Go to elevated position first (diagonal offset - unlimited in unsafe mode)
						offsets.Add(new CellOffset(i, -verticalOffset));
						
						// Then horizontal progression at that level - include ALL intermediate steps
						offsets.Add(new CellOffset(1, 0));
						if (i >= 2)
						{
							for (int j = 2; j <= i; j++)
							{
								offsets.Add(new CellOffset(j, 0));
							}
						}
						
						// Then intermediate vertical steps back down - include ALL steps
						for (int step = 1; step < verticalOffset; step++)
						{
							offsets.Add(new CellOffset(i, -step));
						}
						
						paths.Add(offsets.ToArray());
					}
				}
			}
			
			return paths;
		}


		[HarmonyPatch(typeof(Game), "OnPrefabInit")]
		public static class GamePatch
		{
			public static void Postfix()
			{
				GamePatch.ExpandTables();
			}

			public static void ExpandTables()
			{
				if (!OffsetsExpanded)
				{
					// Read options to get reach distances
					var options = POptions.ReadSettings<LongerArmsOptions>() ?? new LongerArmsOptions();
					int verticalReach = options.VerticalReach;
					int horizontalReach = options.HorizontalReach;
					bool safeMode = options.SafeMode;

					if (EnableDebugLogs)
						Debug.Log($"[LongerArms] ExpandTables called, verticalReach = {verticalReach}, horizontalReach = {horizontalReach}, safeMode = {safeMode}");

					// Total vertical reach includes vanilla (3) plus additional vertical reach
					int totalVerticalReach = 3 + verticalReach;
					List<CellOffset[]> horizontalPaths = GenerateHorizontalReachOffsets(horizontalReach, 3, totalVerticalReach, safeMode);

					// Generate vertical paths
					List<CellOffset[]> verticalPaths = GenerateVerticalReachOffsets(verticalReach, 3);

					// Combine all paths and add in single call to avoid double-mirroring issues
					List<CellOffset[]> allPaths = new List<CellOffset[]>();
					allPaths.AddRange(horizontalPaths);
					allPaths.AddRange(verticalPaths);

					if (allPaths.Count > 0)
					{
						if (EnableDebugLogs)
							Debug.Log($"[LongerArms] Generated {horizontalPaths.Count} horizontal path(s) and {verticalPaths.Count} vertical path(s) for additional reach");
						
						ExpandTable(ref OffsetGroups.InvertedStandardTable, allPaths);
						ExpandTable(ref OffsetGroups.InvertedStandardTableWithCorners, allPaths);
						
						if (EnableDebugLogs)
							Debug.Log($"[LongerArms] Tables expanded successfully with {allPaths.Count} total path(s)");
					}
					else
					{
						if (EnableDebugLogs)
							Debug.LogWarning("[LongerArms] No paths generated!");
					}

					OffsetsExpanded = true;
				}
			}

			/// Expands an offset table by adding new reach offset paths.
			public static void ExpandTable(ref CellOffset[][] inputTable, List<CellOffset[]> newPaths)
			{
				if (newPaths == null || newPaths.Count == 0)
					return;

				CellOffset[][] array = OffsetTable.Mirror(inputTable.ToList<CellOffset[]>().Concat(newPaths).ToArray<CellOffset[]>());
				inputTable = array;
			}
		}
	}
}

