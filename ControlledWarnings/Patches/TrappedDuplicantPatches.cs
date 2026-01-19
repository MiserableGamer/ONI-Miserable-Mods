using System.Collections.Generic;
using HarmonyLib;

namespace ControlledWarnings.Patches
{
    [HarmonyPatch(typeof(TrappedDuplicantDiagnostic), nameof(TrappedDuplicantDiagnostic.CheckTrapped))]
    public static class TrappedDuplicantDiagnostic_CheckTrapped_Patch
    {
        // Runs after the diagnostic check - we check all dupes since diagnostic only reports the first
        public static void Postfix(TrappedDuplicantDiagnostic __instance)
        {
            int worldId = GetWorldId(__instance);
            if (worldId < 0) return;

            var minions = Components.LiveMinionIdentities.GetWorldItems(worldId, false);
            if (minions == null) return;

            foreach (MinionIdentity minion in minions)
            {
                if (minion == null) continue;

                var world = ClusterManager.Instance?.GetWorld(worldId);
                if (world != null && world.IsModuleInterior) continue;

                int dupeId = minion.GetInstanceID();

                if (AlertTracker.HasActiveAlert(dupeId))
                {
                    // Only clear alert when dupe can actually reach a safe destination
                    if (CanReachSafeDestination(minion, worldId))
                    {
                        ControlledWarningsMod.DebugLog($"{minion.GetProperName()} can now reach safety - clearing alert");
                        AlertTracker.HandleFreedDupe(minion);
                    }
                    else
                    {
                        // Still trapped - check if suffocating status changed (for escalation)
                        bool isSuffocating = CheckIfSuffocating(minion);
                        AlertTracker.HandleTrappedDupe(minion, isSuffocating);
                    }
                }
                else
                {
                    // No active alert - check if we should create one
                    if (CheckIfMinionTrapped(minion, worldId))
                    {
                        bool isSuffocating = CheckIfSuffocating(minion);
                        AlertTracker.HandleTrappedDupe(minion, isSuffocating);
                    }
                }
            }
        }

        private static int GetWorldId(TrappedDuplicantDiagnostic instance)
        {
            var field = typeof(ColonyDiagnostic).GetField("worldID", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public);
            
            if (field != null)
                return (int)field.GetValue(instance);
            
            var prop = typeof(ColonyDiagnostic).GetProperty("worldID", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);
            
            if (prop != null)
                return (int)prop.GetValue(instance);
            
            return -1;
        }

        // Checks if dupe is trapped (used for alert creation)
        private static bool CheckIfMinionTrapped(MinionIdentity minion, int worldId)
        {
            if (!CheckMinionBasicallyIdle(minion)) return false;

            Navigator navigator = minion.GetComponent<Navigator>();
            if (navigator == null) return false;

            // Check if can reach any non-idle minion
            var otherMinions = Components.LiveMinionIdentities.GetWorldItems(worldId, false);
            foreach (MinionIdentity other in otherMinions)
            {
                if (other == null || other == minion) continue;

                if (!CheckMinionBasicallyIdle(other))
                {
                    var approachable = other.GetComponent<IApproachable>();
                    if (approachable != null && navigator.CanReach(approachable))
                        return false;
                }
            }

            // Check if can reach telepad
            var telepads = Components.Telepads.GetWorldItems(navigator.GetMyWorld().id, false);
            if (telepads != null && telepads.Count > 0)
            {
                var telepadApproachable = telepads[0].GetComponent<IApproachable>();
                if (telepadApproachable != null && navigator.CanReach(telepadApproachable))
                    return false;
            }

            // Check if can reach warp receiver
            var warpReceivers = Components.WarpReceivers.GetWorldItems(navigator.GetMyWorld().id, false);
            if (warpReceivers != null)
            {
                foreach (WarpReceiver receiver in warpReceivers)
                {
                    if (receiver == null) continue;
                    var receiverApproachable = receiver.GetComponent<IApproachable>();
                    if (receiverApproachable != null && navigator.CanReach(receiverApproachable))
                        return false;
                }
            }

            // Check if can reach assigned bed
            var beds = Components.NormalBeds.WorldItemsEnumerate(navigator.GetMyWorldId(), true);
            foreach (Sleepable bed in beds)
            {
                if (bed == null) continue;
                var assignable = bed.assignable;
                if (assignable != null && assignable.IsAssignedTo(minion))
                {
                    if (bed.approachable != null && navigator.CanReach(bed.approachable))
                        return false;
                }
            }

            return true;
        }

        // Checks if dupe can reach any safe destination (used for alert clearing)
        private static bool CanReachSafeDestination(MinionIdentity minion, int worldId)
        {
            Navigator navigator = minion.GetComponent<Navigator>();
            if (navigator == null) return false;

            var telepads = Components.Telepads.GetWorldItems(navigator.GetMyWorld().id, false);
            if (telepads != null && telepads.Count > 0)
            {
                var telepadApproachable = telepads[0].GetComponent<IApproachable>();
                if (telepadApproachable != null && navigator.CanReach(telepadApproachable))
                    return true;
            }

            var warpReceivers = Components.WarpReceivers.GetWorldItems(navigator.GetMyWorld().id, false);
            if (warpReceivers != null)
            {
                foreach (WarpReceiver receiver in warpReceivers)
                {
                    if (receiver == null) continue;
                    var receiverApproachable = receiver.GetComponent<IApproachable>();
                    if (receiverApproachable != null && navigator.CanReach(receiverApproachable))
                        return true;
                }
            }

            var beds = Components.NormalBeds.WorldItemsEnumerate(navigator.GetMyWorldId(), true);
            foreach (Sleepable bed in beds)
            {
                if (bed == null) continue;
                var assignable = bed.assignable;
                if (assignable != null && assignable.IsAssignedTo(minion))
                {
                    if (bed.approachable != null && navigator.CanReach(bed.approachable))
                        return true;
                }
            }

            var otherMinions = Components.LiveMinionIdentities.GetWorldItems(worldId, false);
            foreach (MinionIdentity other in otherMinions)
            {
                if (other == null || other == minion) continue;

                if (!CheckMinionBasicallyIdle(other))
                {
                    var approachable = other.GetComponent<IApproachable>();
                    if (approachable != null && navigator.CanReach(approachable))
                        return true;
                }
            }

            return false;
        }

        private static bool CheckMinionBasicallyIdle(MinionIdentity minion)
        {
            var prefabId = minion.GetComponent<KPrefabID>();
            if (prefabId == null) return false;

            return prefabId.HasTag(GameTags.Idle) || 
                   prefabId.HasTag(GameTags.RecoveringBreath) || 
                   prefabId.HasTag(GameTags.MakingMess);
        }

        private static bool CheckIfSuffocating(MinionIdentity minion)
        {
            if (minion == null) return false;

            // Check SuffocationMonitor
            var smc = minion.GetComponent<StateMachineController>();
            if (smc != null)
            {
                var suffocationSmi = smc.GetSMI<SuffocationMonitor.Instance>();
                if (suffocationSmi != null && suffocationSmi.IsSuffocating())
                    return true;
            }

            // Check OxygenBreather
            var oxygenBreather = minion.GetComponent<OxygenBreather>();
            if (oxygenBreather != null && oxygenBreather.IsOutOfOxygen)
                return true;

            // Check NoOxygen tag
            var prefabId = minion.GetComponent<KPrefabID>();
            if (prefabId != null && prefabId.HasTag(GameTags.NoOxygen))
                return true;

            return false;
        }
    }

    [HarmonyPatch(typeof(Game), "OnDestroy")]
    public static class Game_OnDestroy_Patch
    {
        public static void Prefix()
        {
            AlertTracker.ClearAllAlerts();
        }
    }
}
