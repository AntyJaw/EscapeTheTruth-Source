using UnityEngine;
using TMPro;
using System.Text;
using EtT.POI;
using EtT.Map;
using EtT.Systems;

namespace EtT.UI.Placeholder
{
    public sealed class PoiFinderPanel : MonoBehaviour
    {
        [SerializeField] private PoiService poiService;
        [SerializeField] private GeoMissionController geo;
        [SerializeField] private TMP_Text listText;  // prosty output; można podpiąć listę UI
        private (PoiInstance poi, double meters)[] _lastResults = new (PoiInstance,double)[0];

        private bool TryGetPlayer(out EtT.World.Position p)
        {
            p = geo ? geo.LastPlayer : default;
            return !(geo == null || (p.Lat == 0 && p.Lng == 0));
        }

        public void FindNearestCafe()
        {
            if (!TryGetPlayer(out var p)) { Write("Brak pozycji gracza."); return; }
            var res = PoiQueryService.FindNearestByType(poiService, p, PoiType.Cafe, 5);
            ShowResults(res, "Kawiarnie");
        }

        public void FindNearestHealing()
        {
            if (!TryGetPlayer(out var p)) { Write("Brak pozycji gracza."); return; }
            var res = PoiQueryService.FindNearestByNeed(poiService, p, NeedCategory.Healing, 5);
            ShowResults(res, "Punkty leczenia");
        }

        public void FindNearestQuickRest()
        {
            if (!TryGetPlayer(out var p)) { Write("Brak pozycji gracza."); return; }
            var res = PoiQueryService.FindNearestByNeed(poiService, p, NeedCategory.QuickRest, 5);
            ShowResults(res, "Szybki odpoczynek");
        }

        private void ShowResults(System.Collections.Generic.List<(PoiInstance poi, double meters)> res, string header)
        {
            _lastResults = res.ToArray();
            var sb = new StringBuilder().AppendLine(header + ":");
            for (int i=0;i<_lastResults.Length;i++)
            {
                var r = _lastResults[i];
                sb.AppendLine($"{i+1}) {r.poi.name} — {(r.meters/1000.0):0.00} km");
            }
            if (_lastResults.Length == 0) sb.AppendLine("Brak w okolicy.");
            Write(sb.ToString());
        }

        // Wybór z listy (1..N) – pod przyciskami UI albo input field
        public void NavigateToIndex(int oneBasedIndex)
        {
            int i = oneBasedIndex - 1;
            if (i < 0 || i >= _lastResults.Length) { Write("Zły indeks."); return; }
            var poi = _lastResults[i].poi;
            PoiQueryService.RouteToPoi(geo, poi);
            Write($"Nawigacja do: {poi.name}");
        }

        public void OpenExternalMapsToIndex(int oneBasedIndex, bool applePreferred)
        {
            int i = oneBasedIndex - 1;
            if (i < 0 || i >= _lastResults.Length) { Write("Zły indeks."); return; }
            PoiQueryService.RouteToPoi(geo, _lastResults[i].poi); // zaktualizuj cel
            Application.OpenURL(geo.ExternalDirectionsUrl(applePreferred));
        }

        private void Write(string s){ if (listText) listText.text = s; }
    }
}