using UnityEngine;
using EtT.Core;
using EtT.Map;
using EtT.Location;
using EtT.Services;
using EtT.World;
using EtT.Weather;

namespace EtT.Systems
{
    // Łączy: GPS -> NavigationService -> MapView; wysyła eventy geofencingu i triggeruje pogodę po wejściu w strefę.
    public sealed class GeoMissionController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private SlippyTileMapView mapView;          // IMapView
        [SerializeField] private LocationServiceWrapper gps;          // GPS
        [SerializeField] private OpenWeatherMapService weather;       // OWM (z API key)

        [Header("Mission Area")]
        [SerializeField] private float defaultRadiusMeters = 150f;

        private INavigationService _nav = new MissionNavigationService();
        private bool _wasInside;

        private void OnEnable()
        {
            GameEvents.OnMissionGenerated += OnMissionGenerated;
        }
        private void OnDisable()
        {
            GameEvents.OnMissionGenerated -= OnMissionGenerated;
        }

        private void Start()
        {
            if (gps) gps.OnLocationUpdated += OnLocation;
        }

        private void OnMissionGenerated(Missions.Mission m)
        {
            // W tej wersji nie mamy jeszcze lokacji w Mission -> demo: przesuwamy w okolice Warszawy.
            var target = new Position(52.2305, 21.0050);
            _nav.SetMissionTarget(target, defaultRadiusMeters);
            if (mapView) mapView.SetMissionArea(target, defaultRadiusMeters);

            if (gps && !gps.IsRunning) gps.StartLocation();
            if (mapView) mapView.Show();
        }

        private void OnLocation(Position p)
        {
            _nav.UpdatePlayerPos(p);
            if (mapView) mapView.SetPlayer(p);

            bool inside = _nav.IsInsideMissionArea;
            if (inside != _wasInside)
            {
                _wasInside = inside;
                var zone = new ZoneInfo { Name = "MissionArea", Center = p, RadiusMeters = defaultRadiusMeters };
                GameEvents.RaiseZoneGate(zone, inside);

                if (inside && weather != null)
                {
                    // Pobierz pogodę z OWM i wyślij OnWeatherChanged
                    weather.Refresh(p.Lat, p.Lng);
                }
            }
        }

        public void OpenExternalDirections(bool preferApple)
        {
            var url = _nav.BuildExternalMapsUrl(preferApple);
            Application.OpenURL(url);
        }
    }
}