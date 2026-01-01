using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace ControlledOverlay
{
	public sealed class ControlledOverlayMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			PUtil.InitLibrary();

			harmony.PatchAll();
		}
	}
}

