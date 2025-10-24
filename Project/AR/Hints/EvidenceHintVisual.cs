using UnityEngine;

namespace EtT.AR.Hints
{
    /// <summary>
    /// Steruje Emission materiałów obiektu dowodu:
    /// - MATCH: stabilny glow w kolorze warstwy,
    /// - MISMATCH: delikatny flicker, ostrzegawczy,
    /// - OFF: brak podpowiedzi.
    /// Działa na URP Lit / Standard (ważne: EnableKeyword("_EMISSION")).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EvidenceHintVisual : MonoBehaviour
    {
        [SerializeField] private Renderer[] targets; // jeśli puste, weźmie wszystkie w dzieciach
        [SerializeField] private float distanceBoostStart = 2.5f; // <2.5m lekko wzmacnia
        [SerializeField] private float distanceBoostFactor = 1.25f;

        private MaterialPropertyBlock _mpb;
        private LayerHintConfig _cfg;
        private Camera _cam;

        private bool _enabled;
        private bool _match;
        private Color _color;

        private void Awake()
        {
            if (targets == null || targets.Length == 0)
                targets = GetComponentsInChildren<Renderer>(includeInactive: true);
            _mpb = new MaterialPropertyBlock();
            _cfg = Resources.Load<LayerHintConfig>("LayerHintConfig");
            _cam = Camera.main;
        }

        public void ShowMatch(Color c)  { _enabled = true; _match = true;  _color = c; }
        public void ShowMismatch(Color c){ _enabled = true; _match = false; _color = c; }
        public void Hide() { _enabled = false; Apply(0f, Color.black); }

        private void Update()
        {
            if (!_enabled || _cfg == null) { Hide(); return; }

            float intensity;
            if (_match)
            {
                intensity = _cfg.glowIntensity;
            }
            else
            {
                float t = Time.time * _cfg.flickerSpeed;
                intensity = _cfg.flickerBase + (Mathf.Sin(t) * 0.5f + 0.5f) * _cfg.flickerAmplitude;
            }

            // wzmocnij, gdy kamera blisko (pomaga w AR wskazać „prawie jesteś!”)
            if (_cam != null)
            {
                float d = Vector3.Distance(_cam.transform.position, transform.position);
                if (d < distanceBoostStart) intensity *= Mathf.Lerp(distanceBoostFactor, 1f, d / distanceBoostStart);
            }

            Apply(intensity, _color);
        }

        private void Apply(float intensity, Color col)
        {
            var emis = col * Mathf.Max(0f, intensity);
            foreach (var r in targets)
            {
                if (!r) continue;
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor("_EmissionColor", emis);
                r.SetPropertyBlock(_mpb);
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    m.EnableKeyword("_EMISSION");
                }
            }
        }
    }
}