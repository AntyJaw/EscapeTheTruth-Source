using UnityEngine;
using TMPro;
using EtT.Services;
using EtT.Teams;

namespace EtT.UI.Placeholder
{
    public sealed class EvidenceUploader : MonoBehaviour
    {
        [SerializeField] private TMP_InputField kindField;
        [SerializeField] private TMP_InputField metaField;

        public void Send()
        {
            var team = ServiceLocator.Get<ITeamService>();
            if (team.Current == null) return;

            var myId = ServiceLocator.Get<IProfilesService>().ActiveId;
            var packet = new EvidencePacket{
                fromUserId = myId,
                kind = string.IsNullOrWhiteSpace(kindField.text) ? "photo" : kindField.text,
                metaJson = string.IsNullOrWhiteSpace(metaField.text) ? "{}" : metaField.text
            };
            team.EnqueueEvidence(packet);
        }
    }
}