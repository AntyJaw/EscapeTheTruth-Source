using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EtT.UI.Map
{
    /// <summary>
    /// Pasek postępu dotarcia do strefy misji W.Y.R.D. (0..1) + opis dystansu.
    /// Użyteczne na mapie, by pokazać graczowi jak blisko jest wejścia do AR.
    /// </summary>
    public sealed class MissionProximityWidget : MonoBehaviour
    {
        [SerializeField] private MapToARBridge bridge;
        [SerializeField] private Slider progress;
        [SerializeField] private TMP_Text distanceText;

        [Header("Zakresy paska (m)")]
        [SerializeField] private float minShown = 50f;   // poniżej „wpada” na 1
        [SerializeField] private float maxShown = 1200f; // powyżej 0

        void Update()
        {
            if (bridge == null || bridge.CurrentMission == null)
            {
                if (progress) progress.value = 0f;
                if (distanceText) distanceText.text = "-";
                return;
            }

            float d = bridge.DistanceMeters;
            if (d < 0f)
            {
                if (progress) progress.value = 0f;
                if (distanceText) distanceText.text = "—";
                return;
            }

            // Normalizacja odwrotna: blisko = 1, daleko = 0
            float t = Mathf.InverseLerp(maxShown, minShown, d);
            t = Mathf.Clamp01(t);

            if (progress) progress.value = t;

            float need = Mathf.Max(0f, d - bridge.CurrentMission.RadiusMeters);
            if (distanceText)
                distanceText.text = d <= bridge.CurrentMission.RadiusMeters
                    ? $"W strefie (r={bridge.CurrentMission.RadiusMeters:0} m)"
                    : $"Do strefy: {need:0} m";
        }
    }
}