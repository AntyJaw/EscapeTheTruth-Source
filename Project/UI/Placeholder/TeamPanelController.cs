using UnityEngine;
using TMPro;
using EtT.Services;
using EtT.Teams;
using EtT.Core;

namespace EtT.UI.Placeholder
{
    public sealed class TeamPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField teamNameOrId;
        [SerializeField] private TMP_Text teamInfo;

        private ITeamService _team;

        private void OnEnable()
        {
            _team = ServiceLocator.Get<ITeamService>();
            GameEvents.OnTeamUpdated += OnTeam;
            Refresh();
        }
        private void OnDisable()
        {
            GameEvents.OnTeamUpdated -= OnTeam;
        }

        public void CreateTeam() { _team.CreateTeam(string.IsNullOrWhiteSpace(teamNameOrId.text) ? "Ekipa" : teamNameOrId.text); }
        public void JoinAsAgent() { _team.JoinTeam(teamNameOrId.text, TeamRole.AgentTerenowy); }
        public void JoinAsCoordinator() { _team.JoinTeam(teamNameOrId.text, TeamRole.Koordynator); }
        public void Ready(bool r) { _team.SetReadiness(r); }
        public void Leave() { _team.LeaveTeam(); }
        public void SyncNow() { _team.SyncNow(); }
        public void PingAll() { _team.SendLink13("Sygnał z LINK-13: sprawdź status!"); }

        private void OnTeam(Team t) => Refresh();
        private void Refresh()
        {
            var t = _team.Current;
            if (t == null) { teamInfo.text = "Brak drużyny."; return; }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Team: {t.name} ({t.teamId})");
            sb.AppendLine($"Members: {t.members.Count}");
            foreach (var m in t.members) sb.AppendLine($"- {m.displayName} [{m.role}] {(m.ready ? "READY":"...")}");
            sb.AppendLine($"Synergy: {t.synergy.cooperationScore:0.00} | succ:{t.synergy.successes} conf:{t.synergy.conflicts}");
            teamInfo.text = sb.ToString();
        }
    }
}