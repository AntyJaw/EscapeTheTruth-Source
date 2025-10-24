using UnityEngine;
using TMPro;
using EtT.Core;
using EtT.Teams;

namespace EtT.UI.Placeholder
{
    public sealed class CoordinatorPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text logText;

        private void OnEnable()
        {
            GameEvents.OnEvidenceReceived += OnEv;
            GameEvents.OnCoordinatorPing += OnPing;
        }
        private void OnDisable()
        {
            GameEvents.OnEvidenceReceived -= OnEv;
            GameEvents.OnCoordinatorPing -= OnPing;
        }

        private void OnEv(EvidencePacket p)
        {
            Log($"> Evidence: {p.kind} id:{p.id} meta:{p.metaJson}");
        }

        private void OnPing(string from, string msg)
        {
            Log($"< Ping from {from}: {msg}");
        }

        private void Log(string s)
        {
            if (!logText) return;
            logText.text = s + "\n" + logText.text;
        }
    }
}