using System;
using UnityEngine;
using STRINGS;

namespace ResourceLimpet.UI
{
    public class RibbonNotifierSidescreen : SideScreenContent
    {
        private ResourceLimpet.Components.RibbonNotifierComponent targetComponent;

        public override string GetTitle()
        {
            return "Ribbon Notifier Settings";
        }

        public override bool IsValidForTarget(GameObject target)
        {
            return target.GetComponent<ResourceLimpet.Components.RibbonNotifierComponent>() != null;
        }

        public override void SetTarget(GameObject target)
        {
            base.SetTarget(target);
            if (target != null)
            {
                targetComponent = target.GetComponent<ResourceLimpet.Components.RibbonNotifierComponent>();
            }
            else
            {
                targetComponent = null;
            }
        }

        public override void ClearTarget()
        {
            targetComponent = null;
            base.ClearTarget();
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            // TODO: Initialize UI elements here
            // For now, this is a basic sidescreen that will be expanded
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // TODO: Set up UI elements here
        }
    }
}

