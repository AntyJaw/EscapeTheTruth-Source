using UnityEngine;
using TMPro;
using EtT.POI;
using EtT.Core;

namespace EtT.UI.Placeholder
{
    public sealed class PoiPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text status;
        [SerializeField] private PoiService poi;

        private PoiInstance _lastEntered;

        private void OnEnable()
        {
            GameEvents.OnPoiEntered += OnEntered;
            GameEvents.OnPoiLeft += OnLeft;
            GameEvents.OnPoiInteracted += OnInteracted;
        }
        private void OnDisable()
        {
            GameEvents.OnPoiEntered -= OnEntered;
            GameEvents.OnPoiLeft -= OnLeft;
            GameEvents.OnPoiInteracted -= OnInteracted;
        }

        private void OnEntered(PoiInstance p){ _lastEntered = p; if (status) status.text = $"W pobliżu: {p.name} ({p.type})"; }
        private void OnLeft(PoiInstance p){ if (_lastEntered!=null && _lastEntered.id==p.id) { _lastEntered = null; if (status) status.text=""; } }
        private void OnInteracted(PoiInstance p, PoiResult r){ if (status) status.text = $"{p.name}: {(r.ok?"sukces":"odmowa")} – {r.message}"; }

        public void Interact()
        {
            if (_lastEntered == null) { if (status) status.text = "Brak POI w zasięgu"; return; }
            var res = poi.Interact(_lastEntered.id);
            if (status) status.text = $"{_lastEntered.name}: {(res.ok?"OK":"X")} – {res.message}";
        }
    }
}