using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using EtT.Services;
using EtT.Core;

namespace EtT.Teams
{
    public sealed class TeamService : ITeamService
    {
        private const string KEY = "team_local_snapshot";
        private readonly ICloudSyncService _cloud;
        private Team _current;
        private readonly List<EvidencePacket> _outbox = new();

        public Team Current => _current;

        public TeamService(ICloudSyncService cloud) { _cloud = cloud; }

        public void Init()
        {
            _cloud.Init();
            LoadLocal();
            if (_current != null) GameEvents.RaiseTeamUpdated(_current);
        }

        public void CreateTeam(string name)
        {
            _current = new Team{
                teamId = Guid.NewGuid().ToString("N"),
                name = name,
                members = new List<Member>(),
                synergy = new Synergy{ compatibility=0.6f, successes=0, conflicts=0, cooperationScore=0.5f },
                updatedAt = DateTime.UtcNow
            };
            SaveLocal();
            SyncNow();
            GameEvents.RaiseTeamUpdated(_current);
        }

        public void JoinTeam(string teamId, TeamRole role)
        {
            // Pull or create
            var snap = _cloud.PullTeam(teamId);
            _current = snap?.team ?? new Team{ teamId = teamId, name = "Ekipa", members=new List<Member>(), synergy=new Synergy{compatibility=0.5f,cooperationScore=0.5f} };
            var myId = ServiceLocator.Get<IProfilesService>().ActiveId;

            if (!_current.members.Any(m => m.userId == myId))
            {
                _current.members.Add(new Member{
                    userId=myId, displayName=myId, platform=DetectPlatform(), role=role, ready=false, psyche=100, energy=100, reputation=50
                });
            }
            _current.updatedAt = DateTime.UtcNow;
            SaveLocal();
            SyncNow();
            GameEvents.RaiseTeamUpdated(_current);
        }

        public void LeaveTeam()
        {
            if (_current == null) return;
            var myId = ServiceLocator.Get<IProfilesService>().ActiveId;
            _current.members.RemoveAll(m => m.userId == myId);
            _current.updatedAt = DateTime.UtcNow;
            SaveLocal();
            SyncNow();
            GameEvents.RaiseTeamUpdated(_current);
        }

        public void SetReadiness(bool ready)
        {
            if (_current == null) return;
            var myId = ServiceLocator.Get<IProfilesService>().ActiveId;
            var me = _current.members.FirstOrDefault(m=>m.userId==myId);
            if (me != null) { me.ready = ready; _current.updatedAt = DateTime.UtcNow; SaveLocal(); SyncNow(); GameEvents.RaiseTeamUpdated(_current); }
        }

        public void UpdateSynergyAfterMission(bool success, int conflicts)
        {
            if (_current == null) return;
            _current.synergy.successes += success ? 1 : 0;
            _current.synergy.conflicts += conflicts;
            _current.synergy.cooperationScore = Mathf.Clamp01(_current.synergy.cooperationScore + (success?0.03f:-0.05f) - conflicts*0.02f);
            _current.updatedAt = DateTime.UtcNow;
            SaveLocal(); SyncNow(); GameEvents.RaiseTeamUpdated(_current);
        }

        public void SendLink13(string message)
        {
            GameEvents.RaiseLink13($"[Team] {message}");
        }

        public void EnqueueEvidence(EvidencePacket packet)
        {
            if (_current == null) return;
            packet.teamId = _current.teamId;
            if (string.IsNullOrEmpty(packet.id)) packet.id = Guid.NewGuid().ToString("N");
            if (packet.unixTime == 0) packet.unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _outbox.Add(packet);
            SaveLocal();
            SyncNow();
        }

        public void SyncNow()
        {
            if (_current == null) return;
            // push team
            _cloud.PushTeam(new TeamSnapshot{ team = _current });
            // push evidence
            if (_outbox.Count > 0)
            {
                _cloud.PushEvidenceQueue(_current.teamId, _outbox.ToArray());
                _outbox.Clear();
                SaveLocal();
            }
            // pull evidence (from others / coordinator)
            var incoming = _cloud.PullEvidenceQueue(_current.teamId);
            if (incoming != null)
                foreach (var p in incoming) GameEvents.RaiseEvidenceReceived(p);
        }

        private Teams.PlatformKind DetectPlatform()
        {
#if UNITY_STANDALONE || UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            return Teams.PlatformKind.Steam;
#elif UNITY_IOS || UNITY_ANDROID
            return Teams.PlatformKind.Mobile;
#else
            return Teams.PlatformKind.Hybrid;
#endif
        }

        // ===== Persist local snapshot (fallback gdy offline) =====
        private void SaveLocal()
        {
            ServiceLocator.Get<ISaveService>().Store(KEY, new Wrapper{ team=_current, outbox=_outbox.ToArray() });
        }
        private void LoadLocal()
        {
            var w = ServiceLocator.Get<ISaveService>().Load<Wrapper>(KEY);
            if (w != null) { _current = w.team; _outbox.Clear(); if (w.outbox!=null) _outbox.AddRange(w.outbox); }
        }
        [Serializable] private class Wrapper { public Team team; public EvidencePacket[] outbox; }
    }
}