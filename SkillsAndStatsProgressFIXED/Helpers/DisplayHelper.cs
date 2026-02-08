using UnityEngine;

namespace SkillsAndStatsProgressFIXED
{
    public static class DisplayHelper
    {
        public static string ShrinkTo(string S, int MaxLength)
        {
            if (S.Length <= MaxLength)
                return S;
            return S.Substring(0, MaxLength);
        }

        public static bool CanShowInfo(GameObject G)
        {
            if (!Config.Cfg.ShowWorkableInfo)
                return false;
            if (Config.Cfg.ShowWorkableOnlyForSelectedDuplicant)
                return IsSelected(G);
            return true;
        }

        public static bool IsSelected(GameObject G)
        {
            if (DetailsScreen.Instance == null || KMonoBehaviour.isLoadingScene || !DetailsScreen.Instance.IsActive())
                return false;
            return DetailsScreen.Instance.target == G;
        }

        public static void ShowText(string Txt, GameObject G, Color C, float Speed, float Time)
        {
            if (!CanShowInfo(G))
                return;

            PopFX popFX = PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, Txt, G.transform, Time, false);
            RectTransform component = popFX.GetComponent<RectTransform>();
            component.SetSizeWithCurrentAnchors(0, 250f);
            if (popFX != null)
            {
                popFX.TextDisplay.color = C;
                // PopFX.Speed is const in current game — cannot be set at runtime
                popFX.offset = new Vector3(1f, 3.5f);
                popFX.TextDisplay.fontSize = Config.Cfg.WorkableReportFontSize;
                var iconColor = popFX.IconDisplay.color;
                iconColor.a = 0f;
                popFX.IconDisplay.color = iconColor;
            }
        }
    }
}
