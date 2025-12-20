using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace EmptyStorage
{
	public class EmptyStoragePatches : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);

			// Initialize PLib
			PUtil.InitLibrary();

			// Register options with shared config location
			// The [ConfigFile] attribute should use shared config location automatically
			var options = new POptions();
			options.RegisterOptions(this, typeof(EmptyStorageOptions));

			// Apply Harmony patches
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(Storage), "OnPrefabInit")]
		public static class Storage_OnPrefabInit_Patch
		{
			internal static void Postfix(Storage __instance)
			{
				if (__instance.gameObject.GetComponent<CargoBay>() == null && __instance.gameObject.GetComponent<CargoBayCluster>() == null && __instance.gameObject.GetComponent<Dumpable>() == null && __instance.gameObject.GetComponent<DropAllWorkable>() == null && __instance.gameObject.GetComponent<HiveHarvestMonitor.Instance>() == null)
				{
					// Always add DropAllWorkable at prefab init so it's properly initialized
					Prioritizable.AddRef(__instance.gameObject);
					DropAllWorkable workable = __instance.gameObject.AddOrGet<DropAllWorkable>();
					
					if (workable != null)
					{
						// Initialize removeTags to prevent null reference errors (like EmptyTheStorageErrand mod)
						workable.removeTags = new List<Tag>();
						
						// Determine storage type and set appropriate skill requirement
						// Gas and Liquid storage -> Plumbing skill
						// Solid storage -> Tidy skill
						string requiredSkillPerk = null;
						
						bool isGasStorage = false;
						bool isLiquidStorage = false;
						
						// Check for ConduitConsumer/ConduitDispenser components and their conduitType
						var conduitConsumer = __instance.gameObject.GetComponent<ConduitConsumer>();
						if (conduitConsumer != null)
						{
							// ConduitConsumer has a public conduitType field
							if (conduitConsumer.conduitType == ConduitType.Gas)
								isGasStorage = true;
							else if (conduitConsumer.conduitType == ConduitType.Liquid)
								isLiquidStorage = true;
						}
						
						var conduitDispenser = __instance.gameObject.GetComponent<ConduitDispenser>();
						if (conduitDispenser != null && !isGasStorage && !isLiquidStorage)
						{
							// ConduitDispenser has a public conduitType field
							if (conduitDispenser.conduitType == ConduitType.Gas)
								isGasStorage = true;
							else if (conduitDispenser.conduitType == ConduitType.Liquid)
								isLiquidStorage = true;
						}
						
						// Fallback: Check BuildingDef's InputConduitType or OutputConduitType
						if (!isGasStorage && !isLiquidStorage)
						{
							var building = __instance.gameObject.GetComponent<Building>();
							if (building != null && building.Def != null)
							{
								if (building.Def.InputConduitType == ConduitType.Gas || building.Def.OutputConduitType == ConduitType.Gas)
									isGasStorage = true;
								else if (building.Def.InputConduitType == ConduitType.Liquid || building.Def.OutputConduitType == ConduitType.Liquid)
									isLiquidStorage = true;
							}
						}
						
						// Check if skills are required (only if not immediate emptying)
						var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
						if (!options.ImmediateEmptying && options.RequireSkills)
						{
							if (isGasStorage || isLiquidStorage)
							{
								// Gas or Liquid storage -> Plumbing skill
								var plumbingSkillId = Db.Get().SkillPerks.CanDoPlumbing.Id;
								requiredSkillPerk = plumbingSkillId.ToString();
							}
							else
							{
								// Solid storage -> Tidy skill
								var tidySkillId = Db.Get().SkillPerks.IncreaseStrengthGroundskeeper.Id;
								requiredSkillPerk = tidySkillId.ToString();
							}
						}
						
						workable.requiredSkillPerk = requiredSkillPerk;
						
						// Set animation properties (like EmptyTheStorageErrand mod)
						workable.faceTargetWhenWorking = true; // Makes dupe face the storage while working
						
						BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
						
						// Set multitool context for animation (like EmptyTheStorageErrand mod)
						var multitoolContextField = typeof(DropAllWorkable).GetField("multitoolContext", bindingAttr);
						if (multitoolContextField != null)
						{
							// Check the field type and set appropriately
							var fieldType = multitoolContextField.FieldType;
							var fieldTypeName = fieldType.FullName;
							
							// HashedString is in KAnim namespace, but we can use reflection to create it
							if (fieldTypeName == "KAnim.HashedString" || fieldTypeName == "HashedString")
							{
								// Create HashedString using reflection
								var hashedStringType = fieldType;
								var hashedStringCtor = hashedStringType.GetConstructor(new Type[] { typeof(string) });
								if (hashedStringCtor != null)
								{
									var hashedString = hashedStringCtor.Invoke(new object[] { "build" });
									multitoolContextField.SetValue(workable, hashedString);
								}
							}
							else if (fieldType == typeof(string))
							{
								multitoolContextField.SetValue(workable, "build");
							}
							else
							{
								// Try implicit conversion
								multitoolContextField.SetValue(workable, "build");
							}
						}
						
						// Set multitool hit effect tag for animation (like EmptyTheStorageErrand mod)
						var multitoolHitEffectTagField = typeof(DropAllWorkable).GetField("multitoolHitEffectTag", bindingAttr);
						if (multitoolHitEffectTagField != null)
						{
							// EffectConfigs.BuildSplashId is a string, need to convert to Tag
							var fieldType = multitoolHitEffectTagField.FieldType;
							if (fieldType == typeof(Tag))
							{
								// Convert string to Tag
								Tag buildSplashTag = new Tag(EffectConfigs.BuildSplashId);
								multitoolHitEffectTagField.SetValue(workable, buildSplashTag);
							}
							else
							{
								// If it's not a Tag type, try to convert
								UnityEngine.Debug.LogWarning($"[EmptyStorage] multitoolHitEffectTag field type is {fieldType}, expected Tag");
							}
						}
						
						// Disable showCmd to prevent button from showing (like EmptyTheStorageErrand mod)
						// This is set at prefab init, but we also need to ensure it stays false
						var showCmdField = typeof(DropAllWorkable).GetField("showCmd", bindingAttr);
						if (showCmdField != null)
						{
							showCmdField.SetValue(workable, false);
						}
						
						// IMPORTANT: Disable skill perk status item to prevent errors on all prefabs
						// This prevents the game from checking skills on prefabs that don't have active tasks
						// This is the key fix from EmptyTheStorageErrand mod
						var shouldShowSkillPerkStatusItemField = typeof(DropAllWorkable).GetField("shouldShowSkillPerkStatusItem", bindingAttr);
						if (shouldShowSkillPerkStatusItemField != null)
						{
							shouldShowSkillPerkStatusItemField.SetValue(workable, false);
						}
						
						// Initially disable it - UpdateWorkableState will set it correctly
						workable.enabled = false;
					}
					
					// Always add our custom setting component for the button
					__instance.gameObject.AddOrGet<EmptyStorageSetting>();
				}
			}
		}

		// Patch DropAllWorkable.GetNewShowCmd to always return false when our component exists
		// This prevents the button from showing on both the building and dupe sidescreen
		[HarmonyPatch(typeof(DropAllWorkable), "GetNewShowCmd")]
		public static class DropAllWorkable_GetNewShowCmd_Patch
		{
			internal static bool Prefix(DropAllWorkable __instance, ref bool __result)
			{
				// If our component exists, always return false to prevent button from showing
				if (__instance.gameObject.GetComponent<EmptyStorageSetting>() != null)
				{
					__result = false;
					// Also ensure showCmd field is set to false
					BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
					var showCmdField = typeof(DropAllWorkable).GetField("showCmd", bindingAttr);
					if (showCmdField != null)
					{
						showCmdField.SetValue(__instance, false);
					}
					return false; // Skip original method
				}
				return true; // Run original method
			}
		}

		// Patch OnStorageChange to keep showCmd false when our component exists
		[HarmonyPatch(typeof(DropAllWorkable), "OnStorageChange")]
		public static class DropAllWorkable_OnStorageChange_Patch
		{
			internal static void Postfix(DropAllWorkable __instance, object data)
			{
				// If our component exists, ensure showCmd stays false
				if (__instance.gameObject.GetComponent<EmptyStorageSetting>() != null)
				{
					BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
					var showCmdField = typeof(DropAllWorkable).GetField("showCmd", bindingAttr);
					if (showCmdField != null)
					{
						showCmdField.SetValue(__instance, false);
					}
				}
			}
		}

		// Also patch OnRefreshUserMenu to prevent button from showing on dupe sidescreen
		[HarmonyPatch(typeof(DropAllWorkable), "OnRefreshUserMenu")]
		public static class DropAllWorkable_OnRefreshUserMenu_Patch
		{
			internal static bool Prefix(DropAllWorkable __instance, object data)
			{
				// If our component exists, ALWAYS block DropAllWorkable from adding its button
				// We handle the button ourselves in EmptyStorageSetting
				// This prevents it from showing on both building and dupe sidescreen
				if (__instance.gameObject.GetComponent<EmptyStorageSetting>() != null)
				{
					// ALWAYS ensure showCmd is false - this is the key to preventing the button
					BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
					var showCmdField = typeof(DropAllWorkable).GetField("showCmd", bindingAttr);
					if (showCmdField != null)
					{
						showCmdField.SetValue(__instance, false);
					}
					
					// Always block - we handle the button ourselves
					return false; // Skip original method - don't add button
				}
				return true; // Run original method
			}
		}

		// Patch DropAll to calculate work time based on storage mass and enable shouldShowSkillPerkStatusItem
		[HarmonyPatch(typeof(DropAllWorkable), "DropAll")]
		public static class DropAllWorkable_DropAll_Patch
		{
			internal static bool Prefix(DropAllWorkable __instance)
			{
				// Only handle this for storages with our component
				// Use a quick check to avoid hanging
				var emptyStorageSetting = __instance.gameObject.GetComponent<EmptyStorageSetting>();
				if (emptyStorageSetting == null)
					return true; // Let original method run
				
				try
				{
					// Check options for work time
					var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
					
					// Only calculate work time if UseWorkTime is enabled
					if (options.UseWorkTime && !options.ImmediateEmptying)
					{
						// Calculate work time based on storage mass and option setting
						// Use GetStorages() method to ensure storages are initialized
						BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
						var getStoragesMethod = typeof(DropAllWorkable).GetMethod("GetStorages", bindingAttr);
						Storage[] storages = null;
						
						if (getStoragesMethod != null)
						{
							storages = getStoragesMethod.Invoke(__instance, null) as Storage[];
						}
						else
						{
							// Fallback to direct component access
							storages = __instance.GetComponents<Storage>();
						}
						
						if (storages != null && storages.Length > 0 && storages[0] != null)
						{
							float massStored = storages[0].MassStored();
							// Use the slider value: workTimePer100kg seconds per 100kg
							float workTime = (massStored / 100f) * options.WorkTimePer100kg;
							if (workTime < 0.1f) workTime = 0.1f; // Minimum work time
							__instance.dropWorkTime = workTime;
							__instance.SetWorkTime(workTime);
						}
					}
					else
					{
						// Instant work time
						__instance.dropWorkTime = 0.1f; // Minimum work time
						__instance.SetWorkTime(0.1f);
					}
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError($"[EmptyStorage] DropAll Prefix error: {ex.Message}\n{ex.StackTrace}");
				}
				
				return true; // Let original method run (same as EmptyTheStorageErrand)
			}
			
			internal static void Postfix(DropAllWorkable __instance)
			{
				try
				{
					// Only handle this for storages with our component
					var emptyStorageSetting = __instance.gameObject.GetComponent<EmptyStorageSetting>();
					if (emptyStorageSetting == null)
						return;
					
					// Check if a chore was created (not cancelled)
					var choreProperty = typeof(DropAllWorkable).GetProperty("Chore", BindingFlags.NonPublic | BindingFlags.Instance);
					var chore = choreProperty?.GetValue(__instance);
					
					if (chore != null)
					{
						// Chore exists - set work time again in case it wasn't set correctly in Prefix
						// This ensures the work time is correct even if storages weren't initialized in Prefix
						var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
						
						if (options.UseWorkTime && !options.ImmediateEmptying)
						{
							BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
							var getStoragesMethod = typeof(DropAllWorkable).GetMethod("GetStorages", bindingAttr);
							Storage[] storages = null;
							
							if (getStoragesMethod != null)
							{
								storages = getStoragesMethod.Invoke(__instance, null) as Storage[];
							}
							else
							{
								// Fallback to direct component access
								storages = __instance.GetComponents<Storage>();
							}
							
							if (storages != null && storages.Length > 0 && storages[0] != null)
							{
								float massStored = storages[0].MassStored();
								// Use the slider value: workTimePer100kg seconds per 100kg
								float workTime = (massStored / 100f) * options.WorkTimePer100kg;
								if (workTime < 0.1f) workTime = 0.1f; // Minimum work time
								if (workTime > 0f)
								{
									__instance.dropWorkTime = workTime;
									__instance.SetWorkTime(workTime);
								}
							}
						}
						else
						{
							// Instant work time
							__instance.dropWorkTime = 0.1f;
							__instance.SetWorkTime(0.1f);
						}
						
						// Enable skill perk status item so it shows if no qualified worker (only if skills are required)
						if (options.RequireSkills && !options.ImmediateEmptying)
						{
							// Use SetShouldShowSkillPerkStatusItem which properly subscribes to skill updates and calls UpdateStatusItem
							__instance.SetShouldShowSkillPerkStatusItem(true);
						}
						else
						{
							__instance.SetShouldShowSkillPerkStatusItem(false);
						}
						
						// Ensure workable is enabled (required for status item to show)
						__instance.enabled = true;
					}
					else
					{
						// Chore was cancelled - disable skill perk status item
						// Use SetShouldShowSkillPerkStatusItem which properly unsubscribes and calls UpdateStatusItem
						__instance.SetShouldShowSkillPerkStatusItem(false);
					}
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError($"[EmptyStorage] DropAll Postfix error: {ex.Message}\n{ex.StackTrace}");
				}
			}
		}


		// Patch MinionResume.HasPerk to allow bionic dupes with Tidying Booster for solid storage
		// This allows bionic dupes with Tidying Booster to do solid storage emptying work
		// We patch HasPerk to return true for Groundskeeper perk when the dupe is bionic
		// Note: This assumes bionic dupes with Tidying Booster should have access to the perk
		// We need to specify the exact method signature to avoid ambiguous match
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
					
					// Compare HashedString values
					if (perkId != tidySkillId)
						return;
					
					// Check if skills are required for our mod
					var options = POptions.ReadSettings<EmptyStorageOptions>() ?? new EmptyStorageOptions();
					if (options.ImmediateEmptying || !options.RequireSkills)
						return;
					
					// Check if worker is a bionic dupe - bionic dupes have MinionModifiers component
					var workerGameObject = __instance.gameObject;
					if (workerGameObject == null)
						return;
					
					var minionModifiers = workerGameObject.GetComponent<MinionModifiers>();
					if (minionModifiers == null)
						return;
					
					// Check if bionic dupe has Tidying Booster installed
					// The Tidying Booster is "Booster_Tidy1" and grants CanDoPlumbing and CanMakeMissiles
					// It doesn't grant IncreaseStrengthGroundskeeper, but we allow it for solid storage emptying
					var bionicUpgradesMonitor = workerGameObject.GetSMI<BionicUpgradesMonitor.Instance>();
					if (bionicUpgradesMonitor == null)
						return;
					
					// Check if they have the Tidying Booster assigned
					Tag tidyingBoosterTag = new Tag("Booster_Tidy1");
					int boosterCount = bionicUpgradesMonitor.CountBoosterAssignments(tidyingBoosterTag);
					if (boosterCount > 0)
					{
						// Bionic dupe has Tidying Booster - allow them to have the Groundskeeper perk
						// This allows them to do solid storage emptying
						__result = true;
					}
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError($"[EmptyStorage] MinionResume.HasPerk Postfix error: {ex.Message}\n{ex.StackTrace}");
				}
			}
		}

		// Patch DropAllWorkable.OnCompleteWork to handle null references
		[HarmonyPatch(typeof(DropAllWorkable), "OnCompleteWork")]
		public static class DropAllWorkable_OnCompleteWork_Patch
		{
			internal static bool Prefix(DropAllWorkable __instance, WorkerBase worker)
			{
				try
				{
					// Use reflection to call GetStorages() method (same as original)
					var getStoragesMethod = typeof(DropAllWorkable).GetMethod("GetStorages", BindingFlags.NonPublic | BindingFlags.Instance);
					Storage[] array = null;
					if (getStoragesMethod != null)
					{
						array = getStoragesMethod.Invoke(__instance, null) as Storage[];
					}
					else
					{
						array = __instance.GetComponents<Storage>();
					}
					
					// Safe null check
					if (array == null || array.Length == 0)
					{
						CleanupChore(__instance);
						return false; // Skip original method
					}
					
					// Get removeTags and resetTargetWorkableOnCompleteWork for use below
					// removeTags should already be initialized at prefab init, but check just in case
					var removeTags = __instance.removeTags ?? new List<Tag>();
					var resetTargetField = typeof(DropAllWorkable).GetField("resetTargetWorkableOnCompleteWork", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
					bool resetTarget = resetTargetField != null && (bool)(resetTargetField.GetValue(__instance) ?? false);
					
					// Process storages safely (replicate original logic with null checks)
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == null)
							continue;
							
						// Get items collection - it's a List<GameObject> or similar collection
						var itemsCollection = array[i].items;
						if (itemsCollection == null)
							continue;
							
						// Create a copy of the items list to avoid modification during iteration
						var itemsList = new List<GameObject>();
						foreach (GameObject item in itemsCollection)
						{
							if (item != null)
								itemsList.Add(item);
						}
						for (int j = 0; j < itemsList.Count; j++)
						{
							if (itemsList[j] == null)
								continue;
								
							GameObject gameObject = array[i].Drop(itemsList[j], true);
							if (gameObject != null)
							{
								// Remove tags safely
								if (removeTags != null)
								{
									foreach (Tag tag in removeTags)
									{
										if (tag != null)
											gameObject.RemoveTag(tag);
									}
								}
								
								gameObject.Trigger(580035959, worker);
								
								if (resetTarget)
								{
									Pickupable component = gameObject.GetComponent<Pickupable>();
									if (component != null)
									{
										component.targetWorkable = component;
										component.SetOffsetTable(OffsetGroups.InvertedStandardTable);
									}
								}
							}
						}
					}
					
					// Cleanup (same as original)
					var choreProperty = typeof(DropAllWorkable).GetProperty("Chore", BindingFlags.NonPublic | BindingFlags.Instance);
					choreProperty?.SetValue(__instance, null);
					
					var refreshMethod = typeof(DropAllWorkable).GetMethod("RefreshStatusItem", BindingFlags.NonPublic | BindingFlags.Instance);
					refreshMethod?.Invoke(__instance, null);
					
					__instance.Trigger(-1957399615, null);
					
					return false; // Skip original method, we've done the work
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError($"[EmptyStorage] OnCompleteWork: Exception: {ex.Message}\n{ex.StackTrace}");
					CleanupChore(__instance);
					return false; // Skip original method on error
				}
			}
			
			private static void CleanupChore(DropAllWorkable instance)
			{
				try
				{
					var choreProperty = typeof(DropAllWorkable).GetProperty("Chore", BindingFlags.NonPublic | BindingFlags.Instance);
					choreProperty?.SetValue(instance, null);
					
					instance.GetComponent<KSelectable>()?.RemoveStatusItem(Db.Get().DuplicantStatusItems.Emptying, false);
					
					// Disable skill perk status item since task is completed
					// Use SetShouldShowSkillPerkStatusItem which properly unsubscribes and calls UpdateStatusItem
					instance.SetShouldShowSkillPerkStatusItem(false);
					
					var refreshMethod = typeof(DropAllWorkable).GetMethod("RefreshStatusItem", BindingFlags.NonPublic | BindingFlags.Instance);
					refreshMethod?.Invoke(instance, null);
				}
				catch { }
			}
		}



		// Patch DropAllWorkable.OnSpawn to ensure showCmd stays false
		[HarmonyPatch(typeof(DropAllWorkable), "OnSpawn")]
		public static class DropAllWorkable_OnSpawn_Patch
		{
			internal static void Postfix(DropAllWorkable __instance)
			{
				// If our component exists, ensure showCmd stays false
				// This runs after GetNewShowCmd() is called in OnSpawn
				if (__instance.gameObject.GetComponent<EmptyStorageSetting>() != null)
				{
					BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
					var showCmdField = typeof(DropAllWorkable).GetField("showCmd", bindingAttr);
					if (showCmdField != null)
					{
						showCmdField.SetValue(__instance, false);
					}
				}
			}
		}

		// Patch UserMenu.AddButton to filter out DropAllWorkable buttons when dupe is selected
		[HarmonyPatch(typeof(UserMenu), "AddButton")]
		public static class UserMenu_AddButton_Patch
		{
			internal static bool Prefix(UserMenu __instance, GameObject go, KIconButtonMenu.ButtonInfo button, float sort_order)
			{
				// Check if the button is from DropAllWorkable and if a dupe is selected
				if (button != null && button.iconName == "action_empty_contents")
				{
					// Check if the target GameObject has our component
					if (go != null && go.GetComponent<EmptyStorageSetting>() != null)
					{
						// Check if a dupe is currently selected
						var selectedObject = SelectTool.Instance?.selected;
						if (selectedObject != null)
						{
							if (selectedObject.GetComponent<MinionIdentity>() != null || selectedObject.GetComponent<MinionResume>() != null)
							{
								// Check if this button is from DropAllWorkable (not our custom button)
								var dropAllWorkable = go.GetComponent<DropAllWorkable>();
								if (dropAllWorkable != null)
								{
									// This is DropAllWorkable's button - block it when dupe is selected
									return false; // Skip adding the button
								}
							}
						}
					}
				}
				return true; // Allow the button to be added
			}
		}

	}
}

