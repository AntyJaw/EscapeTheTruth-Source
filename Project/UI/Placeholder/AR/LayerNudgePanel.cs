using UnityEngine;
using TMPro;
using UnityEngine.UI;
using EtT.Services;

namespace EtT.AR.Hints
{
    /// <summary>
    /// Pokazuje „Przełącz warstwę na UV/EM/Runic/Psychic” i daje szybkie przyciski.
    /// Podłącz w Canvasie: TMP_Text message, oraz opcjonalne cztery przyciski.
    /// </summary>
    public sealed class LayerNudgePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text message;
        [Header("Quick Switch (opcjonalnie)")]
        [SerializeField] private Button btnPhysical;
        [SerializeField] private Button btnUV;
        [SerializeField] private Button btnEM;
        [SerializeField] private Button btnRunic;
        [SerializeField] private Button btnPsychic;

        private void Awake()
        {
            if (btnPhysical) btnPhysical.onClick.AddListener(()=> Set(DetectionLayer.Physical));
            if (btnUV)       btnUV.onClick.AddListener(()=> Set(DetectionLayer.UV));
            if (btnEM)       btnEM.onClick.AddListener(()=> Set(DetectionLayer.EM));
            if (btnRunic)    btnRunic.onClick.AddListener(()=> Set(DetectionLayer.Runic));
            if (btnPsychic)  btnPsychic.onClick.AddListener(()=> Set(DetectionLayer.Psychic));
        }

        public void Suggest(EtT.Evidence.EvidenceLayer layer)
        {
            if (!message) return;
            string needed = layer switch
            {
                EtT.Evidence.EvidenceLayer.UV      => "UV",
                EtT.Evidence.EvidenceLayer.EM      => "EM",
                EtT.Evidence.EvidenceLayer.Runic   => "Runic",
                EtT.Evidence.EvidenceLayer.Psychic => "Psychic",
                _                                   => "Physical"
            };
            message.text = $"Użyj warstwy: {needed}";
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (message) message.text = "";
            gameObject.SetActive(false);
        }

        private void Set(DetectionLayer layer)
        {
            var det = ServiceLocator.Get<IDetectionService>();
            if (det != null) det.SetActive(layer);
        }
    }
}