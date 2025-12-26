using System;
using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

namespace ResourceLimpet
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class RibbonNotifierComponent : KMonoBehaviour, ISaveLoadable
    {
        public static readonly HashedString INPUT_PORT_ID = (HashedString)"RibbonNotifierInput";

        [MyCmpReq]
        private LogicPorts logicPorts;

        // Duplicate LogicAlarm fields for sidescreen compatibility
        [Serialize]
        public string notificationName;

        [Serialize]
        public string notificationTooltip;

        [Serialize]
        public NotificationType notificationType;

        [Serialize]
        public bool pauseOnNotify;

        [Serialize]
        public bool zoomOnNotify;

        [Serialize]
        public float cooldown;

        [MyCmpAdd]
        private CopyBuildingSettings copyBuildingSettings;

        private Notifier notifier;
        private Notification notification;
        private Notification lastNotificationCreated;
        private int lastRibbonValue = 0;
        private bool wasOn;

        private static readonly EventSystem.IntraObjectHandler<RibbonNotifierComponent> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<RibbonNotifierComponent>(delegate(RibbonNotifierComponent component, object data)
        {
            component.OnCopySettings(data);
        });

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe<RibbonNotifierComponent>(-905833192, OnCopySettingsDelegate);
        }

        private void OnCopySettings(object data)
        {
            RibbonNotifierComponent component = ((GameObject)data).GetComponent<RibbonNotifierComponent>();
            if (component != null)
            {
                this.notificationName = component.notificationName;
                this.notificationType = component.notificationType;
                this.pauseOnNotify = component.pauseOnNotify;
                this.zoomOnNotify = component.zoomOnNotify;
                this.cooldown = component.cooldown;
                this.notificationTooltip = component.notificationTooltip;
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            this.notifier = base.gameObject.AddComponent<Notifier>();
            Subscribe((int)GameHashes.LogicEvent, OnLogicValueChanged);
            
            // Set defaults if empty (matching LogicAlarm behavior)
            if (string.IsNullOrEmpty(this.notificationName))
            {
                this.notificationName = UI.UISIDESCREENS.LOGICALARMSIDESCREEN.NAME_DEFAULT;
            }
            if (string.IsNullOrEmpty(this.notificationTooltip))
            {
                this.notificationTooltip = UI.UISIDESCREENS.LOGICALARMSIDESCREEN.TOOLTIP_DEFAULT;
            }
            
            UpdateVisualState();
            UpdateNotification(false);
        }

        protected override void OnCleanUp()
        {
            Unsubscribe((int)GameHashes.LogicEvent, OnLogicValueChanged);
            base.OnCleanUp();
        }

        private void UpdateVisualState()
        {
            var kbac = base.GetComponent<KBatchedAnimController>();
            if (kbac != null)
            {
                // Use same anim names as LogicAlarm
                if (this.wasOn)
                {
                    kbac.Play(new HashedString[] { "on_pre", "on_loop" }, KAnim.PlayMode.Once);
                }
                else
                {
                    kbac.Play(new HashedString[] { "on_pst", "off" }, KAnim.PlayMode.Once);
                }
            }
        }

        private void OnLogicValueChanged(object data)
        {
            LogicValueChanged logicValueChanged = (LogicValueChanged)data;
            if (logicValueChanged.portID == INPUT_PORT_ID)
            {
                int ribbonValue = logicValueChanged.newValue;
                
                // Check if channel 0 or 1 is active (matching LogicAlarm behavior for single wire)
                bool isActive = (ribbonValue & ((1 << 0) | (1 << 1))) != 0;
                
                if (isActive)
                {
                    if (!this.wasOn)
                    {
                        PushNotification();
                        this.wasOn = true;
                        if (this.pauseOnNotify && !SpeedControlScreen.Instance.IsPaused)
                        {
                            SpeedControlScreen.Instance.Pause(false, false);
                        }
                        if (this.zoomOnNotify)
                        {
                            GameUtil.FocusCameraOnWorld(base.gameObject.GetMyWorldId(), base.transform.GetPosition(), 8f, null, true);
                        }
                        UpdateVisualState();
                    }
                }
                else if (this.wasOn)
                {
                    this.wasOn = false;
                    UpdateVisualState();
                }
                
                lastRibbonValue = ribbonValue;
            }
        }

        private void PushNotification()
        {
            if (this.notification != null)
            {
                this.notification.Clear();
            }
            this.notifier.Add(this.notification, "");
        }

        public void UpdateNotification(bool clear)
        {
            if (this.notification != null && clear)
            {
                this.notification.Clear();
                this.lastNotificationCreated = null;
            }
            if (this.notification != this.lastNotificationCreated || this.lastNotificationCreated == null)
            {
                this.notification = CreateNotification();
            }
        }

        public Notification CreateNotification()
        {
            base.GetComponent<KSelectable>();
            Notification result = new Notification(
                this.notificationName, 
                this.notificationType, 
                (List<Notification> n, object d) => this.notificationTooltip, 
                null, true, 0f, null, null, null, false, false, false);
            this.lastNotificationCreated = result;
            return result;
        }
    }
}

