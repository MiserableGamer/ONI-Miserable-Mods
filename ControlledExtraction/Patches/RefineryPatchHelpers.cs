using UnityEngine;

namespace ControlledExtraction.Patches
{
    public static class RefineryPatchHelpers
    {
        public static void SetOutputToStorage(GameObject go, SimHashes element)
        {
            var converter = go.GetComponent<ElementConverter>();
            if (converter?.outputElements == null) return;

            for (int i = 0; i < converter.outputElements.Length; i++)
            {
                if (converter.outputElements[i].elementHash == element)
                {
                    var output = converter.outputElements[i];
                    output.storeOutput = true;
                    converter.outputElements[i] = output;
                    break;
                }
            }
        }
    }
}
