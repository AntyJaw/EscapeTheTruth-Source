using UnityEngine;
using EtT.Core;
using EtT.Services;

namespace EtT.Evidence
{
    [RequireComponent(typeof(Collider))]
    public sealed class EvidenceScanTarget : MonoBehaviour
    {
        [SerializeField] private string evidenceId = "evidence_generic";
        [SerializeField] private string displayName = "Ślad";
        [SerializeField] private EvidenceLayer layer = EvidenceLayer.Physical;
        [SerializeField] private ParticleSystem revealFx;

        private bool _collected;

        public void Scan()
        {
            if (_collected) return;
            var ev = new EvidenceItem(evidenceId, displayName, layer);
            ServiceLocator.Get<EvidenceService>().AddEvidence(ev);
            if (revealFx) revealFx.Play();
            _collected = true;
            GameEvents.RaiseLink13($"[Evidence] Zeskanowano: {displayName}");
        }
    }
}