using UnityEngine;
using EtT.Core;
using EtT.Services;

namespace EtT.AR.Mission
{
    /// <summary>
    /// Przypnij do "uchwytu" (pustego obiektu z colliderem) w scenie AR,
    /// aby można było go "zeskanować".
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class EvidenceScanTarget : MonoBehaviour
    {
        [SerializeField] private string evidenceId = "evidence_generic";
        [SerializeField] private string displayName = "Ślad";
        [SerializeField] private ParticleSystem revealFx;

        public void Scan()
        {
            // TODO: wywołaj prawdziwy EvidenceService, gdy będzie gotowy
            if (revealFx) revealFx.Play();
            GameEvents.RaiseLink13($"[AR] Zeskanowano: {displayName} ({evidenceId})");

            // Przykład: nagroda energii/psyche za znalezienie
            (ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService)?.AddEnergy(+5);
        }
    }
}