using System.Collections.Generic;
using UnityEngine;

namespace CopyMaterialsTool
{
    public sealed class CopyMaterialsTool : DragTool
    {
        private readonly HashSet<int> visited = new HashSet<int>();

        protected override void OnActivateTool()
        {
            base.OnActivateTool();
            visited.Clear();
        }

        protected override void OnDeactivateTool(InterfaceTool newTool)
        {
            base.OnDeactivateTool(newTool);
            visited.Clear();
        }

        public override void OnLeftClickDown(Vector3 cursor_pos)
        {
            GameObject go = GetBuildingUnderCursor();
            if (go == null) return;

            int[] ids = BuildingMaterialUtil.TryGetConstructionElementIds(go);
            if (ids != null && ids.Length > 0)
            {
                MaterialClipboard.Set(ids);
                PopFXManager.Instance.SpawnFX(
                    PopFXManager.Instance.sprite_Plus,
                    "Materials copied",
                    go.transform,
                    Vector3.zero
                );
            }
            else
            {
                PopFXManager.Instance.SpawnFX(
                    PopFXManager.Instance.sprite_Negative,
                    "No materials found",
                    go.transform,
                    Vector3.zero
                );
            }
        }

        protected override void OnDragTool(int cell, int distFromOrigin)
        {
            if (!MaterialClipboard.HasData) return;
            if (!Grid.IsValidCell(cell)) return;

            GameObject go = Grid.Objects[cell, (int)ObjectLayer.Building];
            if (go == null) return;

            int id = go.GetInstanceID();
            if (!visited.Add(id)) return;

            bool ok = RebuildApply.TryApplyAndRebuild(go, MaterialClipboard.CopiedElementIds);
            if (ok)
            {
                PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, "Applied", go.transform, Vector3.zero);
            }
            else
            {
                PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, "Can't apply", go.transform, Vector3.zero);
            }
        }

        private GameObject GetBuildingUnderCursor()
        {
            Vector3 hovered = PlayerController.GetCursorPos(KInputManager.GetMousePos());
            int cell = Grid.PosToCell(hovered);
            if (!Grid.IsValidCell(cell)) return null;

            return Grid.Objects[cell, (int)ObjectLayer.Building];
        }
    }
}
