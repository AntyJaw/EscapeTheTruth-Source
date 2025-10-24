using System.Collections.Generic;
using UnityEngine;
using EtT.Services;

namespace EtT
{
    public interface ITelemetryService
    {
        void RecordMissionOutcome(Missions.Mission m);
        /// <summary>Zwraca boost 0..1, jeśli w pobliżu kandydata były udane sprawy.</summary>
        float SuccessBoostNear(World.Position candidate, float radiusMeters);
    }

    [System.Serializable]
    public sealed class CrimeSpot
    {
        public double lat;
        public double lng;
        public int success;   // +1 ukończona, 0 porzucona
        public int timestamp; // unix utc
    }

    /// <summary>
    /// Telemetria miejsc zbrodni dla W.Y.R.D.:
    /// - zapisuje punkty zakończonych spraw,
    /// - oblicza lokalny "boost" dla generatora W.Y.R.D.,
    /// - MIGRACJA: przenosi stare dane z telemetry_crimespots → telemetry_wyrd_crimespots.
    /// </summary>
    public sealed class TelemetryService : ITelemetryService
    {
        private const string SAVE_KEY_NEW = "telemetry_wyrd_crimespots";
        private const string SAVE_KEY_OLD = "telemetry_crimespots";

        private readonly List<CrimeSpot> _spots = new();

        public TelemetryService(){ Load(); }

        public void RecordMissionOutcome(Missions.Mission m)
        {
            if (m == null || m.LastReport == null) return;

            _spots.Add(new CrimeSpot{
                lat = m.Center.Lat, lng = m.Center.Lng,
                success = (m.LastReport.FinalStatus == Missions.MissionStatus.Completed) ? 1 : 0,
                timestamp = m.LastReport.FinishedUnixUtc
            });

            Persist();
            Debug.Log($"[W.Y.R.D][Telemetry] Recorded spot lat={m.Center.Lat:0.000000}, lng={m.Center.Lng:0.000000}, success={(m.LastReport.FinalStatus==Missions.MissionStatus.Completed ? 1:0)}");
        }

        public float SuccessBoostNear(World.Position candidate, float radiusMeters)
        {
            if (_spots.Count == 0) return 0f;
            int good = 0, total = 0;
            foreach (var s in _spots)
            {
                float d = Haversine(candidate.Lat, candidate.Lng, s.lat, s.lng);
                if (d <= radiusMeters)
                {
                    total++;
                    if (s.success > 0) good++;
                }
            }
            if (total == 0) return 0f;
            float boost = Mathf.Clamp01((float)good / total);
            // Debug (opcjonalne – odkomentuj do QA):
            // Debug.Log($"[W.Y.R.D][Telemetry] boost={boost:0.00} near lat={candidate.Lat:0.000000}, lng={candidate.Lng:0.000000} (r={radiusMeters}m)");
            return boost;
        }

        private void Persist()
        {
            ServiceLocator.Get<ISaveService>().Store(SAVE_KEY_NEW, _spots);
        }

        private void Load()
        {
            var save = ServiceLocator.Get<ISaveService>();

            // Najpierw nowe
            var loadedNew = save.Load<List<CrimeSpot>>(SAVE_KEY_NEW);
            if (loadedNew != null && loadedNew.Count > 0)
                _spots.AddRange(loadedNew);

            // Migracja: dołącz stare (jeśli istnieją), a potem zapisuj już pod nowym kluczem
            var loadedOld = save.Load<List<CrimeSpot>>(SAVE_KEY_OLD);
            if (loadedOld != null && loadedOld.Count > 0)
            {
                _spots.AddRange(loadedOld);
                Persist(); // zapis pod nowym kluczem
                Debug.Log($"[W.Y.R.D][Telemetry] Migrated {loadedOld.Count} spot(s) from legacy key.");
            }
        }

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