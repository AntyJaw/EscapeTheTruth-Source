using UnityEngine;
using EtT.Core;
using EtT.Teams;

namespace EtT.Coordinator
{
    // MVP: działa także w Editorze – pozwala przetestować "biuro" bez Steama.
    public sealed class SteamCoordinatorStub : ISteamCoordinator
    {
        [SerializeField] private bool coordinatorMode = false;

        public bool IsCoordinatorMode => coordinatorMode;

        public void OpenCaseBoard()
        {
            if (!coordinatorMode) return;
            GameEvents.RaiseLink13("[Coordinator] Case Board opened.");
        }

        public void AssignTaskToMobile(string playerId, string taskText)
        {
            if (!coordinatorMode) return;
            var packet = new EvidencePacket{
                id=System.Guid.NewGuid().ToString("N"),
                fromUserId="COORD",
                kind="task",
                metaJson=taskText,
                unixTime=System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            GameEvents.RaiseEvidenceReceived(packet);
        }

        public void AcceptEvidence(EvidencePacket packet)
        {
            if (!coordinatorMode) return;
            GameEvents.RaiseLink13($"[Coordinator] Evidence accepted: {packet.id}/{packet.kind}");
        }

        public void Ping(string playerId, string message)
        {
            if (!coordinatorMode) return;
            GameEvents.RaiseCoordinatorPing(playerId, message);
        }
    }
}