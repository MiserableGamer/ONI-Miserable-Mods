using System;
using System.Collections.Generic;
using System.Linq;
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
		private static List<CellOffset[]> GenerateHorizontalReachOffsets(int additionalReach, int vanillaMax, int totalVerticalReach)
		{
			var paths = new List<CellOffset[]>();
			
			if (additionalReach <= 0)
				return paths;

			for (int verticalOffset = 0; verticalOffset <= totalVerticalReach; verticalOffset++)
			{

				for (int i = 1; i <= additionalReach; i++)
				{
					int targetCell = vanillaMax + i;
					var offsets = new List<CellOffset>();
					
					for (int cellNumber = targetCell; cellNumber >= 1; cellNumber--)
					{
						offsets.Add(new CellOffset(cellNumber, -verticalOffset)); 
					}
					
					paths.Add(offsets.ToArray());
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

					if (EnableDebugLogs)
						Debug.Log($"[LongerArms] ExpandTables called, verticalReach = {verticalReach}, horizontalReach = {horizontalReach}");

					int totalVerticalReach = 3;
					List<CellOffset[]> horizontalPaths = GenerateHorizontalReachOffsets(horizontalReach, 3, totalVerticalReach);

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

