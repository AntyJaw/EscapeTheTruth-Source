using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using EtT.World;

namespace EtT.Map
{
    // Minimalny renderer tile'ów (slippy map) na siatce UI RawImage.
    // Do devów: bez tokenu (OSM tiles). Podmień tileUrlTemplate na własny serwer przy większym ruchu.
    public sealed class SlippyTileMapView : MonoBehaviour, IMapView
    {
        [Header("Tiles")]
        [SerializeField] private string tileUrlTemplate = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
        [SerializeField, Range(1,6)] private int gridRadius = 2; // 2 => siatka (5x5)
        [SerializeField, Range(1,19)] private int zoom = 15;

        [Header("UI")]
        [SerializeField] private RectTransform tilesRoot; // pusty obiekt z GridLayoutGroup (np. 256x256)
        [SerializeField] private RawImage tilePrefab;
        [SerializeField] private RectTransform playerMarker;
        [SerializeField] private RectTransform missionCircle;

        private Position _center; // geo center
        private Vector2Int _centerTile; // tile XY
        private readonly List<RawImage> _tiles = new();
        private readonly Dictionary<string, Texture2D> _cache = new();

        private void Awake()
        {
            BuildGrid();
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void SetPlayer(Position p)
        {
            var pt = GeoToTilePos(p, zoom);
            var local = TileToLocal(pt);
            if (playerMarker) playerMarker.anchoredPosition = local;
        }

        public void SetMissionArea(Position center, float radiusMeters)
        {
            _center = center;
            _centerTile = LatLonToTile(center, zoom);
            CenterOn(_centerTile);
            if (missionCircle)
            {
                missionCircle.anchoredPosition = TileToLocal(GeoToTilePos(center, zoom));
                // W przybliżeniu: 1 tile w zoom15 ~ 4.8km / 256 px => ~19m/px (zależne od lat), uproszczone:
                float metersPerPixel = 19f;
                float px = radiusMeters / metersPerPixel;
                missionCircle.sizeDelta = new Vector2(px*2, px*2);
            }
        }

        public void SetRoute(List<Position> polyline) { /* TODO: opcjonalnie narysować linię na UI */ }
        public void ClearRoute() { /* no-op w tej wersji */ }

        public void SetPois(IEnumerable<Poi> pois)
        {
            // TODO: postaw ikonki jako UI elementy; teraz no-op
        }

        private void BuildGrid()
        {
            if (!tilePrefab || !tilesRoot) return;
            ClearGrid();
            int size = gridRadius * 2 + 1;
            for (int i=0;i<size*size;i++)
            {
                var img = Instantiate(tilePrefab, tilesRoot);
                img.texture = Texture2D.blackTexture;
                _tiles.Add(img);
            }
        }

        private void ClearGrid()
        {
            foreach (var t in _tiles) if (t) Destroy(t.gameObject);
            _tiles.Clear();
        }

        private void CenterOn(Vector2Int centerTile)
        {
            int size = gridRadius * 2 + 1;
            int idx = 0;
            for (int dy = -gridRadius; dy <= gridRadius; dy++)
            {
                for (int dx = -gridRadius; dx <= gridRadius; dx++)
                {
                    var t = new Vector2Int(centerTile.x + dx, centerTile.y + dy);
                    var ri = _tiles[idx++];
                    LoadTile(t, zoom, ri);
                }
            }

            // Odśwież pozycje markerów:
            SetPlayer(_center); // tylko żeby przeliczyć na aktualny układ
        }

        private void LoadTile(Vector2Int tile, int z, RawImage img)
        {
            int max = 1 << z;
            int x = (tile.x % max + max) % max;
            int y = Mathf.Clamp(tile.y, 0, max-1);

            string url = tileUrlTemplate.Replace("{z}", z.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
            string key = $"{z}/{x}/{y}";
            if (_cache.TryGetValue(key, out var tex))
            {
                img.texture = tex; return;
            }
            StartCoroutine(LoadTileRoutine(url, key, img));
        }

        private System.Collections.IEnumerator LoadTileRoutine(string url, string key, RawImage img)
        {
            using var req = UnityWebRequestTexture.GetTexture(url);
            yield return req.SendWebRequest();
#if UNITY_2020_3_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success) yield break;
#else
            if (req.isNetworkError || req.isHttpError) yield break;
#endif
            var tex = DownloadHandlerTexture.GetContent(req);
            _cache[key] = tex;
            if (img) img.texture = tex;
        }

        // Geo helpers
        private static Vector2Int LatLonToTile(Position p, int z)
        {
            int n = 1 << z;
            int xtile = (int)((p.Lng + 180.0) / 360.0 * n);
            double latRad = p.Lat * Mathf.Deg2Rad;
            int ytile = (int)((1.0 - Mathf.Log(Mathf.Tan((float)latRad) + 1.0f / Mathf.Cos((float)latRad)) / Mathf.PI) / 2.0 * n);
            return new Vector2Int(xtile, ytile);
        }

        private static Vector2 GeoToTilePos(Position p, int z)
        {
            // pozycja w pikselach (256 px per tile) względem (0,0) tiles
            double x = (p.Lng + 180.0) / 360.0 * (1 << z) * 256.0;
            double sinLat = Mathf.Sin((float)(p.Lat * Mathf.Deg2Rad));
            double y = (0.5 - System.Math.Log((1 + sinLat) / (1 - sinLat)) / (4 * System.Math.PI)) * (1 << z) * 256.0;
            return new Vector2((float)x, (float)y);
        }

        private Vector2 TileToLocal(Vector2 pixelGlobal)
        {
            // Lokalne współrzędne w obrębie naszej siatki UI
            // Wyznacz lewy–górny róg siatki w pikselach globalnych:
            int size = gridRadius * 2 + 1;
            var topLeft = new Vector2((_centerTile.x - gridRadius) * 256f, (_centerTile.y - gridRadius) * 256f);
            var localPx = pixelGlobal - topLeft;

            // Zakładamy, że tilesRoot ma GridLayoutGroup z cellSize 256x256 i Pivot (0,1) (lewy-górny).
            // Jeżeli masz inny układ — przeskaluj według RectTransform.
            // Dla prostoty przyjmijmy 1px = 1 jednostka anchoredPosition.
            // Flip Y (UI ma oś +Y w górę, slippy ma +y w dół):
            localPx.y = size*256f - localPx.y;
            return localPx;
        }
    }
}