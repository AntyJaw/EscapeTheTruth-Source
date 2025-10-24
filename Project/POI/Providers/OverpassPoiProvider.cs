using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using MiniJSON;
using EtT.Map;

namespace EtT.POI
{
    /// <summary>
    /// Provider POI z OSM (Overpass). Pobiera amenity: cafe/pharmacy/bench/convenience/supermarket/bar/publ/fast_food/vending_machine.
    /// Ma cache JSON (TTL godzin).
    /// </summary>
    public class OverpassPoiProvider : MonoBehaviour, IPoiProvider
    {
        [Header("Overpass")]
        [Tooltip("Endpoint Overpass API")]
        public string endpoint = "https://overpass-api.de/api/interpreter";
        [Tooltip("Maks. liczba elementów do przyjęcia")]
        public int hardLimit = 500;

        [Header("Cache")]
        [Tooltip("Godziny ważności cache’u")] public int cacheTtlHours = 24;

        // Mapowanie typów gry -> amenity OSM
        private static readonly Dictionary<PoiType, string[]> Map = new()
        {
            { PoiType.Cafe,        new[]{ "cafe","bar","pub","fast_food" } },
            { PoiType.Pharmacy,    new[]{ "pharmacy" } },
            { PoiType.Bench,       new[]{ "bench" } },
            { PoiType.Store,       new[]{ "convenience","supermarket","vending_machine" } },
            // inne można dodać później (police/base itd. nie zawsze dostępne publicznie)
        };

        public void Scan(double lat, double lng, int radiusMeters, PoiType[] types, Action<List<PoiInstance>, string> onDone)
        {
            StartCoroutine(CoScan(lat, lng, radiusMeters, types, onDone));
        }

        private System.Collections.IEnumerator CoScan(double lat, double lng, int radiusMeters, PoiType[] types, Action<List<PoiInstance>, string> onDone)
        {
            // 1) Skonstruuj zapytanie
            var amenities = TypesToAmenities(types);
            if (amenities.Length == 0) { onDone?.Invoke(new List<PoiInstance>(), null); yield break; }

            var key = CacheKey(lat, lng, radiusMeters, amenities);
            // 2) Cache?
            if (TryReadCache(key, out var cached))
            {
                onDone?.Invoke(cached, null);
                yield break;
            }

            var q = BuildQuery(lat, lng, radiusMeters, amenities);
            var form = new WWWForm();
            form.AddField("data", q);

            using var req = UnityWebRequest.Post(endpoint, form);
            req.timeout = 25;
            yield return req.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                onDone?.Invoke(new List<PoiInstance>(), "net:" + req.error);
                yield break;
            }

            var text = req.downloadHandler.text;
            var list = ParseOverpassJson(text, out string parseError);
            if (parseError != null)
            {
                onDone?.Invoke(new List<PoiInstance>(), parseError);
                yield break;
            }

            // dopasuj do typów – mapowanie amenity->PoiType
            var typed = new List<PoiInstance>(list.Count);
            foreach (var e in list)
            {
                if (e.type == null) continue;
                if (!AmenityToPoiType(e.amenity, out var poiType)) continue;
                if (!types.Contains(poiType)) continue;

                var display = string.IsNullOrEmpty(e.name) ? e.amenity : e.name;
                typed.Add(new PoiInstance{
                    id = $"osm_{e.id}",
                    name = display,
                    type = poiType,
                    lat = e.lat,
                    lng = e.lon,
                    radiusMeters = DefaultRadiusFor(poiType),
                    layers = PoiLayer.GPS,
                    req = default,
                    effect = default
                });
                if (typed.Count >= hardLimit) break;
            }

            WriteCache(key, typed);
            onDone?.Invoke(typed, null);
        }

        private static float DefaultRadiusFor(PoiType t)
        {
            return t switch
            {
                PoiType.Cafe => 20f,
                PoiType.Pharmacy => 20f,
                PoiType.Bench => 8f,
                PoiType.Store => 15f,
                _ => 20f
            };
        }

        private static string[] TypesToAmenities(PoiType[] types)
        {
            var set = new HashSet<string>();
            foreach (var t in types)
                if (Map.TryGetValue(t, out var arr))
                    foreach (var a in arr) set.Add(a);
            return new List<string>(set).ToArray();
        }

        private static bool AmenityToPoiType(string amenity, out PoiType t)
        {
            foreach (var kv in Map)
                if (kv.Value.Contains(amenity)) { t = kv.Key; return true; }
            t = PoiType.Store; return false;
        }

        private static string BuildQuery(double lat, double lng, int radius, string[] amenities)
        {
            // Around + amenity regexp
            var regex = string.Join("|", amenities);
            var sb = new StringBuilder();
            sb.Append("[out:json][timeout:25];(");
            sb.Append($"node(around:{radius},{lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lng.ToString(System.Globalization.CultureInfo.InvariantCulture)})[\"amenity\"~\"{regex}\"];");
            sb.Append(");out center tags;");
            return sb.ToString();
        }

        // ====== Cache ======
        private static string CacheDir => Path.Combine(Application.persistentDataPath, "poi_cache");
        private string CachePath(string key) => Path.Combine(CacheDir, key + ".json");
        private string CacheKey(double lat,double lng,int radius,string[] amenities)
        {
            string s = $"{Math.Round(lat,4)}_{Math.Round(lng,4)}_{radius}_{string.Join("-",amenities.OrderBy(x=>x))}";
            return ToSafe(s);
        }
        private static string ToSafe(string s)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }

        private bool TryReadCache(string key, out List<PoiInstance> list)
        {
            list = null;
            try
            {
                var path = CachePath(key);
                if (!File.Exists(path)) return false;
                var fi = new FileInfo(path);
                if (DateTime.UtcNow - fi.LastWriteTimeUtc > TimeSpan.FromHours(cacheTtlHours)) return false;
                var txt = File.ReadAllText(path, Encoding.UTF8);
                var data = Json.Deserialize(txt) as List<object>;
                list = DeserializePoiInstances(data);
                return true;
            }
            catch { return false; }
        }

        private void WriteCache(string key, List<PoiInstance> list)
        {
            try
            {
                if (!Directory.Exists(CacheDir)) Directory.CreateDirectory(CacheDir);
                var path = CachePath(key);
                var data = SerializePoiInstances(list);
                var json = ToJsonArray(data);
                File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch { /* ignore */ }
        }

        // Prościutka serializacja do listy słowników
        private static List<object> SerializePoiInstances(List<PoiInstance> list)
        {
            var r = new List<object>(list.Count);
            foreach (var p in list)
            {
                r.Add(new Dictionary<string, object>{
                    {"id", p.id}, {"name", p.name}, {"type", p.type.ToString()},
                    {"lat", p.lat}, {"lng", p.lng}, {"radius", p.radiusMeters}
                });
            }
            return r;
        }
        private static List<PoiInstance> DeserializePoiInstances(List<object> arr)
        {
            var r = new List<PoiInstance>();
            if (arr == null) return r;
            foreach (var o in arr)
            {
                var d = o as Dictionary<string, object>;
                if (d == null) continue;
                var p = new PoiInstance{
                    id = d.TryGetValue("id", out var id) ? (string)id : Guid.NewGuid().ToString("N"),
                    name = d.TryGetValue("name", out var nm) ? (string)nm : "POI",
                    type = EnumTry<PoiType>(d, "type", PoiType.Store),
                    lat = ToDouble(d, "lat"),
                    lng = ToDouble(d, "lng"),
                    radiusMeters = d.TryGetValue("radius", out var rad) ? Convert.ToSingle(rad) : 20f,
                    layers = PoiLayer.GPS
                };
                r.Add(p);
            }
            return r;
        }
        private static double ToDouble(Dictionary<string,object> d,string k){ return d.TryGetValue(k, out var v) ? Convert.ToDouble(v, System.Globalization.CultureInfo.InvariantCulture) : 0.0; }
        private static T EnumTry<T>(Dictionary<string,object> d,string k, T def) where T:struct
        {
            if (d.TryGetValue(k, out var v)) if (Enum.TryParse(v.ToString(), out T t)) return t; return def;
        }
        private static string ToJsonArray(List<object> arr)
        {
            // Brak serializatora – zlepimy szybki JSON (wystarczy do cache’u)
            var sb = new StringBuilder(); sb.Append('[');
            for (int i=0;i<arr.Count;i++)
            {
                if (i>0) sb.Append(',');
                sb.Append(ToJsonObj(arr[i] as Dictionary<string,object>));
            }
            sb.Append(']'); return sb.ToString();
        }
        private static string ToJsonObj(Dictionary<string,object> d)
        {
            var sb = new StringBuilder(); sb.Append('{'); bool first=true;
            foreach (var kv in d)
            {
                if (!first) sb.Append(',');
                first=false; sb.Append('"').Append(kv.Key).Append('"').Append(':');
                switch (kv.Value)
                {
                    case string s: sb.Append('"').Append(s.Replace("\"","\\\"")).Append('"'); break;
                    case bool b: sb.Append(b ? "true":"false"); break;
                    default: sb.Append(Convert.ToString(kv.Value, System.Globalization.CultureInfo.InvariantCulture)); break;
                }
            }
            sb.Append('}'); return sb.ToString();
        }

        // ===== PARSER ODP. OVERPASS =====
        private sealed class OverpassItem
        {
            public string id;
            public string type; // node/way
            public double lat;
            public double lon;
            public string name;
            public string amenity;
        }

        private static List<OverpassItem> ParseOverpassJson(string txt, out string err)
        {
            err = null;
            var res = new List<OverpassItem>();
            try
            {
                var root = Json.Deserialize(txt) as Dictionary<string, object>;
                if (root == null || !root.TryGetValue("elements", out var el)) { err="bad json"; return res; }
                var arr = el as List<object>;
                foreach (var o in arr)
                {
                    var d = o as Dictionary<string, object>;
                    if (d == null) continue;
                    var item = new OverpassItem();
                    item.id = Convert.ToString(d["id"], System.Globalization.CultureInfo.InvariantCulture);
                    item.type = d.TryGetValue("type", out var typ) ? (string)typ : "node";

                    // pozycja: node => lat/lon, way => center{lat,lon} jeśli poprosimy "out center"
                    if (d.TryGetValue("lat", out var lat)) item.lat = Convert.ToDouble(lat, System.Globalization.CultureInfo.InvariantCulture);
                    if (d.TryGetValue("lon", out var lon)) item.lon = Convert.ToDouble(lon, System.Globalization.CultureInfo.InvariantCulture);
                    if ((item.lat==0 || item.lon==0) && d.TryGetValue("center", out var center))
                    {
                        var c = center as Dictionary<string, object>;
                        if (c!=null)
                        {
                            item.lat = Convert.ToDouble(c["lat"], System.Globalization.CultureInfo.InvariantCulture);
                            item.lon = Convert.ToDouble(c["lon"], System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }

                    if (d.TryGetValue("tags", out var tagsObj))
                    {
                        var tags = tagsObj as Dictionary<string, object>;
                        if (tags != null)
                        {
                            if (tags.TryGetValue("name", out var n)) item.name = n as string;
                            if (tags.TryGetValue("amenity", out var a)) item.amenity = a as string;
                        }
                    }

                    if (item.lat!=0 && item.lon!=0 && !string.IsNullOrEmpty(item.amenity))
                        res.Add(item);
                }
            }
            catch (Exception e) { err = e.Message; }
            return res;
        }
    }
}