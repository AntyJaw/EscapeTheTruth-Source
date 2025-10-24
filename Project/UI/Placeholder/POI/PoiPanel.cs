using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using EtT.Services;

namespace EtT.UI.Placeholder
{
    public sealed class PoiPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text listText;
        [SerializeField] private TMP_Dropdown kindDropdown;
        [SerializeField] private Button refreshBtn;
        [SerializeField] private Button interactBtn;
        [SerializeField] private Button needBtn;
        [SerializeField] private Button navigateBtn;

        private EtT.PoiKind _currentKind = EtT.PoiKind.Cafe;
        private EtT.World.Poi.Poi? _selected;

        private void Awake()
        {
            if (kindDropdown)
            {
                kindDropdown.ClearOptions();
                kindDropdown.AddOptions(new System.Collections.Generic.List<string>{
                    "Apteka","Kawiarnia","Sklep","Ławka","Rytuał","Posterunek","LINK-13","Baza"
                });
                kindDropdown.onValueChanged.AddListener(OnKind);
            }
            if (refreshBtn)  refreshBtn.onClick.AddListener(RefreshList);
            if (interactBtn) interactBtn.onClick.AddListener(DoInteract);
            if (needBtn)     needBtn.onClick.AddListener(FindByNeed);
            if (navigateBtn) navigateBtn.onClick.AddListener(DoNavigate);
        }

        private void OnEnable(){ RefreshList(); }

        private void OnKind(int idx)
        {
            _currentKind = idx switch {
                0=>EtT.PoiKind.Pharmacy, 1=>EtT.PoiKind.Cafe, 2=>EtT.PoiKind.Store, 3=>EtT.PoiKind.Bench,
                4=>EtT.PoiKind.Ritual, 5=>EtT.PoiKind.Police, 6=>EtT.PoiKind.Link13Station, 7=>EtT.PoiKind.Base,
                _=>EtT.PoiKind.Cafe
            };
            RefreshList();
        }

        private void RefreshList()
        {
            var gps = ServiceLocator.Get<EtT.IGpsWorldService>();
            var poiSvc = ServiceLocator.Get<EtT.IPoiService>();
            var pos = gps.GetPlayerPosition();
            var arr = poiSvc.Nearest(pos, _currentKind, 5);

            var sb = new StringBuilder();
            _selected = null;
            for (int i=0;i<arr.Length;i++)
            {
                float dist = Haversine(pos.Lat,pos.Lng, arr[i].lat,arr[i].lng);
                sb.AppendLine($"{i+1}. {arr[i].name} — {dist:0} m");
                if (i==0) _selected = arr[i];
            }
            if (listText) listText.text = sb.Length>0? sb.ToString() : "Brak punktów tego typu w pobliżu.";
        }

        private void FindByNeed()
        {
            var gps = ServiceLocator.Get<EtT.IGpsWorldService>();
            var player = ServiceLocator.Get<EtT.IPlayerService>() as EtT.Player.PlayerService;
            var poiInt = ServiceLocator.Get<EtT.IPoiInteractionService>();
            var pos = gps.GetPlayerPosition();

            float e01 = Mathf.Clamp01(player.Energy/100f);
            float h01 = Mathf.Clamp01(player.Health/100f);
            var p = poiInt.RecommendByNeed(pos, e01, h01);
            _selected = p;
            if (p.HasValue)
                EtT.Services.ServiceLocator.Get<EtT.ILink13Service>()?.SendSystem($"[POI] Najlepszy punkt wg potrzeby: {p.Value.name}");
            else
                EtT.Services.ServiceLocator.Get<EtT.ILink13Service>()?.SendSystem("[POI] Brak dopasowania po potrzebie.");
        }

        private void DoInteract()
        {
            if (!_selected.HasValue) return;
            ServiceLocator.Get<EtT.IPoiInteractionService>().Interact(_selected.Value);
        }

        private void DoNavigate()
        {
            if (!_selected.HasValue) return;
            var gps = ServiceLocator.Get<EtT.IGpsWorldService>();
            var route = ServiceLocator.Get<EtT.IRoutingProvider>();
            var from = gps.GetPlayerPosition();
            var to   = new EtT.World.Position(_selected.Value.lat, _selected.Value.lng);
            route.RequestRoute(from, to, EtT.RouteMode.Foot,
                _ => EtT.Services.ServiceLocator.Get<EtT.ILink13Service>()?.SendSystem($"[Nawigacja] Trasa do {_selected.Value.name} gotowa."),
                err => EtT.Services.ServiceLocator.Get<EtT.ILink13Service>()?.SendSystem($"[Nawigacja] Błąd: {err}")
            );
        }

        private static float Haversine(double lat1,double lon1,double lat2,double lon2)
        {
            const double R=6371000.0;
            double dLat=(lat2-lat1)*Mathf.Deg2Rad; double dLon=(lon2-lon1)*Mathf.Deg2Rad;
            double a=System.Math.Sin(dLat/2)*System.Math.Sin(dLat/2)+System.Math.Cos(lat1*Mathf.Deg2Rad)*System.Math.Cos(lat2*Mathf.Deg2Rad)*System.Math.Sin(dLon/2)*System.Math.Sin(dLon/2);
            double c=2*System.Math.Atan2(System.Math.Sqrt(a),System.Math.Sqrt(1-a));
            return (float)(R*c);
        }
    }
}