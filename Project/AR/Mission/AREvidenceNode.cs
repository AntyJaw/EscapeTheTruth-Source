using UnityEngine;
using EtT.Services;

namespace EtT.AR.Mission
{
    [RequireComponent(typeof(Collider))]
    public sealed class AREvidenceNode : MonoBehaviour
    {
        [Header("Definicja")]
        [SerializeField] private string evidenceId = "evidence_generic";
        [SerializeField] private string displayName = "Ślad";
        [SerializeField] private EvidenceLayer layer = EvidenceLayer.Physical;

        [Header("FX")]
        [SerializeField] private ParticleSystem revealFx;
        [SerializeField] private AudioSource sfx;

        private bool _collected;

        // Wywoływane przez ARScanner po weryfikacji warstwy i ukończeniu postępu
        public void Collect()
        {
            if (_collected) return;
            _collected = true;

            var evSvc = ServiceLocator.Get<EtT.Evidence.EvidenceService>();
            if (evSvc != null)
            {
                evSvc.AddEvidence(new EtT.Evidence.EvidenceItem(evidenceId, displayName, layer));
                ServiceLocator.Get<ILink13Service>()?.SendSystem($"[Evidence] Zeskanowano: {displayName} [{layer}]");
            }

            if (revealFx) revealFx.Play();
            if (sfx) sfx.Play();

            // podaj progres do ARMissionService
            (ServiceLocator.Get<IARMissionService>() as EtT.AR.Mission.ARMissionService)?.NotifyEvidenceCollected();

            // ukryj obiekt
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            GetComponent<Collider>().enabled = false;
        }

        public EvidenceLayer Layer => layer;
        public string DisplayName => displayName;
    }
}