using UnityEngine;
using TMPro;

namespace EtT.UI.Map.Compass
{
    /// <summary>
    /// Pokazuje dystans do centrum misji oraz szacowany czas dotarcia (ETA)
    /// dla trybu „pieszo” (regulowany parametrem speedMps).
    /// Podłącz do TMP_Text i wskaż MapToARBridge.
    /// </summary>
    public sealed class MissionDistanceLabel : MonoBehaviour
    {
        [SerializeField] private EtT.UI.Map.MapToARBridge bridge;
        [SerializeField] private TMP_Text label;
        [Header("Prędkość (m/s)")]
        [Range(0.5f, 5f)] public float speedMps = 1.4f; // ok. 5 km/h

        void Update()
        {
            if (label == null) return;
            if (bridge == null || bridge.CurrentMission == null)
            {
                label.text = "—";
                return;
            }

            float d = bridge.DistanceMeters;
            if (d < 0) { label.text = "—"; return; }

            string dist = d >= 1000f ? $"{d/1000f:0.0} km" : $"{d:0} m";

            float etaSec = Mathf.Max(0f, d / Mathf.Max(0.1f, speedMps));
            string eta = FormatEta(etaSec);

            if (bridge.IsInsideArea)
                label.text = $"W strefie (r={bridge.CurrentMission.RadiusMeters:0} m)";
            else
                label.text = $"{dist} • ETA {eta}";
        }

        string FormatEta(float sec)
        {
            int m = Mathf.FloorToInt(sec / 60f);
            int s = Mathf.FloorToInt(sec % 60f);
            if (m <= 0) return $"{s}s";
            return $"{m}m {s}s";
        }
    }
}