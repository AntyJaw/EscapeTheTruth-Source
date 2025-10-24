using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EtT.Evidence.Analysis.UI
{
    /// <summary>Warstwa UI – łączy przyciski/slider z EvidenceAnalyzer.</summary>
    public sealed class EvidenceAnalysisUI : MonoBehaviour
    {
        [Header("Źródło")]
        [SerializeField] private EvidenceAnalyzer analyzer;

        [Header("UI")]
        [SerializeField] private Button btnPhysical;
        [SerializeField] private Button btnUV;
        [SerializeField] private Button btnEM;
        [SerializeField] private Button btnRunic;
        [SerializeField] private Button btnPsychic;

        [SerializeField] private Slider progress;
        [SerializeField] private TMP_Text hint;
        [SerializeField] private TMP_Text findingToast;

        [Header("Model orbit (opcjonalny)")]
        [SerializeField] private ModelOrbit orbit;

        void Awake()
        {
            Wire(btnPhysical, () => analyzer.SetLayer((int)AnalysisLayer.Physical));
            Wire(btnUV,       () => analyzer.SetLayer((int)AnalysisLayer.UV));
            Wire(btnEM,       () => analyzer.SetLayer((int)AnalysisLayer.EM));
            Wire(btnRunic,    () => analyzer.SetLayer((int)AnalysisLayer.Runic));
            Wire(btnPsychic,  () => analyzer.SetLayer((int)AnalysisLayer.Psychic));

            analyzer.onProgress.AddListener(v => { if (progress) progress.value = v; });
            analyzer.onHint.AddListener(h => { if (hint) hint.text = h; });
            analyzer.onFinding.AddListener(FireToast);
            analyzer.onPreviewInstantiated.AddListener(go => { if (orbit) orbit.Bind(go.transform); });
        }

        void Wire(Button b, System.Action a) { if (b) b.onClick.AddListener(() => a()); }

        void FireToast(string msg)
        {
            if (!findingToast) return;
            findingToast.text = msg;
            findingToast.gameObject.SetActive(true);
            CancelInvoke(nameof(HideToast));
            Invoke(nameof(HideToast), 2.2f);
        }

        void HideToast()
        {
            if (findingToast) findingToast.gameObject.SetActive(false);
        }
    }
}