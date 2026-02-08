using UnityEngine;

namespace ControlledExtraction.Patches
{
    public static class GeneratorPatchHelpers
    {
        public static void SetOutputToStorage(GameObject go, SimHashes element)
        {
            var generator = go.GetComponent<EnergyGenerator>();
            if (generator?.formula.outputs == null) return;

            for (int i = 0; i < generator.formula.outputs.Length; i++)
            {
                if (generator.formula.outputs[i].element == element)
                {
                    generator.formula.outputs[i].store = true;
                    break;
                }
            }
        }
    }
}
