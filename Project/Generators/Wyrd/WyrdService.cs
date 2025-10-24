using UnityEngine;
using EtT.Services;
using static EtT.Generators.Wyrd.IWyrdService;

namespace EtT.Generators.Wyrd
{
    /// <summary>
    /// W.Y.R.D. – Woven Yields of Reality Distortion (Żarno Prawdy).
    /// • dystans wg mobilności/poziomu,
    /// • unika restricted,
    /// • bias: potrzeby gracza (kawiarnia/apteka), pogoda (deszcz→zadaszenie), noc (światło/publiczne),
    /// • telemetria: preferuj „dobre” okolice z archiwum sukcesów,
    /// • fallback gdy nie znajdzie.
    /// </summary>
    public sealed class WyrdService : IWyrdService
    {
        private readonly WyrdConfig _cfg;
        private readonly System.Random _rng;

        public WyrdService(WyrdConfig cfg = null)
        {
            _cfg = cfg ?? ScriptableObject.CreateInstance<WyrdConfig>();
            _rng = new System.Random();
        }

        public World.Position ChooseCrimeCenter(World.Position playerPos, MobilityKind mobility, int playerLevel)
        {
            var gps   = ServiceLocator.Get<IGpsWorldService>();
            var poi   = ServiceLocator.Get<IPoiService>();
            var time  = ServiceLocator.Get<ITimeService>();
            var wx    = ServiceLocator.Get<IWeatherService>();
            var tel   = ServiceLocator.Get<ITelemetryService>();

            // 1) zakresy dystansu wg mobilności + poziom
            var baseRange = mobility switch
            {
                MobilityKind.Walk => _cfg.walkDistanceRange,
                MobilityKind.Bike => _cfg.bikeDistanceRange,
                _                  => _cfg.carDistanceRange
            };
            float levelMul = Mathf.Clamp(_cfg.distanceByLevel.Evaluate(Mathf.Max(1, playerLevel)), 0.5f, 2.0f);
            float minM = baseRange.x * levelMul;
            float maxM = baseRange.y * levelMul;

            // 2) zbuduj „wektor” biasu kierunkowego (opcjonalny)
            World.Position? biasTarget = null;

            // 2a) bias po potrzebie gracza (energia/zdrowie)
            if (_cfg.biasByPlayerNeeds && poi != null)
            {
                var player = ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService;
                float e01 = player != null ? Mathf.Clamp01(player.Energy / 100f) : 1f;
                float h01 = player != null ? Mathf.Clamp01(player.Health / 100f) : 1f;

                if (e01 < 0.3f)
                {
                    var arr = (poi as EtT.World.Poi.PoiService)?.Nearest(playerPos, EtT.PoiKind.Cafe, 1);
                    if (arr != null && arr.Length > 0)
                        biasTarget = new World.Position(arr[0].lat, arr[0].lng);
                }
                else if (h01 < 0.4f)
                {
                    var arr = (poi as EtT.World.Poi.PoiService)?.Nearest(playerPos, EtT.PoiKind.Pharmacy, 1);
                    if (arr != null && arr.Length > 0)
                        biasTarget = new World.Position(arr[0].lat, arr[0].lng);
                }
            }

            // 2b) bias pogodowo-czasowy (deszcz → zadaszenia, noc → światło/publiczne)
            if (_cfg.biasByWeatherAndTime && (wx != null || time != null))
            {
                bool heavyRain = wx != null && wx.Rain01 >= _cfg.rainBiasThreshold;
                bool dark = wx != null ? (wx.Light01 <= _cfg.nightBiasLightThreshold) : time.IsNightLocal();

                if (heavyRain || dark)
                {
                    var svc = poi as EtT.World.Poi.PoiService;
                    EtT.World.Poi.Poi? best = null;

                    void Consider(EtT.PoiKind k)
                    {
                        if (best.HasValue) return;
                        var arr = svc?.Nearest(playerPos, k, 1);
                        if (arr != null && arr.Length > 0) best = arr[0];
                    }

                    if (dark) { Consider(EtT.PoiKind.Police); Consider(EtT.PoiKind.Base); }
                    if (heavyRain) { Consider(EtT.PoiKind.Cafe); }
                    Consider(EtT.PoiKind.Link13Station);

                    if (best.HasValue) biasTarget = new World.Position(best.Value.lat, best.Value.lng);
                }
            }

            // 3) sampling – z uwzględnieniem telemetrii i restricted
            for (int i = 0; i < Mathf.Max(8, _cfg.maxSamples); i++)
            {
                float bearing = (float)(_rng.NextDouble() * 360.0);

                if (biasTarget.HasValue && _rng.NextDouble() < _cfg.biasBearingShare)
                {
                    float toBias = Bearing(playerPos, biasTarget.Value);
                    bearing = toBias + Mathf.Lerp(-_cfg.biasBearingSpreadDeg, _cfg.biasBearingSpreadDeg, (float)_rng.NextDouble());
                }

                float t = (float)_rng.NextDouble();
                float distM = Mathf.Lerp(minM, maxM, Mathf.Pow(t, 0.65f));

                var candidate = OffsetPosition(playerPos, distM, bearing);

                if (!IsAllowed(candidate, gps)) continue;
                if (!FarFromRestricted(candidate, gps, _cfg.minDistanceFromRestricted)) continue;

                // telemetria: jeśli w promieniu są „dobre” miejsca, zwiększ akceptację próby
                if (_cfg.useTelemetry && tel != null)
                {
                    float boost = tel.SuccessBoostNear(candidate, _cfg.telemetryInfluenceRadius);
                    if (boost > 0f && _rng.NextDouble() > (_cfg.telemetryAcceptanceBoost + boost))
                        continue;
                }

                return candidate;
            }

            // 4) fallback
            return OffsetPosition(playerPos, Mathf.Max(120f, minM * 0.5f), (float)(_rng.NextDouble() * 360.0));
        }

        // ===== Helpers =====

        private static bool IsAllowed(World.Position p, IGpsWorldService gps)
        {
            if (gps == null) return true;
            return gps.IsAllowed(p);
        }

        private static bool FarFromRestricted(World.Position p, IGpsWorldService gps, float minDist)
        {
            if (gps == null) return true;
            var zones = gps.GetRestrictedZones();
            if (zones == null) return true;
            foreach (var z in zones)
            {
                float d = Haversine(p.Lat, p.Lng, z.Center.Lat, z.Center.Lng);
                if (d < z.RadiusMeters + minDist) return false;
            }
            return true;
        }

        private static World.Position OffsetPosition(World.Position origin, double meters, double bearingDeg)
        {
            double R = 6371000.0;
            double br = bearingDeg * Mathf.Deg2Rad;

            double lat1 = origin.Lat * Mathf.Deg2Rad;
            double lon1 = origin.Lng * Mathf.Deg2Rad;

            double lat2 = System.Math.Asin(System.Math.Sin(lat1) * System.Math.Cos(meters / R) +
                                           System.Math.Cos(lat1) * System.Math.Sin(meters / R) * System.Math.Cos(br));
            double lon2 = lon1 + System.Math.Atan2(System.Math.Sin(br) * System.Math.Sin(meters / R) * System.Math.Cos(lat1),
                                                   System.Math.Cos(meters / R) - System.Math.Sin(lat1) * System.Math.Sin(lat2));

            return new World.Position(lat2 * Mathf.Rad2Deg, lon2 * Mathf.Rad2Deg);
        }

        private static float Haversine(double lat1,double lon1,double lat2,double lon2)
        {
            const double R=6371000.0;
            double dLat=(lat2-lat1)*Mathf.Deg2Rad, dLon=(lon2-lon1)*Mathf.Deg2Rad;
            double a=System.Math.Sin(dLat/2)*System.Math.Sin(dLat/2)+System.Math.Cos(lat1*Mathf.Deg2Rad)*System.Math.Cos(lat2*Mathf.Deg2Rad)*System.Math.Sin(dLon/2)*System.Math.Sin(dLon/2);
            double c=2*System.Math.Atan2(System.Math.Sqrt(a),System.Math.Sqrt(1-a));
            return (float)(R*c);
        }

        private static float Bearing(World.Position from, World.Position to)
        {
            double lat1 = from.Lat * Mathf.Deg2Rad, lat2 = to.Lat * Mathf.Deg2Rad;
            double dLon = (to.Lng - from.Lng) * Mathf.Deg2Rad;
            double y = System.Math.Sin(dLon) * System.Math.Cos(lat2);
            double x = System.Math.Cos(lat1) * System.Math.Sin(lat2) - System.Math.Sin(lat1) * System.Math.Cos(lat2) * System.Math.Cos(dLon);
            return (float)((System.Math.Atan2(y, x) * Mathf.Rad2Deg + 360.0) % 360.0);
        }
    }
}