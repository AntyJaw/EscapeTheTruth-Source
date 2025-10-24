using UnityEngine;

namespace EtT.Evidence.Analysis
{
    /// <summary>Definicja dowodu do analizy – asset przypinany w UI/analizatorze.</summary>
    [CreateAssetMenu(fileName = "EvidenceItemData", menuName = "EtT/Evidence/Item Data")]
    public class EvidenceItemData : ScriptableObject
    {
        [Header("Identyfikator (musi odpowiadać temu zebranym w AR)")]
        public string evidenceId = "ev_001";

        [Header("Wygląd / Prefab podglądu 3D")]
        [Tooltip("Lekki prefab (LOD) do analizy w biurze.")]
        public GameObject previewPrefab;

        [Header("Warstwy (czy posiada)")]
        public bool hasUV;
        public bool hasEM;
        public bool hasRunic;
        public bool hasPsychic;

        [Header("Progi odkryć (0..1) – ile ładowania potrzeba na warstwie")]
        [Range(0.1f,1f)] public float physicalThreshold = 0.35f;
        [Range(0.1f,1f)] public float uvThreshold = 0.6f;
        [Range(0.1f,1f)] public float emThreshold = 0.55f;
        [Range(0.1f,1f)] public float runicThreshold = 0.7f;
        [Range(0.1f,1f)] public float psychicThreshold = 0.8f;

        [Header("Opis / metadane")]
        [TextArea] public string description;
        public string locationHint;  // np. skąd pochodzi, powiązania
    }
}