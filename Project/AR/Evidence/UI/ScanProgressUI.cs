using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EtT.AR.Evidence.UI
{
    /// <summary>
    /// Podłącz ten skrypt do Canvasu AR, a w Inspectorze przypnij:
    /// - referencję do EvidenceScanner,
    /// - Slider (0..1) i TMP_Text na podpowiedzi trybu (UV/EM/Runy).
    /// </summary>
    public sealed class ScanProgressUI : MonoBehaviour
    {
        [SerializeField] private EtT.AR.Evidence.EvidenceScanner scanner;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text hint;

        void OnEnable()
        {
            if (!scanner) return;
            scanner.onChargeChanged.AddListener(OnCharge);
            scanner.onHintChanged.AddListener(OnHint);
        }

        void OnDisable()
        {
            if (!scanner) return;
            scanner.onChargeChanged.RemoveListener(OnCharge);
            scanner.onHintChanged.RemoveListener(OnHint);
        }

        void OnCharge(float v)
        {
            if (slider) slider.value = v;
        }

        void OnHint(string h)
        {
            if (hint) hint.text = h;
        }
    }
}