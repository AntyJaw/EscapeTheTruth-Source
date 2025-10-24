using UnityEngine;
using UnityEngine.UI;
using EtT.Services;

namespace EtT.UI.Map.Compass
{
    /// <summary>
    /// Obraca UI-ową strzałkę tak, by wskazywała kierunek do centrum misji W.Y.R.D.
    /// Względem północy (kompasu) i pozycji gracza (GPS).
    /// Podpinamy do Image (RectTransform) z grafiką strzałki (domyślnie „w górę” = 0°).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class MissionDirectionArrow : MonoBehaviour
    {
        [Header("Źródła")]
        [SerializeField] private EtT.UI.Map.MapToARBridge bridge;

        [Header("Ustawienia")]
        [Tooltip("Dodatkowa rotacja (jeśli sprite ma inny „zerowy” kierunek).")]
        public float spriteNorthOffsetDeg = 0f;

        [Tooltip("Gładzenie rotacji strzałki.")]
        [Range(0,1)] public float rotateLerp = 0.2f;

        RectTransform _rt;
        IGpsWorldService _gps;

        void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _gps = ServiceLocator.Get<IGpsWorldService>();
        }

        void Update()
        {
            if (bridge == null || bridge.CurrentMission == null || _gps == null)
                return;

            var player = _gps.GetPlayerPosition();
            var target = bridge.CurrentMission.Center;

            float bearingToTarget = Bearing(player.Lat, player.Lng, target.Lat, target.Lng); // 0..360 względem PÓŁNOCY
            float heading = EtT.UI.Map.Compass.CompassDriver.Instance != null
                ? EtT.UI.Map.Compass.CompassDriver.Instance.HeadingDeg
                : 0f;

            // kierunek względem aktualnego patrzenia gracza (ekranu/headingu)
            float relative = bearingToTarget - heading;
            relative = Mathf.Repeat(relative, 360f);

            float desiredZ = -(relative + spriteNorthOffsetDeg); // UI rotacja Z (clockwise neg)
            float currentZ = _rt.eulerAngles.z;
            float newZ = Mathf.LerpAngle(currentZ, desiredZ, rotateLerp);
            _rt.rotation = Quaternion.Euler(0, 0, newZ);
        }

        // --- helpers ---
        static float Bearing(double lat1, double lon1, double lat2, double lon2)
        {
            double φ1 = lat1 * Mathf.Deg2Rad;
            double φ2 = lat2 * Mathf.Deg2Rad;
            double Δλ = (lon2 - lon1) * Mathf.Deg2Rad;

            double y = System.Math.Sin(Δλ) * System.Math.Cos(φ2);
            double x = System.Math.Cos(φ1) * System.Math.Sin(φ2) -
                       System.Math.Sin(φ1) * System.Math.Cos(φ2) * System.Math.Cos(Δλ);
            double θ = System.Math.Atan2(y, x);
            double brng = (θ * Mathf.Rad2Deg + 360.0) % 360.0;
            return (float)brng;
        }
    }
}