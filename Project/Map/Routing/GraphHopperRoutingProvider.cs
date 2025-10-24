using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using EtT.Services;

namespace EtT.Map.Routing
{
    public sealed class GraphHopperRoutingProvider : IRoutingProvider
    {
        private readonly GraphHopperConfig _cfg;
        private readonly MonoRunner _runner;

        public GraphHopperRoutingProvider(GraphHopperConfig cfg)
        {
            _cfg = cfg ?? ScriptableObject.CreateInstance<GraphHopperConfig>();
            _runner = MonoRunner.GetOrCreate();
        }

        public void RequestRoute(World.Position from, World.Position to, RouteMode mode,
                                 System.Action<RouteResult> onSuccess,
                                 System.Action<string> onError)
        {
            // Brak klucza lub pusty endpoint -> fallback
            if (string.IsNullOrWhiteSpace(_cfg.apiBase) || string.IsNullOrWhiteSpace(_cfg.apiKey))
            {
                onSuccess?.Invoke(FallbackStraightLine(from, to, mode));
                return;
            }

            _runner.StartCoroutine(CoRequest(from, to, mode, onSuccess, onError));
        }

        private IEnumerator CoRequest(World.Position from, World.Position to, RouteMode mode,
                                      System.Action<RouteResult> onSuccess, System.Action<string> onError)
        {
            string profile = mode switch
            {
                RouteMode.Foot => "foot",
                RouteMode.Bike => "bike",
                _ => "car"
            };

            // GraphHopper: points=from|to; profile; locale=; points_encoded=false żeby dostać surowe punkty
            // Doc: https://graphhopper.com/api/1/docs/
            var url = $"{_cfg.apiBase}?point={from.Lat}%2C{from.Lng}&point={to.Lat}%2C{to.Lng}&profile={profile}&locale=en&points_encoded=false&key={_cfg.apiKey}";
            using var req = UnityWebRequest.Get(url);
            req.timeout = _cfg.timeoutSec;

            yield return req.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                onError?.Invoke($"HTTP: {req.error}");
                yield break;
            }

            var json = req.downloadHandler.text;
            try
            {
                var parsed = JsonUtility.FromJson<GHResponseWrapper>(GHJson.Fix(json));
                if (parsed == null || parsed.paths == null || parsed.paths.Length == 0)
                {
                    onError?.Invoke("Empty result");
                    yield break;
                }

                var path = parsed.paths[0];
                var pts = new List<World.Position>();
                foreach (var c in path.points.coordinates)
                {
                    if (c.Length >= 2) pts.Add(new World.Position(c[1], c[0])); // GH -> [lon, lat]
                    if (pts.Count >= _cfg.maxPolylinePoints) break;
                }

                var res = new RouteResult
                {
                    points = pts,
                    distanceMeters = path.distance,
                    durationSeconds = path.time / 1000f,
                    mode = profile,
                    source = "graphhopper"
                };
                onSuccess?.Invoke(res);
            }
            catch (System.SystemException ex)
            {
                onError?.Invoke($"Parse error: {ex.Message}");
            }
        }

        private static RouteResult FallbackStraightLine(World.Position from, World.Position to, RouteMode mode)
        {
            var list = new List<World.Position> { from, to };
            float dist = Haversine(from.Lat, from.Lng, to.Lat, to.Lng);
            // proste tempo: Foot 5km/h, Bike 15km/h, Car 40km/h (MVP)
            float speed = mode switch { RouteMode.Foot => 5f, RouteMode.Bike => 15f, _ => 40f }; // km/h
            float sec = (dist/1000f) / speed * 3600f;

            return new RouteResult
            {
                points = list,
                distanceMeters = dist,
                durationSeconds = sec,
                mode = mode.ToString().ToLowerInvariant(),
                source = "fallback"
            };
        }

        private static float Haversine(double lat1,double lon1,double lat2,double lon2)
        {
            const double R=6371000.0;
            double dLat=(lat2-lat1)*Mathf.Deg2Rad, dLon=(lon2-lon1)*Mathf.Deg2Rad;
            double a=System.Math.Sin(dLat/2)*System.Math.Sin(dLat/2)+System.Math.Cos(lat1*Mathf.Deg2Rad)*System.Math.Cos(lat2*Mathf.Deg2Rad)*System.Math.Sin(dLon/2)*System.Math.Sin(dLon/2);
            double c=2*System.Math.Atan2(System.Math.Sqrt(a),System.Math.Sqrt(1-a));
            return (float)(R*c);
        }

        // ===== Helpers: prosty runner i JSON modele =====
        private class MonoRunner : MonoBehaviour
        {
            private static MonoRunner _inst;
            public static MonoRunner GetOrCreate()
            {
                if (_inst) return _inst;
                var go = new GameObject("[GraphHopperRunner]");
                DontDestroyOnLoad(go);
                _inst = go.AddComponent<MonoRunner>();
                return _inst;
            }
        }

        // JSON wrappers (minimal)
        [System.Serializable] private class GHResponseWrapper { public GHPath[] paths; }
        [System.Serializable] private class GHPath { public float distance; public float time; public GHPoints points; }
        [System.Serializable] private class GHPoints { public float[][][] coordinates; } // GeoJSON LineString
        private static class GHJson
        {
            // GraphHopper zwraca „type, coordinates” w obiekcie geometry – JsonUtility jest ograniczony,
            // więc robimy szybki wrapper/corrector.
            public static string Fix(string raw)
            {
                // zamieniamy "points":{"coordinates":[[lon,lat],...]} do klasy GHPoints
                // Unity JsonUtility wymaga „dokładnie” dopasowanych pól — GH ma dodatkowe pola, ale to mu nie przeszkadza.
                return raw.Replace("\"type\":\"LineString\",", ""); // kosmetyka pod JsonUtility
            }
        }
    }
}