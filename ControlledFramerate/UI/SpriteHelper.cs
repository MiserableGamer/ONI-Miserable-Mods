using System;
using System.Collections.Generic;
using PeterHan.PLib.UI;
using UnityEngine;

namespace ControlledFramerate.UI
{
    internal static class SpriteHelper
    {
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        private const string ResourcePrefix = "ControlledFramerate.Assets.";

        internal static Sprite Load(string name, string fallbackGameSprite = "icon_thermal_conductivity")
        {
            if (cache.TryGetValue(name, out var cached))
                return cached;

            Sprite sprite = null;
            try
            {
                sprite = PUIUtils.LoadSprite(ResourcePrefix + name + ".png", log: false);
            }
            catch { }

            if (sprite == null)
            {
                sprite = Assets.GetSprite(fallbackGameSprite);
                ControlledFramerateMod.Log($"Using fallback sprite for '{name}' (place {name}.png in Assets/)");
            }

            cache[name] = sprite;
            return sprite;
        }
    }
}
