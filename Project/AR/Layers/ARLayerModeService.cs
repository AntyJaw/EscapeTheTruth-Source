using UnityEngine;

namespace EtT.AR.Layers
{
    public sealed class ARLayerModeService : IARLayerModeService
    {
        public ARLayerMode Current { get; private set; } = ARLayerMode.Physical;
        public event System.Action<ARLayerMode> OnChanged;

        public void Set(ARLayerMode mode)
        {
            if (Current == mode) return;
            Current = mode;
            PlayerPrefs.SetInt("ar_layer_mode", (int)mode);
            OnChanged?.Invoke(Current);
        }

        public void CycleNext()
        {
            var next = (int)Current + 1;
            if (next > (int)ARLayerMode.Psychic) next = 0;
            Set((ARLayerMode)next);
        }

        public ARLayerModeService()
        {
            if (PlayerPrefs.HasKey("ar_layer_mode"))
                Current = (ARLayerMode)Mathf.Clamp(PlayerPrefs.GetInt("ar_layer_mode"), 0, 4);
        }
    }
}