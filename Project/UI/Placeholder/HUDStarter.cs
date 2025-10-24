using UnityEngine;
using TMPro;
using UnityEngine.UI;
using EtT.Services;

namespace EtT.UI.Placeholder
{
    /// <summary>
    /// Prosty HUD do startu: paski + przycisk misji.
    /// </summary>
    public sealed class HUDStarter : MonoBehaviour
    {
        [Header("UI refs")]
        [SerializeField] private TMP_Text missionTitle;
        [SerializeField] private TMP_Text missionDesc;
        [SerializeField] private Button generateMissionBtn;

        private void Awake()
        {
            if (generateMissionBtn) generateMissionBtn.onClick.AddListener(GenerateMission);
            EtT.Core.GameEvents.OnMissionGenerated += OnMission;
        }

        private void OnDestroy()
        {
            EtT.Core.GameEvents.OnMissionGenerated -= OnMission;
        }

        private void GenerateMission()
        {
            ServiceLocator.Get<IMissionService>().GenerateDaily();
        }

        private void OnMission(EtT.Missions.Mission m)
        {
            if (missionTitle) missionTitle.text = m.Title;
            if (missionDesc)  missionDesc.text  = m.Description;
        }
    }
}