using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EtT.Services;
using EtT.AR.Layers;

namespace EtT.UI.AR
{
    /// <summary>
    /// Prosty HUD warstw AR: pięć przycisków + etykieta trybu.
    /// Przypnij na Canvasie AR i podłącz referencje przycisków.
    /// </summary>
    public sealed class ARLayerHUD : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button btnPhysical;
        [SerializeField] private Button btnUV;
        [SerializeField] private Button btnEM;
        [SerializeField] private Button btnRunic;
        [SerializeField] private Button btnPsychic;
        [SerializeField] private TMP_Text modeLabel;

        IARLayerModeService _layers;

        void Awake()
        {
            _layers = ServiceLocator.Get<IARLayerModeService>();

            Wire(btnPhysical, () => _layers.Set(ARLayerMode.Physical));
            Wire(btnUV,       () => _layers.Set(ARLayerMode.UV));
            Wire(btnEM,       () => _layers.Set(ARLayerMode.EM));
            Wire(btnRunic,    () => _layers.Set(ARLayerMode.Runic));
            Wire(btnPsychic,  () => _layers.Set(ARLayerMode.Psychic));

            _layers.OnChanged += OnModeChanged;
            OnModeChanged(_layers.Current);
        }

        void OnDestroy()
        {
            if (_layers != null) _layers.OnChanged -= OnModeChanged;
        }

        void Wire(Button b, System.Action a) { if (b) b.onClick.AddListener(() => a()); }

        void OnModeChanged(ARLayerMode m)
        {
            if (!modeLabel) return;
            modeLabel.text = m switch
            {
                ARLayerMode.Physical => "Tryb: FIZYCZNY",
                ARLayerMode.UV       => "Tryb: UV",
                ARLayerMode.EM       => "Tryb: EM",
                ARLayerMode.Runic    => "Tryb: RUNICZNY",
                ARLayerMode.Psychic  => "Tryb: PSYCHICZNY",
                _ => "Tryb"
            };
        }
    }
}