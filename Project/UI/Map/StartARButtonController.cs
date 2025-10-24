using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EtT.UI.Map
{
    /// <summary>
    /// Steruje przyciskiem „Wejdź do AR”:
    /// - odświeża stan (interactable + komunikat),
    /// - wywołuje MapToARBridge.EnterAR() po kliknięciu.
    /// </summary>
    public sealed class StartARButtonController : MonoBehaviour
    {
        [SerializeField] private MapToARBridge bridge;
        [SerializeField] private Button startArButton;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text sublabel;

        [Header("Teksty")]
        [SerializeField] private string readyText = "Wejdź do AR";
        [SerializeField] private string tooFarText = "Podejdź bliżej";
        [SerializeField] private string noMissionText = "Brak aktywnej misji";

        void Awake()
        {
            if (startArButton) startArButton.onClick.AddListener(OnClick);
        }

        void Update()
        {
            if (bridge == null || bridge.CurrentMission == null)
            {
                SetState(false, noMissionText, "");
                return;
            }

            if (bridge.CanEnterAR)
            {
                SetState(true, readyText, $"W promieniu misji (≤ {bridge.CurrentMission.RadiusMeters:0} m)");
            }
            else
            {
                // pokaż dystans i ile brakuje
                float dist = bridge.DistanceMeters;
                float need = Mathf.Max(0f, dist - bridge.CurrentMission.RadiusMeters);
                SetState(false, tooFarText, $"Dystans: {dist:0} m (brakuje ~{need:0} m)");
            }
        }

        private void OnClick()
        {
            if (bridge != null) bridge.EnterAR();
        }

        private void SetState(bool interactable, string main, string sub)
        {
            if (startArButton) startArButton.interactable = interactable;
            if (label) label.text = main;
            if (sublabel) sublabel.text = sub;
        }
    }
}