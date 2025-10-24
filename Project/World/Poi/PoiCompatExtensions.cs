using System;
using System.Collections.Generic;

namespace EtT.World.Poi
{
    public static class PoiCompatExtensions
    {
        // Konwersja z (lat,lng,name) → Poi
        public static Poi ToPoi(this (double lat,double lng,string name) t, EtT.PoiKind kind,
                                float radiusMeters = 15f, bool locked = false)
        {
            return new Poi{
                id = Guid.NewGuid().ToString("N"),
                name = t.name, type = kind,
                lat = t.lat, lng = t.lng,
                radiusMeters = radiusMeters,
                layers = EtT.PoiLayer.GPS,
                req = new EtT.PoiRequirements(),
                effect = new EtT.PoiEffect(),
                locked = locked
            };
        }

        // Zamiennik starego IPoiService.BestByNeed(...)
        public static Poi? BestByNeed(this IPoiService svc, EtT.World.Position player, float energy01, float health01)
        {
            var inter = EtT.Services.ServiceLocator.Get<IPoiInteractionService>();
            return inter?.RecommendByNeed(player, energy01, health01);
        }
    }
}