using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EtT.World;
using EtT.Map;

namespace EtT.POI
{
    public enum NeedCategory { CoffeeBreak, Healing, QuickRest, Supplies, Ritual, CrimeScene, Coordinator, TeamRally }

    public static class PoiQueryService
    {
        // Zwraca listę najbliższych POI danego typu (posortowane rosnąco po dystansie)
        public static List<(PoiInstance poi, double meters)> FindNearestByType(
            PoiService poiService, Position from, PoiType type, int maxCount = 5)
        {
            var list = new List<(PoiInstance,double)>();
            foreach (var p in poiService.Active)
            {
                if (p.type != type) continue;
                var d = Haversine(p.lat, p.lng, from.Lat, from.Lng);
                list.Add((p, d));
            }
            return list.OrderBy(t => t.Item2).Take(maxCount).ToList();
        }

        // Mapowanie „potrzeby” na zestaw typów POI
        public static readonly Dictionary<NeedCategory, PoiType[]> NeedToTypes = new()
        {
            { NeedCategory.CoffeeBreak, new[]{ PoiType.Cafe } },
            { NeedCategory.Healing,     new[]{ PoiType.Pharmacy, PoiType.Base, PoiType.Police } },
            { NeedCategory.QuickRest,   new[]{ PoiType.Bench, PoiType.Cafe } },
            { NeedCategory.Supplies,    new[]{ PoiType.Store } },
            { NeedCategory.Ritual,      new[]{ PoiType.RitualSite, PoiType.CursedPlace } },
            { NeedCategory.CrimeScene,  new[]{ PoiType.CrimeScene } },
            { NeedCategory.Coordinator, new[]{ PoiType.Link13Station, PoiType.Base } },
            { NeedCategory.TeamRally,   new[]{ PoiType.TeamRally } },
        };

        // Zwraca najbliższe POI spełniające „potrzebę” (agreguje po typach)
        public static List<(PoiInstance poi, double meters)> FindNearestByNeed(
            PoiService poiService, Position from, NeedCategory need, int maxCount = 5)
        {
            var types = NeedToTypes.TryGetValue(need, out var t) ? t : Array.Empty<PoiType>();
            var list = new List<(PoiInstance,double)>();
            foreach (var p in poiService.Active)
            {
                if (!types.Contains(p.type)) continue;
                var d = Haversine(p.lat, p.lng, from.Lat, from.Lng);
                list.Add((p, d));
            }
            return list.OrderBy(tu => tu.Item2).Take(maxCount).ToList();
        }

        // Prosty „routing” MVP – linia prosta (polyline) do MapView
        public static void RouteToPoi(EtT.Systems.GeoMissionController geo, PoiInstance poi)
        {
            if (!geo) return;
            geo.NavigateToPoi(poi.lat, poi.lng, poi.radiusMeters);
        }

        private static double Haversine(double lat1,double lon1,double lat2,double lon2)
        {
            const double R = 6371000.0;
            double dLat = (lat2-lat1) * Mathf.Deg2Rad;
            double dLon = (lon2-lon1) * Mathf.Deg2Rad;
            double a = Mathf.Sin((float)(dLat/2))*Mathf.Sin((float)(dLat/2)) +
                       Mathf.Cos((float)(lat1*Mathf.Deg2Rad)) * Mathf.Cos((float)(lat2*Mathf.Deg2Rad)) *
                       Mathf.Sin((float)(dLon/2))*Mathf.Sin((float)(dLon/2));
            return R * 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1-a)));
        }
    }
}