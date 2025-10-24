using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EtT.Services;
using EtT.Core;
using System.Text;

namespace EtT.UI.Placeholder
{
    public sealed class EvidencePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text evidenceListText;
        [SerializeField] private Button refreshBtn;

        private void OnEnable()
        {
            if (refreshBtn) refreshBtn.onClick.AddListener(RenderList);
            GameEvents.OnEvidenceFound += _ => RenderList();
            RenderList();
        }

        private void OnDisable()
        {
            if (refreshBtn) refreshBtn.onClick.RemoveListener(RenderList);
            GameEvents.OnEvidenceFound -= _ => RenderList();
        }

        private void RenderList()
        {
            var sb = new StringBuilder();
            var service = ServiceLocator.Get<EtT.Evidence.EvidenceService>();
            if (service == null) return;
            foreach (var ev in service.Active)
            {
                string status = ev.Collected ? "✔️" : $"{Mathf.RoundToInt(ev.Integrity*100)}%";
                sb.AppendLine($"{ev.DisplayName} [{ev.Layer}] - {status}");
            }
            evidenceListText.text = sb.ToString();
        }
    }
}