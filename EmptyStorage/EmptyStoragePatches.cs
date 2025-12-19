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
						
						if (isGasStorage || isLiquidStorage)
						{
							// Gas or Liquid storage -> Plumbing skill
							var plumbingSkillId = Db.Get().SkillPerks.CanDoPlumbing.Id;
							requiredSkillPerk = plumbingSkillId.ToString();
							UnityEngine.Debug.Log($"[EmptyStorage] Storage type: {(isGasStorage ? "Gas" : "Liquid")}, skill: Plumbing");
						}
						else
						{
							// Solid storage -> Tidy skill
							var tidySkillId = Db.Get().SkillPerks.IncreaseStrengthGroundskeeper.Id;
							requiredSkillPerk = tidySkillId.ToString();
							UnityEngine.Debug.Log($"[EmptyStorage] Storage type: Solid, skill: Tidy");
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
		// Also check if chore was cancelled and clean up shouldShowSkillPerkStatusItem
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
					
					// Check if chore was cancelled (chore is null but shouldShowSkillPerkStatusItem is still true)
					// This handles the case where the cancel tool cancels the chore
					var choreProperty = typeof(DropAllWorkable).GetProperty("Chore", BindingFlags.NonPublic | BindingFlags.Instance);
					var chore = choreProperty?.GetValue(__instance);
					if (chore == null)
					{
						var shouldShowField = typeof(DropAllWorkable).GetField("shouldShowSkillPerkStatusItem", bindingAttr);
						if (shouldShowField != null)
						{
							var shouldShow = (bool)(shouldShowField.GetValue(__instance) ?? false);
							if (shouldShow)
							{
								// Chore is null but shouldShow is still true - disable it
								__instance.SetShouldShowSkillPerkStatusItem(false);
								UnityEngine.Debug.Log($"[EmptyStorage] OnStorageChange - Chore is null, disabled shouldShowSkillPerkStatusItem");
							}
						}
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
					// Calculate work time based on storage mass (like EmptyTheStorageErrand mod)
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
						float workTime = massStored / 100f; // Same calculation as EmptyTheStorageErrand
						if (workTime < 0.1f) workTime = 0.1f; // Minimum work time
						__instance.dropWorkTime = workTime;
						__instance.SetWorkTime(workTime);
						UnityEngine.Debug.Log($"[EmptyStorage] DropAll Prefix - Calculated work time: {workTime}s based on mass: {massStored}kg");
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
					
					UnityEngine.Debug.Log($"[EmptyStorage] DropAll Postfix - Starting");
					
					// Check if a chore was created (not cancelled)
					var choreProperty = typeof(DropAllWorkable).GetProperty("Chore", BindingFlags.NonPublic | BindingFlags.Instance);
					var chore = choreProperty?.GetValue(__instance);
					
					UnityEngine.Debug.Log($"[EmptyStorage] DropAll Postfix - Chore: {(chore != null ? "exists" : "null")}");
					
					if (chore != null)
					{
						// Chore exists - set work time again in case it wasn't set correctly in Prefix
						// This ensures the work time is correct even if storages weren't initialized in Prefix
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
							float workTime = massStored / 100f;
							if (workTime < 0.1f) workTime = 0.1f; // Minimum work time
							if (workTime > 0f)
							{
								__instance.dropWorkTime = workTime;
								__instance.SetWorkTime(workTime);
								UnityEngine.Debug.Log($"[EmptyStorage] DropAll Postfix - Set work time: {workTime}s based on mass: {massStored}kg");
							}
						}
						
						// Enable skill perk status item so it shows if no qualified worker
						// Use SetShouldShowSkillPerkStatusItem which properly subscribes to skill updates and calls UpdateStatusItem
						UnityEngine.Debug.Log($"[EmptyStorage] DropAll Postfix - Enabling skill perk status item");
						__instance.SetShouldShowSkillPerkStatusItem(true);
						
						// Ensure workable is enabled (required for status item to show)
						__instance.enabled = true;
						
						// Verify requiredSkillPerk is set
						var requiredSkillPerk = __instance.requiredSkillPerk;
						UnityEngine.Debug.Log($"[EmptyStorage] DropAll - Chore created, enabled skill perk status item. requiredSkillPerk: {requiredSkillPerk}, enabled: {__instance.enabled}");
					}
					else
					{
						// Chore was cancelled - disable skill perk status item
						// Use SetShouldShowSkillPerkStatusItem which properly unsubscribes and calls UpdateStatusItem
						UnityEngine.Debug.Log($"[EmptyStorage] DropAll Postfix - Disabling skill perk status item");
						__instance.SetShouldShowSkillPerkStatusItem(false);
						
						UnityEngine.Debug.Log($"[EmptyStorage] DropAll - Chore cancelled, disabled skill perk status item");
					}
					
					UnityEngine.Debug.Log($"[EmptyStorage] DropAll Postfix - Completed");
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError($"[EmptyStorage] DropAll Postfix error: {ex.Message}\n{ex.StackTrace}");
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
						UnityEngine.Debug.Log($"[EmptyStorage] OnCompleteWork: No Storage components found");
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
					
					UnityEngine.Debug.Log($"[EmptyStorage] OnCompleteWork: Completed safely");
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

		// Patch the Chore property setter to detect when chore is cancelled
		// This is the most reliable way to detect cancellation - we hook directly into when Chore is set to null
		[HarmonyPatch(typeof(Workable), "set_Chore")]
		public static class Workable_set_Chore_Patch
		{
			internal static void Postfix(Workable __instance, Chore value)
			{
				try
				{
					// Only handle DropAllWorkable instances with our component
					var dropAllWorkable = __instance as DropAllWorkable;
					if (dropAllWorkable == null)
						return;
					
					var emptyStorageSetting = dropAllWorkable.gameObject.GetComponent<EmptyStorageSetting>();
					if (emptyStorageSetting == null)
						return;
					
					// If Chore is being set to null, the chore was cancelled
					if (value == null)
					{
						BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
						var shouldShowField = typeof(DropAllWorkable).GetField("shouldShowSkillPerkStatusItem", bindingAttr);
						if (shouldShowField != null)
						{
							var shouldShow = (bool)(shouldShowField.GetValue(dropAllWorkable) ?? false);
							if (shouldShow)
							{
								// Chore is being set to null but shouldShow is still true - disable it immediately
								dropAllWorkable.SetShouldShowSkillPerkStatusItem(false);
								
								// Call RefreshStatusItem to update the status item display
								var refreshMethod = typeof(DropAllWorkable).GetMethod("RefreshStatusItem", BindingFlags.NonPublic | BindingFlags.Instance);
								refreshMethod?.Invoke(dropAllWorkable, null);
								
								UnityEngine.Debug.Log($"[EmptyStorage] Workable.set_Chore - Chore cancelled, disabled shouldShowSkillPerkStatusItem");
							}
						}
					}
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError($"[EmptyStorage] Workable.set_Chore Postfix error: {ex.Message}\n{ex.StackTrace}");
				}
			}
		}

		// Patch RefreshStatusItem to handle cleanup when chore is cancelled
		// This is a backup in case the Chore setter patch doesn't catch all cases
		[HarmonyPatch(typeof(DropAllWorkable), "RefreshStatusItem")]
		public static class DropAllWorkable_RefreshStatusItem_Patch
		{
			internal static void Postfix(DropAllWorkable __instance)
			{
				try
				{
					// Only handle this for storages with our component
					var emptyStorageSetting = __instance.gameObject.GetComponent<EmptyStorageSetting>();
					if (emptyStorageSetting == null)
						return;
					
					// Check if chore is null
					var choreProperty = typeof(DropAllWorkable).GetProperty("Chore", BindingFlags.NonPublic | BindingFlags.Instance);
					var chore = choreProperty?.GetValue(__instance);
					
					// If chore is null, ensure shouldShowSkillPerkStatusItem is also false
					// This handles the case where the cancel tool cancels the chore directly
					if (chore == null)
					{
						var shouldShowField = typeof(DropAllWorkable).GetField("shouldShowSkillPerkStatusItem", BindingFlags.Instance | BindingFlags.NonPublic);
						if (shouldShowField != null)
						{
							var shouldShow = (bool)(shouldShowField.GetValue(__instance) ?? false);
							if (shouldShow)
							{
								// Chore is null but shouldShow is still true - disable it
								__instance.SetShouldShowSkillPerkStatusItem(false);
								UnityEngine.Debug.Log($"[EmptyStorage] RefreshStatusItem - Chore is null, disabled shouldShowSkillPerkStatusItem");
							}
						}
					}
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError($"[EmptyStorage] RefreshStatusItem Postfix error: {ex.Message}\n{ex.StackTrace}");
				}
			}
		}
		
		// Patch OnRefreshUserMenu to also check for cancelled chores and clean up
		// This is called periodically and when the user menu is refreshed, so it's a good place
		// to detect if a chore was cancelled by the cancel tool
		// Note: This is a separate patch from the one that blocks the button (which uses Prefix)
		// Harmony allows multiple patches on the same method, and Postfix runs after Prefix
		[HarmonyPatch(typeof(DropAllWorkable), "OnRefreshUserMenu")]
		public static class DropAllWorkable_OnRefreshUserMenu_Cleanup_Patch
		{
			internal static void Postfix(DropAllWorkable __instance, object data)
			{
				try
				{
					// Only handle this for storages with our component
					var emptyStorageSetting = __instance.gameObject.GetComponent<EmptyStorageSetting>();
					if (emptyStorageSetting == null)
						return;
					
					// Check if chore is null
					var choreProperty = typeof(DropAllWorkable).GetProperty("Chore", BindingFlags.NonPublic | BindingFlags.Instance);
					var chore = choreProperty?.GetValue(__instance);
					
					BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
					var shouldShowField = typeof(DropAllWorkable).GetField("shouldShowSkillPerkStatusItem", bindingAttr);
					var shouldShow = shouldShowField != null ? (bool)(shouldShowField.GetValue(__instance) ?? false) : false;
					
					UnityEngine.Debug.Log($"[EmptyStorage] OnRefreshUserMenu Cleanup - Chore: {(chore != null ? "exists" : "null")}, shouldShow: {shouldShow}");
					
					// If chore is null, ensure shouldShowSkillPerkStatusItem is also false
					if (chore == null && shouldShow)
					{
						// Chore is null but shouldShow is still true - disable it
						__instance.SetShouldShowSkillPerkStatusItem(false);
						UnityEngine.Debug.Log($"[EmptyStorage] OnRefreshUserMenu - Chore is null, disabled shouldShowSkillPerkStatusItem");
					}
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError($"[EmptyStorage] OnRefreshUserMenu Postfix error: {ex.Message}\n{ex.StackTrace}");
				}
			}
		}

		// Note: Cannot patch Chore.Cancel directly as it's abstract
		// RefreshStatusItem patch above handles cleanup when chore is cancelled by any means

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
						UnityEngine.Debug.Log($"[EmptyStorage] OnSpawn - Set showCmd to false");
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
									UnityEngine.Debug.Log($"[EmptyStorage] UserMenu.AddButton - Blocking DropAllWorkable button (dupe selected: {selectedObject.name})");
									return false; // Skip adding the button
								}
							}
						}
					}
				}
				return true; // Allow the button to be added
			}
		}

		// Patch DropAllWorkable to debug work creation
		[HarmonyPatch(typeof(DropAllWorkable), "OnPrefabInit")]
		public static class DropAllWorkable_OnPrefabInit_Patch
		{
			internal static void Postfix(DropAllWorkable __instance)
			{
				UnityEngine.Debug.Log($"[EmptyStorage] DropAllWorkable.OnPrefabInit - enabled: {__instance.enabled}");
			}
		}
	}
}

