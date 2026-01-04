using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Options;
using UnityEngine;

namespace EmptyStorage
{
	// Add EmptyStorageSetting component to storages
	// We use our own EmptyStorageWorkable which doesn't register with the priority system on init
	[HarmonyPatch(typeof(Storage), "OnPrefabInit")]
	public static class Storage_OnPrefabInit_Patch
	{
		internal static void Postfix(Storage __instance)
		{
			// Skip dupe/minion internal storages - they have MinionIdentity component
			if (__instance.gameObject.GetComponent<MinionIdentity>() != null)
				return;
			
			// Skip objects that already have special handling
			if (__instance.gameObject.GetComponent<CargoBay>() == null && 
			    __instance.gameObject.GetComponent<CargoBayCluster>() == null && 
			    __instance.gameObject.GetComponent<Dumpable>() == null && 
			    __instance.gameObject.GetComponent<DropAllWorkable>() == null && 
			    __instance.gameObject.GetComponent<EmptyStorageWorkable>() == null &&
			    __instance.gameObject.GetComponent<HiveHarvestMonitor.Instance>() == null)
			{
				// Only add EmptyStorageSetting - it will handle adding the workable on-demand
				__instance.gameObject.AddOrGet<EmptyStorageSetting>();
			}
		}
	}

	// Patch MinionResume.HasPerk to allow bionic dupes with Tidying Booster for solid storage
	[HarmonyPatch(typeof(MinionResume), "HasPerk", new System.Type[] { typeof(HashedString) })]
	public static class MinionResume_HasPerk_Patch
	{
		internal static void Postfix(MinionResume __instance, HashedString perkId, ref bool __result)
		{
			try
			{
				// If they already have the perk, no need to check further
				if (__result)
					return;
				
				// Only check for Groundskeeper perk (Tidy skill for solid storage)
				var tidySkillId = Db.Get().SkillPerks.IncreaseStrengthGroundskeeper.Id;
				
				if (perkId != tidySkillId)
					return;
				
				// Check if skills are required for our mod
				var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
				if (options.ImmediateEmptying || !options.RequireSkills)
					return;
				
				// Check if worker is a bionic dupe
				var workerGameObject = __instance.gameObject;
				if (workerGameObject == null)
					return;
				
				var minionModifiers = workerGameObject.GetComponent<MinionModifiers>();
				if (minionModifiers == null)
					return;
				
				// Check if bionic dupe has Tidying Booster installed
				var bionicUpgradesMonitor = workerGameObject.GetSMI<BionicUpgradesMonitor.Instance>();
				if (bionicUpgradesMonitor == null)
					return;
				
				Tag tidyingBoosterTag = new Tag("Booster_Tidy1");
				int boosterCount = bionicUpgradesMonitor.CountBoosterAssignments(tidyingBoosterTag);
				if (boosterCount > 0)
				{
					__result = true;
				}
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogError($"[EmptyStorage] MinionResume.HasPerk Postfix error: {ex.Message}\n{ex.StackTrace}");
			}
		}
	}
}
