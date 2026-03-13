using KSerialization;

namespace ControlledVisuals.State
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class CleanEdgesSaveState : KMonoBehaviour
    {
        [Serialize]
        private bool cleanEdgesConverted;

        [Serialize]
        private int conversionVersion;

        public bool CleanEdgesConverted
        {
            get => cleanEdgesConverted;
            set => cleanEdgesConverted = value;
        }

        public int ConversionVersion
        {
            get => conversionVersion;
            set => conversionVersion = value;
        }
    }
}
