using STRINGS;

namespace CopyTileTool
{
    public static class CopyTileStrings
    {
        public static class UI
        {
            public static class COPY_TILE
            {
                public static LocString BUTTON_TEXT = "Copy Tile";
                public static LocString BUTTON_TOOLTIP = "Select this tile as the destination type, then click another tile to select what to replace, then drag to replace matching tiles.";
                
                public static LocString SELECT_SOURCE = "Now click a tile to replace";
                public static LocString READY_TO_DRAG = "Drag to replace matching tiles";
                public static LocString TILE_COPIED = "Tile copied";
                public static LocString SOURCE_SELECTED = "Source selected";
                public static LocString REPLACEMENT_QUEUED = "Replacement queued";
                public static LocString NOT_A_TILE = "Not a tile";
                public static LocString USE_COPY_MATERIALS = "Same type - use Copy Materials";
            }
        }

        public static void Register()
        {
            Strings.Add("STRINGS.UI.COPY_TILE.BUTTON_TEXT", UI.COPY_TILE.BUTTON_TEXT);
            Strings.Add("STRINGS.UI.COPY_TILE.BUTTON_TOOLTIP", UI.COPY_TILE.BUTTON_TOOLTIP);
            Strings.Add("STRINGS.UI.COPY_TILE.SELECT_SOURCE", UI.COPY_TILE.SELECT_SOURCE);
            Strings.Add("STRINGS.UI.COPY_TILE.READY_TO_DRAG", UI.COPY_TILE.READY_TO_DRAG);
            Strings.Add("STRINGS.UI.COPY_TILE.TILE_COPIED", UI.COPY_TILE.TILE_COPIED);
            Strings.Add("STRINGS.UI.COPY_TILE.SOURCE_SELECTED", UI.COPY_TILE.SOURCE_SELECTED);
            Strings.Add("STRINGS.UI.COPY_TILE.REPLACEMENT_QUEUED", UI.COPY_TILE.REPLACEMENT_QUEUED);
            Strings.Add("STRINGS.UI.COPY_TILE.NOT_A_TILE", UI.COPY_TILE.NOT_A_TILE);
            Strings.Add("STRINGS.UI.COPY_TILE.USE_COPY_MATERIALS", UI.COPY_TILE.USE_COPY_MATERIALS);
        }
    }
}

