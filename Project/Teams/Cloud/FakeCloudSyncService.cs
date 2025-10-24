using System.Collections.Generic;
using UnityEngine;

namespace EtT.Teams
{
    // Prosty singleton w pamięci procesu - emuluje serwer.
    public sealed class FakeCloudSyncService : ICloudSyncService
    {
        private static readonly Dictionary<string, TeamSnapshot> _teams = new();
        private static readonly Dictionary<string, List<EvidencePacket>> _queues = new();

        public void Init(){}

        public void PushTeam(TeamSnapshot snap)
        {
            if (snap?.team == null) return;
            _teams[snap.team.teamId] = snap;
            if (!_queues.ContainsKey(snap.team.teamId)) _queues[snap.team.teamId] = new List<EvidencePacket>();
        }

        public TeamSnapshot PullTeam(string teamId)
        {
            _teams.TryGetValue(teamId, out var t); return t;
        }

        public void PushEvidenceQueue(string teamId, EvidencePacket[] packets)
        {
            if (!_queues.ContainsKey(teamId)) _queues[teamId] = new List<EvidencePacket>();
            _queues[teamId].AddRange(packets);
        }

        public EvidencePacket[] PullEvidenceQueue(string teamId)
        {
            if (!_queues.TryGetValue(teamId, out var list) || list.Count == 0) return null;
            var arr = list.ToArray(); list.Clear(); return arr;
        }
    }
}