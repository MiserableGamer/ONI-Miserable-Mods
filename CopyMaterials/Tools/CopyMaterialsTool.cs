using System;
using System.Collections.Generic;
using UnityEngine;

namespace CopyMaterialsTool
{
    public sealed class CopyMaterialsTool : InterfaceTool
    {
        internal static bool DEBUG_LOGS = true;

        private bool isDragging;
        private int originCell = Grid.InvalidCell;
        private int currentCell = Grid.InvalidCell;

        private GameObject dragBoxGO;
        private LineRenderer line;

        private readonly HashSet<int> visited = new HashSet<int>();

        protected override void OnActivateTool()
        {
            base.OnActivateTool();

            isDragging = false;
            originCell = Grid.InvalidCell;
            currentCell = Grid.InvalidCell;
            visited.Clear();

            EnsureDragBox();
            HideDragBox();

            PendingRebuildManager.DEBUG_LOGS = DEBUG_LOGS;
            RebuildApply.DEBUG_LOGS = DEBUG_LOGS;

            Log("Tool activated");
        }

        protected override void OnDeactivateTool(InterfaceTool newTool)
        {
            base.OnDeactivateTool(newTool);

            isDragging = false;
            originCell = Grid.InvalidCell;
            currentCell = Grid.InvalidCell;
            visited.Clear();

            HideDragBox();
            Log("Tool deactivated");
        }

        public override void OnLeftClickDown(Vector3 cursor_pos)
        {
            // COPY if clipboard empty
            if (!MaterialClipboard.HasData)
            {
                GameObject src = GetBuildingUnderCursor(cursor_pos);
                if (src == null)
                    return;

                MaterialClipboard.SetFromSource(src);
                if (!MaterialClipboard.HasData)
                {
                    PopFXManager.Instance.SpawnFX(
                        PopFXManager.Instance.sprite_Negative,
                        "No materials to copy",
                        src.transform,
                        Vector3.zero);
                    return;
                }

                Log("Copied PrefabID=" + MaterialClipboard.SourcePrefabID + " mats=" + MaterialClipboard.CopiedMaterialTags.Length);

                PopFXManager.Instance.SpawnFX(
                    PopFXManager.Instance.sprite_Plus,
                    "Materials copied",
                    src.transform,
                    Vector3.zero);
                return;
            }

            // APPLY: start selection
            isDragging = true;
            visited.Clear();

            originCell = Grid.PosToCell(cursor_pos);
            if (!Grid.IsValidCell(originCell))
            {
                originCell = Grid.InvalidCell;
                return;
            }

            currentCell = originCell;

            EnsureDragBox();
            ShowDragBox();
            UpdateDragBox(originCell, currentCell);
        }

        public override void OnMouseMove(Vector3 cursor_pos)
        {
            if (!isDragging) return;
            if (originCell == Grid.InvalidCell) return;

            int cell = Grid.PosToCell(cursor_pos);
            if (!Grid.IsValidCell(cell)) return;
            if (cell == currentCell) return;

            currentCell = cell;
            UpdateDragBox(originCell, currentCell);
        }

        public override void OnLeftClickUp(Vector3 cursor_pos)
        {
            if (!isDragging)
            {
                base.OnLeftClickUp(cursor_pos);
                return;
            }

            isDragging = false;

            int endCell = Grid.PosToCell(cursor_pos);
            if (!Grid.IsValidCell(endCell))
                endCell = currentCell;
            if (!Grid.IsValidCell(endCell))
                endCell = originCell;

            if (originCell != Grid.InvalidCell && Grid.IsValidCell(endCell))
                ApplyRect(originCell, endCell);

            originCell = Grid.InvalidCell;
            currentCell = Grid.InvalidCell;

            HideDragBox();
            base.OnLeftClickUp(cursor_pos);
        }

        private void ApplyRect(int a, int b)
        {
            int minX, maxX, minY, maxY;
            GetRectBounds(a, b, out minX, out maxX, out minY, out maxY);

            visited.Clear();

            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                    ApplyAtCell(Grid.XYToCell(x, y));
        }

        private void ApplyAtCell(int cell)
        {
            if (!Grid.IsValidCell(cell))
                return;

            GameObject go = Grid.Objects[cell, (int)ObjectLayer.Building];
            if (go == null)
                return;

            int id = go.GetInstanceID();
            if (!visited.Add(id))
                return;

            string targetPrefabId = TryGetPrefabId(go);
            if (string.IsNullOrEmpty(targetPrefabId))
                return;

            // exact building type only
            if (!string.Equals(targetPrefabId, MaterialClipboard.SourcePrefabID, StringComparison.Ordinal))
                return;

            var result = RebuildApply.TryApplyAndQueueRebuild(go, MaterialClipboard.CopiedMaterialTags);

            if (result == RebuildApply.ApplyResult.Applied)
                PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, "Rebuild queued", go.transform, Vector3.zero);
        }

        private static string TryGetPrefabId(GameObject go)
        {
            try
            {
                var bc = go.GetComponent<BuildingComplete>();
                if (bc != null && bc.Def != null)
                    return bc.Def.PrefabID;
            }
            catch { }
            return null;
        }

        // ---------------------------
        // Drag box visuals (cell-edge aligned)
        // ---------------------------

        private void EnsureDragBox()
        {
            if (dragBoxGO != null)
                return;

            dragBoxGO = new GameObject("CopyMaterialsDragBox");
            UnityEngine.Object.DontDestroyOnLoad(dragBoxGO);

            line = dragBoxGO.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = 4;
            line.startWidth = 0.04f;
            line.endWidth = 0.04f;
            line.sortingOrder = 999;

            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.white;
            line.endColor = Color.white;
        }

        private void ShowDragBox()
        {
            if (dragBoxGO != null)
                dragBoxGO.SetActive(true);
        }

        private void HideDragBox()
        {
            if (dragBoxGO != null)
                dragBoxGO.SetActive(false);
        }

        private void UpdateDragBox(int a, int b)
        {
            if (line == null) return;

            int minX, maxX, minY, maxY;
            GetRectBounds(a, b, out minX, out maxX, out minY, out maxY);

            float half = Grid.CellSizeInMeters * 0.5f;
            float z = -0.1f;

            Vector3 bl = Grid.CellToPos(Grid.XYToCell(minX, minY)) + new Vector3(-half, -half, z);
            Vector3 br = Grid.CellToPos(Grid.XYToCell(maxX, minY)) + new Vector3(half, -half, z);
            Vector3 tr = Grid.CellToPos(Grid.XYToCell(maxX, maxY)) + new Vector3(half, half, z);
            Vector3 tl = Grid.CellToPos(Grid.XYToCell(minX, maxY)) + new Vector3(-half, half, z);

            line.SetPosition(0, bl);
            line.SetPosition(1, br);
            line.SetPosition(2, tr);
            line.SetPosition(3, tl);
        }

        private static void GetRectBounds(int a, int b, out int minX, out int maxX, out int minY, out int maxY)
        {
            Vector2I av = Grid.CellToXY(a);
            Vector2I bv = Grid.CellToXY(b);

            minX = Math.Min(av.x, bv.x);
            maxX = Math.Max(av.x, bv.x);
            minY = Math.Min(av.y, bv.y);
            maxY = Math.Max(av.y, bv.y);

            minX = Mathf.Clamp(minX, 0, Grid.WidthInCells - 1);
            maxX = Mathf.Clamp(maxX, 0, Grid.WidthInCells - 1);
            minY = Mathf.Clamp(minY, 0, Grid.HeightInCells - 1);
            maxY = Mathf.Clamp(maxY, 0, Grid.HeightInCells - 1);
        }

        private static GameObject GetBuildingUnderCursor(Vector3 cursor_pos)
        {
            int cell = Grid.PosToCell(cursor_pos);
            if (!Grid.IsValidCell(cell))
                return null;
            return Grid.Objects[cell, (int)ObjectLayer.Building];
        }

        private static void Log(string msg)
        {
            if (DEBUG_LOGS)
                Debug.Log("[CopyMaterialsTool] " + msg);
        }
    }
}
