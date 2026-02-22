using UnityEngine;

namespace ControlledMods.Components
{
    /// <summary>
    /// Placed on the VineBranch prefab by ControlledMods when Customizable Plants has max_age set for VineBranch.
    /// VineBranch.Instance constructor reads this and overrides the hardcoded 2400f oldAge max.
    /// </summary>
    public class VineBranchMaxAgeOverride : MonoBehaviour
    {
        /// <summary>
        /// Max age in seconds (from Customizable Plants config). If &lt;= 0, branch effectively never grows old.
        /// </summary>
        public float MaxAge = 2400f;
    }
}
