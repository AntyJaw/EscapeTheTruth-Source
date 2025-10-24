using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EtT.Services;

namespace EtT.UI.Placeholder
{
    /// <summary>
    /// Prosty HUD: przycisk start/stop, crosshair, tooltip.
    /// </summary>
    public sealed class ARMissionHUD : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private GameObject root;      // cały HUD
        [SerializeField] private GameObject crosshair; // obrazek na środku
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Button startBtn;
        [SerializeField] private Button stopBtn;

        private void Awake()
        {
            startBtn.onClick.AddListener(()=> ServiceLocator.Get<EtT.IARMissionService>().StartMission());
            stopBtn.onClick.AddListener(()=> ServiceLocator.Get<EtT.IARMissionService>().StopMission());
        }

        private void OnEnable()
        {
            ServiceLocator.Get<EtT.IARMissionService>().OnStateChanged += OnState;
            OnState(ServiceLocator.Get<EtT.IARMissionService>().IsActive);
        }

        private void OnDisable()
        {
            ServiceLocator.Get<EtT.IARMissionService>().OnStateChanged -= OnState;
        }

        private void OnState(bool active)
        {
            if (root) root.SetActive(true);
            if (crosshair) crosshair.SetActive(active);
            if (hintText) hintText.text = active ? "Dotknij śladu, aby zeskanować" : "Naciśnij START, aby rozpocząć misję AR";
            startBtn.interactable = !active;
            stopBtn.interactable  = active;
        }
    }
}