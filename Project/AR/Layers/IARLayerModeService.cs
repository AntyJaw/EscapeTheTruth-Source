namespace EtT.AR.Layers
{
    public enum ARLayerMode { Physical = 0, UV = 1, EM = 2, Runic = 3, Psychic = 4 }

    public interface IARLayerModeService
    {
        ARLayerMode Current { get; }
        void Set(ARLayerMode mode);
        void CycleNext();
        event System.Action<ARLayerMode> OnChanged;
    }
}