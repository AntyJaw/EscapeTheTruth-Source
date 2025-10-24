using UnityEngine;
using EtT.Services;
using EtT.Core;

namespace EtT.UI.Map
{
    /// <summary>
    /// Spina logikę mapy z trybem AR:
    /// - trzyma referencję do aktualnej/aktywnej misji,
    /// - liczy dystans gracza do centrum W.Y.R.D.,
    /// - wylicza, czy można wejść do AR (w promieniu misji),
    /// - publikuje prosty status do UI.
    /// </summary>
    public sealed class MapToARBridge : MonoBehaviour
    {
        [Header("Ustawienia")]
        [Tooltip("Margin pozwalający wejść do AR ciut poza promieniem (np. drift GPS).")]
        [Range(0f, 0.5f)] public float radiusSlack = 0.15f; // 15%

        [Header("Runtime (read-only)")]
        [SerializeField] private EtT.Missions.Mission currentMission;
        [SerializeField] private float distanceMeters;
        [SerializeField] private bool insideArea;

        private IGpsWorldService _gps;
        private IARMissionService _ar;
        private IMissionService _missions;

        public EtT.Missions.Mission CurrentMission => currentMission;
        public float DistanceMeters => distanceMeters;
        public bool IsInsideArea => insideArea;
        public bool CanEnterAR => currentMission != null && insideArea && !_ar.IsInAR;

        void Awake()
        {
            _gps = ServiceLocator.Get<IGpsWorldService>();
            _ar  = ServiceLocator.Get<IARMissionService>();
            _missions = ServiceLocator.Get<IMissionService>();

            GameEvents.OnMissionGenerated      += OnMissionGenerated;
            GameEvents.OnMissionStatusChanged  += OnMissionStatusChanged;

            // jeśli już mamy jakieś aktywne – wybierz ostatnią Pending/Active
            var ms = _missions as EtT.Missions.MissionService;
            if (ms != null && ms.Active.Count > 0)
            {
                for (int i = ms.Active.Count - 1; i >= 0; --i)
                {
                    var m = ms.Active[i];
                    if (m.Status == EtT.Missions.MissionStatus.Pending || m.Status == EtT.Missions.MissionStatus.Active)
                    {
                        currentMission = m;
                        break;
                    }
                }
            }
        }

        void OnDestroy()
        {
            GameEvents.OnMissionGenerated     -= OnMissionGenerated;
            GameEvents.OnMissionStatusChanged -= OnMissionStatusChanged;
        }

        void Update()
        {
            if (currentMission == null || _gps == null)
            {
                distanceMeters = -1f;
                insideArea = false;
                return;
            }

            var p = _gps.GetPlayerPosition();
            distanceMeters = Haversine(p.Lat, p.Lng, currentMission.Center.Lat, currentMission.Center.Lng);

            float slack = Mathf.Max(0f, currentMission.RadiusMeters * radiusSlack);
            insideArea = distanceMeters <= (currentMission.RadiusMeters + slack);
        }

        public void EnterAR()
        {
            if (!CanEnterAR) return;

            // Upewnij się, że misja ma status Active (start, jeśli Pending)
            _missions.StartMission(currentMission);

            // Odpal AR dla tej misji
            _ar.EnterARScene(currentMission);
        }

        public void ClearSelection()
        {
            currentMission = null;
        }

        public void SetMission(EtT.Missions.Mission m)
        {
            currentMission = m;
        }

        private void OnMissionGenerated(EtT.Missions.Mission m)
        {
            // Auto-wybór najnowszej wygenerowanej misji
            currentMission = m;
        }

        private void OnMissionStatusChanged(EtT.Missions.Mission m, EtT.Missions.MissionStatus s)
        {
            if (currentMission == null) return;
            if (m.Id != currentMission.Id) return;

            if (s == EtT.Missions.MissionStatus.Archived)
                currentMission = null; // misja skończona → odpinamy
        }

        // --- helpers ---
        private static float Haversine(double lat1,double lon1,double lat2,double lon2)
        {
            const double R=6371000.0;
            double dLat=(lat2-lat1)*Mathf.Deg2Rad, dLon=(lon2-lon1)*Mathf.Deg2Rad;
            double a=System.Math.Sin(dLat/2)*System.Math.Sin(dLat/2)
                    + System.Math.Cos(lat1*Mathf.Deg2Rad)*System.Math.Cos(lat2*Mathf.Deg2Rad)
                    * System.Math.Sin(dLon/2)*System.Math.Sin(dLon/2);
            double c=2*System.Math.Atan2(System.Math.Sqrt(a),System.Math.Sqrt(1-a));
            return (float)(R*c);
        }
    }
}