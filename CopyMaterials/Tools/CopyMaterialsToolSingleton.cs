using UnityEngine;

namespace CopyMaterialsTool
{
    internal static class CopyMaterialsToolSingleton
    {
        private static CopyMaterialsTool _tool;

        internal static CopyMaterialsTool GetOrCreate()
        {
            if (_tool != null) return _tool;

            var go = new GameObject("CopyMaterialsTool_Instance");
            go.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(go);

            _tool = go.AddComponent<CopyMaterialsTool>();
            return _tool;
        }
    }
}
