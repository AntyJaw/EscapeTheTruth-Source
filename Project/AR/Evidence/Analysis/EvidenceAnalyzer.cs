using UnityEngine;
using UnityEngine.Events;
using EtT.Services;

namespace EtT.Evidence.Analysis
{
    public enum AnalysisLayer { Physical, UV, EM, Runic, Psychic }

    /// <summary>
    /// Analiza dowodu w „biurze”: instancjuje podgląd 3D, obsługuje warstwy,
    /// nalicza postęp (0..1) zależnie od umiejętności gracza i odkrywa „ustalenia”.
    /// </summary>
    public sealed class EvidenceAnalyzer : MonoBehaviour
    {
        [Header("Dane (ustaw w Inspectorze lub z kodu)")]
        public EvidenceItemData item;

        [Header("Miejsce podglądu (pusty pivot pod model)")]
        public Transform previewRoot;

        [Header("Szybkość analizy (sekundy do 100% przy warstwie bazowej)")]
        [Range(0.5f, 8f)] public float baseSecondsToFull = 3f;

        [Header("Zdarzenia UI")]
        public UnityEvent<float> onProgress;                 // 0..1
        public UnityEvent<string> onHint;                    // tekst warstwy/hintu
        public UnityEvent<string> onFinding;                 // krótki komunikat odkrycia
        public UnityEvent<GameObject> onPreviewInstantiated; // do podpięcia orbit/FX

        // runtime
        GameObject _preview;
        AnalysisLayer _layer = AnalysisLayer.Physical;
        float _progress;
        bool _foundPhysical, _foundUV, _foundEM, _foundRunic, _foundPsychic;

        void OnDisable()
        {
            ClearPreview();
        }

        public void Load(EvidenceItemData data)
        {
            item = data;
            _progress = 0f;
            _foundPhysical = _foundUV = _foundEM = _foundRunic = _foundPsychic = false;
            onProgress?.Invoke(_progress);
            onHint?.Invoke(item ? item.description : "");
            RebuildPreview();
        }

        public void SetLayer(int layerIndex)
        {
            _layer = (AnalysisLayer)Mathf.Clamp(layerIndex, 0, 4);
            _progress = 0f;
            onProgress?.Invoke(_progress);
            onHint?.Invoke(LayerLabel(_layer));
            ApplyLayerVisuals(_layer);
        }

        void Update()
        {
            if (!item || !_preview) return;

            float seconds = Mathf.Max(0.3f, baseSecondsToFull * DifficultyFactor(_layer));
            _progress = Mathf.MoveTowards(_progress, 1f, Time.deltaTime / seconds);
            onProgress?.Invoke(_progress);

            TryFinishFinding();
        }

        // === Finding / XP / OMUR / EvidenceService ===
        void TryFinishFinding()
        {
            if (!item) return;

            switch (_layer)
            {
                case AnalysisLayer.Physical:
                    if (!_foundPhysical && _progress >= item.physicalThreshold) Found(ref _foundPhysical, "Odkryto: ślady fizyczne", 6);
                    break;
                case AnalysisLayer.UV:
                    if (item.hasUV && !_foundUV && _progress >= item.uvThreshold) Found(ref _foundUV, "Odkryto: ślady UV", 8);
                    break;
                case AnalysisLayer.EM:
                    if (item.hasEM && !_foundEM && _progress >= item.emThreshold) Found(ref _foundEM, "Odkryto: ślady EM", 8);
                    break;
                case AnalysisLayer.Runic:
                    if (item.hasRunic && !_foundRunic && _progress >= item.runicThreshold) Found(ref _foundRunic, "Odkryto: znak runiczny", 10, esoteric:true);
                    break;
                case AnalysisLayer.Psychic:
                    if (item.hasPsychic && !_foundPsychic && _progress >= item.psychicThreshold) Found(ref _foundPsychic, "Odkryto: echo psychiczne", 12, esoteric:true);
                    break;
            }
        }

        void Found(ref bool flag, string msg, int xp, bool esoteric=false)
        {
            flag = true;
            onFinding?.Invoke(msg);

            var player = ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService;
            if (player != null)
            {
                // Personal XP + ewentualnie ezoteryczne
                player.AddPersonalXp(Mathf.RoundToInt(xp * 0.5f));
                if (esoteric) player.AddEsotericXp(Mathf.RoundToInt(xp * 0.8f));
                player.GainSkillXp($"lab_{_layer}", EtT.Skills.SkillKind.Technika, xp, EtT.Skills.XpChannel.Investigation, esoteric);
            }

            // OMUR – prawda obniża napięcie
            ServiceLocator.Get<EtT.World.Omur.IOmurService>()?.Pulse("truth");

            // Oznacz w EvidenceService jako zanalizowane (jeśli istnieje)
            var ev = ServiceLocator.Get<EtT.Evidence.EvidenceService>();
            if (ev != null) ev.MarkAnalyzed(item.evidenceId, _layer.ToString());
        }

        // === Podgląd / materiały
        void RebuildPreview()
        {
            ClearPreview();
            if (item && item.previewPrefab && previewRoot)
            {
                _preview = Instantiate(item.previewPrefab, previewRoot);
                _preview.transform.localPosition = Vector3.zero;
                _preview.transform.localRotation = Quaternion.identity;
                _preview.transform.localScale = Vector3.one;
                onPreviewInstantiated?.Invoke(_preview);
                ApplyLayerVisuals(_layer);
            }
        }

        void ClearPreview()
        {
            if (_preview) Destroy(_preview);
            _preview = null;
        }

        void ApplyLayerVisuals(AnalysisLayer layer)
        {
            // Minimalny „look” pod warstwy – jeżeli w prefabie istnieją tagi materiałów/keywordy,
            // można tu sterować shaderami (EnableKeyword itp.). MVP: zmiana koloru emisji.
            if (!_preview) return;
            var rends = _preview.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                foreach (var m in r.materials)
                {
                    if (!m.HasProperty("_EmissionColor")) continue;
                    switch (layer)
                    {
                        case AnalysisLayer.Physical: m.SetColor("_EmissionColor", Color.black); break;
                        case AnalysisLayer.UV:       m.SetColor("_EmissionColor", new Color(0.7f, 0.2f, 1f)*0.6f); break;
                        case AnalysisLayer.EM:       m.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 1f)*0.6f); break;
                        case AnalysisLayer.Runic:    m.SetColor("_EmissionColor", new Color(1f, 0.6f, 0.2f)*0.6f); break;
                        case AnalysisLayer.Psychic:  m.SetColor("_EmissionColor", new Color(0.6f, 1f, 0.6f)*0.6f); break;
                    }
                }
            }
        }

        // === Balans trudności (im lepsze staty, tym szybciej)
        float DifficultyFactor(AnalysisLayer layer)
        {
            var p = ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService;
            float tech = p != null ? Mathf.Clamp01(p.Attr.Technique / 10f) : 0.4f;
            float perc = p != null ? Mathf.Clamp01(p.Attr.Perception / 10f) : 0.4f;
            float eso  = p != null ? Mathf.Clamp01(p.Ezoteryka / 100f) : 0f;

            float mod = 1.0f;
            switch (layer)
            {
                case AnalysisLayer.Physical: mod = Mathf.Lerp(1.0f, 0.65f, tech); break;
                case AnalysisLayer.UV:       mod = Mathf.Lerp(1.1f, 0.75f, tech*0.7f + perc*0.3f); break;
                case AnalysisLayer.EM:       mod = Mathf.Lerp(1.1f, 0.75f, tech*0.8f + perc*0.2f); break;
                case AnalysisLayer.Runic:    mod = Mathf.Lerp(1.25f, 0.8f, eso*0.8f + perc*0.2f); break;
                case AnalysisLayer.Psychic:  mod = Mathf.Lerp(1.3f, 0.85f, eso); break;
            }
            // lekki wpływ napięcia OMUR (utrudnia przy wysokim)
            var omur = ServiceLocator.Get<EtT.World.Omur.IOmurService>();
            if (omur != null) mod *= Mathf.Lerp(1.0f, 1.15f, Mathf.Clamp01(omur.CurrentTension/100f));
            return Mathf.Clamp(mod, 0.5f, 1.5f);
        }

        string LayerLabel(AnalysisLayer layer) =>
            layer switch
            {
                AnalysisLayer.Physical => "Warstwa: FIZYCZNA",
                AnalysisLayer.UV       => "Warstwa: UV",
                AnalysisLayer.EM       => "Warstwa: EM",
                AnalysisLayer.Runic    => "Warstwa: RUNICZNA",
                AnalysisLayer.Psychic  => "Warstwa: PSYCHICZNA",
                _ => "Warstwa"
            };
    }
}