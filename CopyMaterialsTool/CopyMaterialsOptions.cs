using PeterHan.PLib.Options;

namespace CopyMaterials
{
    [RestartRequired]
    public sealed class CopyMaterialsOptions
    {
        [Option("Enable Copy Materials", "Enable or disable the Copy Materials feature.")]
        public bool EnableCopyMaterials { get; set; } = true;
    }
}