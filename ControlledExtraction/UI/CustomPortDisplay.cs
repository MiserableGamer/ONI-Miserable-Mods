using UnityEngine;

namespace ControlledExtraction.UI
{
    // Displays a single custom port icon
    [SkipSaveFileSerialization]
    public class CustomPortDisplay : KMonoBehaviour
    {
        private GameObject portObject;
        private int lastUtilityCell = -1;

        public ConduitType conduitType;
        public CellOffset offset;
        public bool isInput;
        public SimHashes element;
        public Color color;

        public void Setup(PortDisplayInfo info)
        {
            conduitType = info.ConduitType;
            offset = info.Offset;
            isInput = info.IsInput;
            element = info.Element;
            color = info.Color;
        }

        public void Draw(BuildingCellVisualizer visualizer, bool force)
        {
            var building = visualizer.GetComponent<Building>();
            if (building == null) return;

            int utilityCell = Grid.OffsetCell(building.GetCell(), offset);

            if (force || utilityCell != lastUtilityCell)
            {
                lastUtilityCell = utilityCell;
                
                // Get the appropriate sprite based on input/output and conduit type
                Sprite sprite = GetSprite();

                if (sprite != null)
                {
                    // Create or update the port icon
                    if (portObject == null)
                    {
                        portObject = new GameObject("CustomPortIcon");
                        portObject.transform.SetParent(visualizer.transform);
                        
                        var renderer = portObject.AddComponent<SpriteRenderer>();
                        renderer.sprite = sprite;
                        renderer.color = color;
                        renderer.sortingLayerName = "FX";
                        renderer.sortingOrder = 100;
                    }

                    // Position the icon
                    Vector3 pos = Grid.CellToPosCCC(utilityCell, Grid.SceneLayer.Building);
                    portObject.transform.position = pos;
                    portObject.SetActive(true);
                }
            }
        }

        private Sprite GetSprite()
        {
            var resources = BuildingCellVisualizerResources.Instance();
            if (resources == null) return null;

            if (isInput)
            {
                if (conduitType == ConduitType.Gas)
                    return resources.gasInputIcon;
                else if (conduitType == ConduitType.Liquid)
                    return resources.liquidInputIcon;
                else
                    return resources.liquidInputIcon; // Fallback for solid
            }
            else
            {
                if (conduitType == ConduitType.Gas)
                    return resources.gasOutputIcon;
                else if (conduitType == ConduitType.Liquid)
                    return resources.liquidOutputIcon;
                else
                    return resources.liquidOutputIcon; // Fallback for solid
            }
        }

        public void DisableIcon()
        {
            if (portObject != null && portObject.activeInHierarchy)
            {
                portObject.SetActive(false);
            }
        }

        protected override void OnCleanUp()
        {
            base.OnCleanUp();
            if (portObject != null)
            {
                Destroy(portObject);
            }
        }

        public bool MatchesOverlay(HashedString mode)
        {
            if (mode == OverlayModes.GasConduits.ID && conduitType == ConduitType.Gas)
                return true;
            if (mode == OverlayModes.LiquidConduits.ID && conduitType == ConduitType.Liquid)
                return true;
            if (mode == OverlayModes.SolidConveyor.ID && conduitType == ConduitType.Solid)
                return true;
            return false;
        }
    }
}
