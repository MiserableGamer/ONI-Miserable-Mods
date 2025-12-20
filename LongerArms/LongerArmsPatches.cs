using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace LongerArms
{
	public class LongerArmsPatches : UserMod2
	{
		private static bool OffsetsExpanded = false;

		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			
			// Initialize PLib
			PUtil.InitLibrary();
			
			// Register options
			var options = new POptions();
			options.RegisterOptions(this, typeof(LongerArmsOptions));
			
			// Apply Harmony patches
			harmony.PatchAll();
		}

		/// <summary>
		/// Generates vertical cell offset paths for additional reach beyond vanilla.
		/// Based on CrackReacher's exact pattern: single path at horizontal offset 0.
		/// CrackReacher uses: (0, -4), (0, -3), (0, -2), (0, -1) to reach cell 4.
		/// The OffsetTable.Mirror call handles mirroring to all directions.
		/// </summary>
		private static List<CellOffset[]> GenerateVerticalReachOffsets(int additionalReach, int vanillaMax)
		{
			var paths = new List<CellOffset[]>();
			
			if (additionalReach <= 0)
				return paths;

			// Generate a path for each additional cell we want to reach vertically
			// EXACTLY like CrackReacher: only at horizontal offset 0
			// Vanilla vertical reach is typically 3 cells (cells 1-3 from center at 0)
			// So we add cells starting from 4 and going up
			for (int i = 1; i <= additionalReach; i++)
			{
				int targetCell = vanillaMax + i;
				var offsets = new List<CellOffset>();
				
				// Create path from target cell back to cell 1 (descending order)
				// EXACTLY like CrackReacher: (0, -4), (0, -3), (0, -2), (0, -1) for targetCell=4
				// Only at horizontal offset 0 - Mirror handles all directions
				for (int cellNumber = targetCell; cellNumber >= 1; cellNumber--)
				{
					offsets.Add(new CellOffset(0, -cellNumber)); // Negative Y is upward, horizontal offset is always 0
				}
				
				paths.Add(offsets.ToArray());
			}
			
			return paths;
		}

		/// <summary>
		/// Generates horizontal cell offset paths for additional reach beyond vanilla.
		/// The offset arrays represent PATHS from the target cell back to the origin.
		/// Each cell in the path must be validated (not solid) by the game.
		/// In vanilla, horizontal reach can extend to cells within the vertical limit,
		/// so we generate horizontal paths at multiple vertical offsets.
		/// </summary>
		private static List<CellOffset[]> GenerateHorizontalReachOffsets(int additionalReach, int vanillaMax, int totalVerticalReach)
		{
			var paths = new List<CellOffset[]>();
			
			if (additionalReach <= 0)
				return paths;

			// Total vertical reach = vanilla (3) + additional vertical reach
			// Horizontal reach should work at all vertical offsets from 0 to -totalVerticalReach
			// Generate horizontal paths at each vertical offset level
			for (int verticalOffset = 0; verticalOffset <= totalVerticalReach; verticalOffset++)
			{
				// Generate a path for each additional cell we want to reach horizontally at this vertical level
				// Vanilla horizontal reach is typically 3 cells (cells 1-3 from center at 0)
				// So we add cells starting from 4 and going up
				for (int i = 1; i <= additionalReach; i++)
				{
					int targetCell = vanillaMax + i;
					var offsets = new List<CellOffset>();
					
					// Create path from target cell back to cell 1 (descending order)
					// For horizontal: positive X is right, negative X is left
					// Include vertical offset: negative Y is upward
					for (int cellNumber = targetCell; cellNumber >= 1; cellNumber--)
					{
						offsets.Add(new CellOffset(cellNumber, -verticalOffset)); // Positive X is right, negative Y is up
					}
					
					paths.Add(offsets.ToArray());
				}
			}
			
			return paths;
		}

		// Diagonal reach functionality removed for v1.0 release
		// Will be re-implemented in a future version

		/// <summary>
		/// Checks if two CellOffset arrays are equal.
		/// </summary>
		private static bool Equals(CellOffset[] a, CellOffset[] b)
		{
			if (a == null || b == null || a.Length != b.Length || a.Length == 0)
				return false;

			for (int i = 0; i < a.Length; i++)
			{
				if (!a[i].Equals(b[i]))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Checks if a table contains a specific CellOffset array.
		/// </summary>
		private static bool Contains(CellOffset[][] table, CellOffset[] array)
		{
			foreach (CellOffset[] a in table)
			{
				if (Equals(a, array))
					return true;
			}
			return false;
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

					// Debug logging
					Debug.Log($"[LongerArms] ExpandTables called, verticalReach = {verticalReach}, horizontalReach = {horizontalReach}");

					// Generate horizontal paths - use vanilla vertical reach (3) - this was working
					int totalVerticalReach = 3; // Vanilla max is 3 - keep this for horizontal (was working)
					List<CellOffset[]> horizontalPaths = GenerateHorizontalReachOffsets(horizontalReach, 3, totalVerticalReach);

					// Generate vertical paths
					List<CellOffset[]> verticalPaths = GenerateVerticalReachOffsets(verticalReach, 3);

					// Combine all paths and add in single call to avoid double-mirroring issues
					List<CellOffset[]> allPaths = new List<CellOffset[]>();
					allPaths.AddRange(horizontalPaths);
					allPaths.AddRange(verticalPaths);

					if (allPaths.Count > 0)
					{
						Debug.Log($"[LongerArms] Generated {horizontalPaths.Count} horizontal path(s) and {verticalPaths.Count} vertical path(s) for additional reach");
						
						// Add all paths to both tables - like CrackReacher does
						// This is required for the paths to work correctly
						ExpandTable(ref OffsetGroups.InvertedStandardTable, allPaths);
						ExpandTable(ref OffsetGroups.InvertedStandardTableWithCorners, allPaths);
						
						Debug.Log($"[LongerArms] Tables expanded successfully with {allPaths.Count} total path(s)");
					}
					else
					{
						Debug.LogWarning("[LongerArms] No paths generated!");
					}

					OffsetsExpanded = true;
				}
			}

			/// <summary>
			/// Expands an offset table by adding multiple new reach offset paths at once.
			/// This is more efficient than adding them one by one, as it only mirrors once.
			/// </summary>
			public static void ExpandTable(ref CellOffset[][] inputTable, List<CellOffset[]> newPaths)
			{
				if (newPaths == null || newPaths.Count == 0)
					return;

				// Convert existing table to list
				List<CellOffset[]> list = inputTable.ToList();
				
				// Convert list to array for Contains check
				CellOffset[][] existingTable = list.ToArray();
				
				// Add only new paths that don't already exist
				foreach (CellOffset[] path in newPaths)
				{
					if (!Contains(existingTable, path))
					{
						list.Add(path);
					}
				}
				
				// Mirror all paths at once (much more efficient than mirroring after each addition)
				CellOffset[][] array = OffsetTable.Mirror(list.ToArray());
				inputTable = array;
			}
		}

		// Diagonal reach patch removed for v1.0 release
		// Will be re-implemented in a future version
	}
}

