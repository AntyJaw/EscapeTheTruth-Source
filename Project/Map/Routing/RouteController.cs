using UnityEngine;
using TMPro;
using EtT.Services;
using EtT.World.Poi;
using EtT.World;
using EtT.Core;

namespace EtT.Map.Routing
{
    /// <summary>
    /// Prosty kontroler do uruchamiania nawigacji do wybranego POI.
    /// Aktualnie wyświetla dystans/ETA i log przez LINK-13.
    /// Rysowanie linii zrobimy po wpięciu mapy.
    /// </summary>
    public sealed class RouteController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Dropdown modeDropdown; // Foot/Bike/Car
        [SerializeField] private TMP_Text infoText;

        public void NavigateToNearest(PoiType type)
        {
            var gps = ServiceLocator.Get<IGpsWorldService>();
            var from = gps.GetPlayerPosition();

            var poiSvc = ServiceLocator.Get<PoiService>();
            var nearest = poiSvc.FindNearest(type, from.Lat, from.Lng);
            if (nearest == null)
            {
                GameEvents.RaiseLink13($"[Routing] Brak POI typu {type} w rejestrze.");
                if (infoText) infoText.text = "Brak punktów.";
                return;
            }

            NavigateToPosition(new Position(nearest.Lat, nearest.Lng), nearest.Name);
        }

        public void NavigateToPosition(Position target, string label = "cel")
        {
            var gps = ServiceLocator.Get<IGpsWorldService>();
            var from = gps.GetPlayerPosition();

            var routing = ServiceLocator.Get<IRoutingProvider>();
            var mode = GetMode();

            routing.RequestRoute(from, target, mode,
                onSuccess: r =>
                {
                    var km = r.DistanceMeters / 1000f;
                    var min = r.DurationSeconds / 60f;
                    string source = r.IsFallback ? "linia prosta" : "ulice";
                    GameEvents.RaiseLink13($"[Routing] Trasa ({source}): {km:F2} km, ~{min:F0} min.");

                    if (infoText)
                        infoText.text = $"{label}\n{km:F2} km • ~{min:F0} min • {mode}";
                    // TODO: przekazać r.Points do modułu mapy i narysować
                },
                onError: err =>
                {
                    GameEvents.RaiseLink13($"[Routing] Błąd: {err}");
                    if (infoText) infoText.text = $"Błąd routingu: {err}";
                });
        }

        private RouteMode GetMode()
        {
            if (!modeDropdown) return RouteMode.Foot;
            return modeDropdown.value switch
            {
                1 => RouteMode.Bike,
                2 => RouteMode.Car,
                _ => RouteMode.Foot
            };
        }
    }
}