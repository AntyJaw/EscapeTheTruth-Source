using System.Collections;
using UnityEngine;
using EtT.Core;

namespace EtT.World
{
    /// <summary>
    /// Realny GPS na urządzeniu + symulacja w Edytorze.
    /// </summary>
    public sealed class GpsWorldServiceUnity : IGpsWorldService
    {
        private bool _running;
        private Position _lastPos = new Position(52.229676, 21.012229); // centrum Wawy na start
        private double _accuracy = 9999;
        private readonly ZoneInfo[] _restricted;

#if UNITY_EDITOR
        private bool _simulate = true;
#endif

        public GpsWorldServiceUnity()
        {
            _restricted = new[]
            {
                new ZoneInfo{ Name="Szpital – strefa cicha", Center=new Position(52.230, 21.010), RadiusMeters=150f },
            };

            // próba odczytu startowego z PlayerPrefs (opcjonalnie)
            if (PlayerPrefs.HasKey("gps_lat") && PlayerPrefs.HasKey("gps_lng"))
            {
                _lastPos = new Position(PlayerPrefs.GetFloat("gps_lat"), PlayerPrefs.GetFloat("gps_lng"));
                _accuracy = PlayerPrefs.GetFloat("gps_acc", 50f);
            }
        }

        public void Start()
        {
            if (_running) return;
            _running = true;

#if UNITY_ANDROID || UNITY_IOS
            CoroutineRunner.Run(StartLocationRoutine());
#else
            // w Edytorze nic nie robimy – zostaje symulacja
#endif
        }

        public void Stop()
        {
            _running = false;
#if UNITY_ANDROID || UNITY_IOS
            Input.location.Stop();
#endif
        }

        public Position GetPlayerPosition() => _lastPos;
        public double AccuracyMeters => _accuracy;
        public bool IsAvailable
        {
            get
            {
#if UNITY_ANDROID || UNITY_IOS
                return Input.location.status == LocationServiceStatus.Running;
#else
                return true; // Edytor: zawsze dostępny (symulacja)
#endif
            }
        }

        public ZoneInfo[] GetRestrictedZones() => _restricted;

        public bool IsAllowed(Position p)
        {
            foreach (var z in _restricted)
            {
                var d = Haversine(p.Lat, p.Lng, z.Center.Lat, z.Center.Lng);
                if (d <= z.RadiusMeters) return false;
            }
            return true;
        }

#if UNITY_EDITOR
        public void SetSimulatedPosition(double lat, double lng, double accuracyMeters = 5.0)
        {
            _lastPos = new Position(lat, lng);
            _accuracy = accuracyMeters;
            PlayerPrefs.SetFloat("gps_lat", (float)lat);
            PlayerPrefs.SetFloat("gps_lng", (float)lng);
            PlayerPrefs.SetFloat("gps_acc", (float)accuracyMeters);
        }
#endif

        // === Routines ===
#if UNITY_ANDROID || UNITY_IOS
        private IEnumerator StartLocationRoutine()
        {
            if (!Input.location.isEnabledByUser)
            {
                GameEvents.RaiseLink13("[GPS] Usługi lokalizacji wyłączone.");
                yield break;
            }

            Input.location.Start(5f, 1f); // accuracyMeters, distanceFilterMeters

            // czekaj na init max 10 sek
            int maxWait = 10;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }
            if (maxWait <= 0)
            {
                GameEvents.RaiseLink13("[GPS] Timeout inicjalizacji.");
                yield break;
            }
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                GameEvents.RaiseLink13("[GPS] Nie można odczytać lokalizacji.");
                yield break;
            }

            GameEvents.RaiseLink13("[GPS] Start OK.");

            while (_running)
            {
                var data = Input.location.lastData;
                _lastPos = new Position(data.latitude, data.longitude);
                _accuracy = data.horizontalAccuracy;

                PlayerPrefs.SetFloat("gps_lat", (float)data.latitude);
                PlayerPrefs.SetFloat("gps_lng", (float)data.longitude);
                PlayerPrefs.SetFloat("gps_acc", (float)data.horizontalAccuracy);

                yield return null; // co frame; możesz dodać throttling
            }
        }
#endif

        private static float Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371000.0;
            double dLat = Mathf.Deg2Rad * (lat2 - lat1);
            double dLon = Mathf.Deg2Rad * (lon2 - lon1);
            double a =
                Mathf.Sin((float)dLat/2) * Mathf.Sin((float)dLat/2) +
                Mathf.Cos(Mathf.Deg2Rad*(float)lat1) * Mathf.Cos(Mathf.Deg2Rad*(float)lat2) *
                Mathf.Sin((float)dLon/2) * Mathf.Sin((float)dLon/2);
            double c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1-a));
            return (float)(R * c);
        }

        // Mały helper do uruchamiania korutyn spoza MonoBehaviour
        private static class CoroutineRunner
        {
            private class Runner : MonoBehaviour { }
            private static Runner _runner;
            public static void Run(IEnumerator routine)
            {
                if (_runner == null)
                {
                    var go = new GameObject("[GpsCoroutineRunner]");
                    Object.DontDestroyOnLoad(go);
                    _runner = go.AddComponent<Runner>();
                }
                _runner.StartCoroutine(routine);
            }
        }
    }
}