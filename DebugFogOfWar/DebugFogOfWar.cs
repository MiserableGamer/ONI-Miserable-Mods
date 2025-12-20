using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace DebugFogOfWar
{
	public class DebugFogOfWar : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			
			// Initialize PLib
			PUtil.InitLibrary();
			
			// Apply Harmony patches
			harmony.PatchAll();
		}
	}
}

