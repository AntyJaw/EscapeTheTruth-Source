using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EtT.Core;
using EtT.Services;

namespace EtT.UI.Placeholder
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private Slider sanityBar;
        [SerializeField] private Slider reputationBar;
        [SerializeField] private TMP_Text missionTitle;
        [SerializeField] private TMP_Text missionDesc;
        [SerializeField] private TMP_Text timerLabel; // dodaj w Inspectorze

        private Missions.Mission _last;

        private void OnEnable()
        {
            GameEvents.OnSanityChanged += OnSanity;
            GameEvents.OnReputationChanged += OnReputation;
            GameEvents.OnMissionGenerated += OnMission;
        }
        private void OnDisable()
        {
            GameEvents.OnSanityChanged -= OnSanity;
            GameEvents.OnReputationChanged -= OnReputation;
            GameEvents.OnMissionGenerated -= OnMission;
        }

        private void Update()
        {
            ServiceLocator.Get<IMissionService>().TickTimers(Time.deltaTime);
            if (_last != null && timerLabel)
                timerLabel.text = $"{Mathf.CeilToInt(_last.TimeLeft)}s";
        }

        public void GenerateDaily()
        {
            _last = ServiceLocator.Get<IMissionService>().GenerateDaily();
            if (_last != null) OnMission(_last);
        }

        private void OnSanity(float v)     { if (sanityBar) sanityBar.value = v/100f; }
        private void OnReputation(float v) { if (reputationBar) reputationBar.value = v/100f; }
        private void OnMission(Missions.Mission m)
        {
            _last = m;
            if (missionTitle) missionTitle.text = m.Title;
            if (missionDesc)  missionDesc.text  = m.Description;
        }
    }
}