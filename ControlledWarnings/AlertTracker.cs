using System.Collections.Generic;
using PeterHan.PLib.Options;
using UnityEngine;

namespace ControlledWarnings
{
    public static class AlertTracker
    {
        private class TrackedAlert
        {
            public Notification Notification;
            public float LastAlertTime;
            public bool IsCritical;
            public string DupeName;
        }

        private static Dictionary<int, TrackedAlert> activeAlerts = new Dictionary<int, TrackedAlert>();
        private static Dictionary<int, float> cooldownTimers = new Dictionary<int, float>();

        public static void HandleTrappedDupe(MinionIdentity minion, bool isSuffocating)
        {
            if (minion == null) return;

            var options = POptions.ReadSettings<Options.ControlledWarningsOptions>() 
                          ?? new Options.ControlledWarningsOptions();

            if (!options.EnableTrappedAlert) return;

            int dupeId = minion.GetInstanceID();
            string dupeName = minion.GetProperName();
            bool shouldBeCritical = isSuffocating && options.EnableCriticalEscalation;

            ControlledWarningsMod.DebugLog($"HandleTrappedDupe: {dupeName}, suffocating: {isSuffocating}");

            if (activeAlerts.TryGetValue(dupeId, out TrackedAlert existing))
            {
                // Escalate to critical if needed
                if (shouldBeCritical && !existing.IsCritical)
                {
                    RemoveAlert(dupeId);
                    CreateAlert(minion, dupeName, dupeId, shouldBeCritical, options);
                }
                return;
            }

            if (IsOnCooldown(dupeId, options.TrappedCooldown))
                return;

            CreateAlert(minion, dupeName, dupeId, shouldBeCritical, options);
        }

        public static void HandleFreedDupe(MinionIdentity minion, bool clearCooldown = true)
        {
            if (minion == null) return;
            RemoveAlert(minion.GetInstanceID(), clearCooldown);
        }

        public static bool HasActiveAlert(int dupeId)
        {
            return activeAlerts.ContainsKey(dupeId);
        }

        public static void ClearAllAlerts()
        {
            foreach (int id in new List<int>(activeAlerts.Keys))
                RemoveAlert(id);

            activeAlerts.Clear();
            cooldownTimers.Clear();
        }

        private static void CreateAlert(MinionIdentity minion, string dupeName, int dupeId, 
                                        bool isCritical, Options.ControlledWarningsOptions options)
        {
            var notifier = minion.GetComponent<Notifier>();
            if (notifier == null) return;

            string title;
            string tooltip;
            NotificationType notifType;

            if (isCritical)
            {
                title = string.Format(ControlledWarningsStrings.NOTIFICATIONS.TRAPPED.TITLE_CRITICAL, dupeName);
                tooltip = string.Format(ControlledWarningsStrings.NOTIFICATIONS.TRAPPED.TOOLTIP_CRITICAL, dupeName);
                notifType = NotificationType.DuplicantThreatening;
            }
            else
            {
                title = string.Format(ControlledWarningsStrings.NOTIFICATIONS.TRAPPED.TITLE, dupeName);
                tooltip = string.Format(ControlledWarningsStrings.NOTIFICATIONS.TRAPPED.TOOLTIP, dupeName);
                notifType = NotificationType.BadMinor;
            }

            var notification = new Notification(
                title,
                notifType,
                tooltip: (notifications, data) => tooltip,
                tooltip_data: null,
                expires: !options.TrappedPersistent,
                delay: 0f,
                custom_click_callback: null,
                custom_click_data: null,
                click_focus: minion.transform
            );

            notifier.Add(notification);

            activeAlerts[dupeId] = new TrackedAlert
            {
                Notification = notification,
                LastAlertTime = Time.unscaledTime,
                IsCritical = isCritical,
                DupeName = dupeName
            };

            if (isCritical && options.PauseOnCritical)
            {
                if (SpeedControlScreen.Instance != null && !SpeedControlScreen.Instance.IsPaused)
                    SpeedControlScreen.Instance.Pause(false);
            }

            ControlledWarningsMod.DebugLog($"Created {(isCritical ? "CRITICAL" : "warning")} alert for {dupeName}");
        }

        // RESTORE POINT: clearCooldown parameter added for positive confirmation clearing
        // When a dupe is confirmed freed (reached safety), we clear cooldown so re-trapping triggers new alert
        // When alert is dismissed manually, cooldown remains to prevent spam
        private static void RemoveAlert(int dupeId, bool clearCooldown = false)
        {
            if (activeAlerts.TryGetValue(dupeId, out TrackedAlert alert))
            {
                alert.Notification?.Clear();
                
                if (clearCooldown)
                    cooldownTimers.Remove(dupeId);
                else
                    cooldownTimers[dupeId] = Time.unscaledTime;
                
                activeAlerts.Remove(dupeId);
                ControlledWarningsMod.DebugLog($"Removed alert for {alert.DupeName} (cooldown cleared: {clearCooldown})");
            }
        }

        private static bool IsOnCooldown(int dupeId, float cooldownDuration)
        {
            if (cooldownTimers.TryGetValue(dupeId, out float lastTime))
                return (Time.unscaledTime - lastTime) < cooldownDuration;
            return false;
        }
    }
}
