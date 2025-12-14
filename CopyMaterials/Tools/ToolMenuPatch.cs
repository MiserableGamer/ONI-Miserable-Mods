using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CopyMaterialsTool
{
    // IMPORTANT: Patch InstantiateCollectionsUI so our ToolInfo exists BEFORE toggles are created
    [HarmonyPatch(typeof(ToolMenu), "InstantiateCollectionsUI")]
    public static class ToolMenu_InstantiateCollectionsUI_Patch
    {
        private static bool _added;

        public static void Prefix(ToolMenu __instance)
        {
            if (_added) return;
            _added = true;

            try
            {
                // Create the tool (DragTool is a MonoBehaviour)
                var go = new GameObject("CopyMaterialsTool");
                go.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(go);
                var tool = go.AddComponent<CopyMaterialsTool>();

                var toolInfoType = AccessTools.Inner(typeof(ToolMenu), "ToolInfo");
                var toolCollectionType = AccessTools.Inner(typeof(ToolMenu), "ToolCollection");
                if (toolInfoType == null || toolCollectionType == null)
                {
                    Debug.LogError("[CopyMaterialsTool] ToolInfo/ToolCollection type not found");
                    return;
                }

                // ToolMenu.basicTools : List<ToolCollection>
                object basicToolsObj = Traverse.Create(__instance).Field("basicTools").GetValue();
                IList basicCollections = basicToolsObj as IList;
                if (basicCollections == null || basicCollections.Count == 0)
                {
                    Debug.LogError("[CopyMaterialsTool] basicTools list not found/empty");
                    return;
                }

                // Choose the collection that already contains CopySettingsTool if possible
                object targetCollection = FindCollectionContainingToolName(basicCollections, "CopySettingsTool", toolInfoType);
                if (targetCollection == null)
                    targetCollection = basicCollections[0];

                // Avoid duplicates if Prefix runs more than once
                if (CollectionAlreadyHasTool(targetCollection, "CopyMaterialsTool", toolInfoType))
                    return;

                // Find the exact ToolInfo ctor you dumped:
                // (string text, string icon_name, Action hotkey, string ToolName, ToolCollection toolCollection,
                //  string tooltip, Action<object> onSelectCallback, object toolData)
                ConstructorInfo ctor = toolInfoType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(c => {
                        var p = c.GetParameters();
                        return p.Length == 8
                            && p[0].ParameterType == typeof(string)
                            && p[1].ParameterType == typeof(string)
                            && p[2].ParameterType.Name == "Action" // game Action type (NOT System.Action)
                            && p[3].ParameterType == typeof(string)
                            && toolCollectionType.IsAssignableFrom(p[4].ParameterType)
                            && p[5].ParameterType == typeof(string)
                            && p[6].ParameterType == typeof(Action<object>)
                            && p[7].ParameterType == typeof(object);
                    });

                if (ctor == null)
                {
                    Debug.LogError("[CopyMaterialsTool] ToolInfo ctor not found");
                    return;
                }

                Type hotkeyType = ctor.GetParameters()[2].ParameterType;
                object noHotkey = GetNoHotkeyValue(hotkeyType);

                object toolInfo = ctor.Invoke(new object[] {
                    "Copy Materials",                              // text
					"icon_action_copysettings",                    // icon_name (string)
					noHotkey,                                      // hotkey (game Action type)
					"CopyMaterialsTool",                           // ToolName
					targetCollection,                              // toolCollection
					"Copy construction materials; drag to apply",  // tooltip
					new Action<object>(obj => ToolActivator.Activate(obj as InterfaceTool)),
                    tool                                           // toolData passed to callback
				});

                // Add to ToolCollection.tools (NOT a random IList field)
                if (!TryAddToCollectionToolsList(targetCollection, toolInfo, toolInfoType))
                {
                    Debug.LogError("[CopyMaterialsTool] Failed to add ToolInfo to ToolCollection.tools");
                    return;
                }

                Debug.Log("[CopyMaterialsTool] Tool registered (before UI build)");
            }
            catch (Exception e)
            {
                Debug.LogError("[CopyMaterialsTool] Error registering tool: " + e);
            }
        }

        private static bool TryAddToCollectionToolsList(object collection, object toolInfo, Type toolInfoType)
        {
            // ToolCollection almost always has a field named "tools"
            var toolsField = collection.GetType().GetField("tools", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (toolsField == null) return false;

            var list = toolsField.GetValue(collection) as IList;
            if (list == null) return false;

            list.Add(toolInfo);
            return true;
        }

        private static bool CollectionAlreadyHasTool(object collection, string toolName, Type toolInfoType)
        {
            var toolsField = collection.GetType().GetField("tools", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (toolsField == null) return false;

            var list = toolsField.GetValue(collection) as IList;
            if (list == null) return false;

            for (int i = 0; i < list.Count; i++)
            {
                var ti = list[i];
                if (ti == null || !toolInfoType.IsInstanceOfType(ti)) continue;
                var tn = Traverse.Create(ti).Field("ToolName").GetValue() as string;
                if (tn == toolName) return true;
            }
            return false;
        }

        private static object FindCollectionContainingToolName(IList collections, string toolName, Type toolInfoType)
        {
            for (int c = 0; c < collections.Count; c++)
            {
                var col = collections[c];
                if (col == null) continue;

                var toolsField = col.GetType().GetField("tools", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (toolsField == null) continue;

                var tools = toolsField.GetValue(col) as IList;
                if (tools == null) continue;

                for (int i = 0; i < tools.Count; i++)
                {
                    var ti = tools[i];
                    if (ti == null || !toolInfoType.IsInstanceOfType(ti)) continue;
                    var tn = Traverse.Create(ti).Field("ToolName").GetValue() as string;
                    if (tn == toolName) return col;
                }
            }
            return null;
        }

        private static object GetNoHotkeyValue(Type hotkeyType)
        {
            if (hotkeyType.IsEnum)
            {
                var names = Enum.GetNames(hotkeyType);
                string none = names.FirstOrDefault(n => n.Equals("None", StringComparison.OrdinalIgnoreCase))
                           ?? names.FirstOrDefault(n => n.Equals("Invalid", StringComparison.OrdinalIgnoreCase));
                if (none != null) return Enum.Parse(hotkeyType, none);
                return Enum.ToObject(hotkeyType, 0);
            }
            return hotkeyType.IsValueType ? Activator.CreateInstance(hotkeyType) : null;
        }
    }
}
