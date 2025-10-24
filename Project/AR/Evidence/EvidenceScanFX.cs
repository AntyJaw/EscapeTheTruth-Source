using UnityEngine;

namespace EtT.AR.Evidence
{
    /// <summary>
    /// Lekkie podświetlenie celu zależnie od „celowania” i odkrycia.
    /// Podpinaj na ten sam obiekt co EvidenceTarget (lub dziecko z Rendererem).
    /// </summary>
    [RequireComponent(typeof(EvidenceTarget))]
    public sealed class EvidenceScanFX : MonoBehaviour
    {
        public Renderer rend;
        public float aimGlow = 1.5f;
        public float revealedGlow = 2.5f;
        public float damp = 6f;

        EvidenceTarget _t;
        Material _mat;
        float _glow;

        void Awake()
        {
            _t = GetComponent<EvidenceTarget>();
            if (!rend) rend = _t.highlightRenderer ? _t.highlightRenderer : GetComponentInChildren<Renderer>();
            if (rend) _mat = rend.material;
        }

        void OnDestroy()
        {
            if (_mat != null) Destroy(_mat);
        }

        void Update()
        {
            if (_mat == null) return;

            float target = 0f;
            if (_t.isRevealed) target = revealedGlow;
            else if (_t.isAimed) target = aimGlow;

            _glow = Mathf.Lerp(_glow, target, 1f - Mathf.Exp(-damp * Time.deltaTime));

            if (_mat.HasProperty("_EmissionColor"))
            {
                Color baseC = _t.glowColor;
                _mat.SetColor("_EmissionColor", baseC * (0.5f + _glow));
            }
        }
    }
}