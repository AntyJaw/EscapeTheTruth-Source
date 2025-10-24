using UnityEngine;
using TMPro;
using EtT.Services;
using EtT.Player;

namespace EtT.UI.Placeholder
{
    public sealed class AttributePreviewUI : MonoBehaviour
    {
        [Header("UI Teksty")]
        [SerializeField] private TMP_Text radiusText;
        [SerializeField] private TMP_Text degradationText;
        [SerializeField] private TMP_Text analysisText;

        [Header("Bazowe wartości testowe")]
        [SerializeField] private float baseMissionRadius = 100f;   // w metrach
        [SerializeField] private float baseDegradation = 1.0f;     // 1.0 = 100% normalnej degradacji
        [SerializeField] private float baseAnalysis = 0f;          // %
        
        private PlayerService _player;

        private void OnEnable()
        {
            GameEvents.OnAttributesChanged += Refresh;
            _player = ServiceLocator.Get<IPlayerService>() as PlayerService;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnAttributesChanged -= Refresh;
        }

        private void Refresh()
        {
            if (_player == null) return;

            float radiusEff = _player.MissionRadiusModifier(baseMissionRadius);
            float degradationEff = _player.EvidenceDegradationModifier(baseDegradation);
            float analysisEff = _player.AnalysisBoostPercent();

            if (radiusText) radiusText.text = $"Obszar misji: {radiusEff:F0} m";
            if (degradationText) degradationText.text = $"Degradacja dowodów: {(degradationEff*100f):F0}%";
            if (analysisText) analysisText.text = $"Analiza danych: +{analysisEff:F0}%";
        }
    }
}