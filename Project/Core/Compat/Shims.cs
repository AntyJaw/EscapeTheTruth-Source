// ===== EtT – Compatibility Shims =====
// Dostarcza minimalne definicje brakujących typów, żeby cały projekt się kompilował.
// Gdy wdrożymy docelowe serwisy, te definicje można usunąć/pliki rozbić.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtT
{
    // ----- ROUTING -----
    public enum RouteMode { Foot, Bike, Car, Transit }

    public interface IRoutingProvider
    {
        void RequestRoute(World.Position from, World.Position to, RouteMode mode,
            Action<List<World.Position>> onOk,
            Action<string> onError);
    }

    // Prosty stub – rysuje "trasę" jako linię prostą
    public sealed class SimpleRoutingProvider : IRoutingProvider
    {
        public void RequestRoute(World.Position from, World.Position to, RouteMode mode,
            Action<List(World.Position)> onOk, Action<string> onError)
        {
            onOk?.Invoke(new List<World.Position> { from, to });
        }
    }

    // ----- PROFILES -----
    public interface IProfilesService
    {
        string ActiveId { get; }
        event Action<string> OnActiveProfileChanged;
    }

    // ----- INVENTORY -----
    public interface IInventoryService
    {
        void Init(string keyPrefix);
        bool HasItem(string itemId, int amount = 1);
        void AddItem(string itemId, int amount = 1);
        bool ConsumeItem(string itemId, int amount = 1);
    }

    // ----- POI -----
    public enum PoiKind
    {
        Pharmacy, Cafe, Store, Bench, Ritual, Police, Link13Station, Base
    }

    [System.Flags]
    public enum PoiLayer { GPS = 1, AR = 2, Runic = 4, Psychic = 8 }

    public readonly struct PoiRequirements
    {
        public readonly int minLevel;
        public readonly int minEzoteryka;
        public PoiRequirements(int minLevel=1, int minEzoteryka=0)
        { this.minLevel=minLevel; this.minEzoteryka=minEzoteryka; }
    }

    public readonly struct PoiEffect
    {
        public readonly int healthDelta, energyDelta, psycheDelta;
        public readonly int personalXp, classXp;
        public readonly int cooldownSeconds;
        public PoiEffect(int healthDelta=0,int energyDelta=0,int psycheDelta=0,int personalXp=0,int classXp=0,int cooldownSeconds=60)
        { this.healthDelta=healthDelta; this.energyDelta=energyDelta; this.psycheDelta=psycheDelta; this.personalXp=personalXp; this.classXp=classXp; this.cooldownSeconds=cooldownSeconds; }
    }
}

namespace EtT.World
{
    // Masz to wcześniej w projekcie; zostawiam tu, jeśli brak.
    public readonly struct Position { public readonly double Lat, Lng; public Position(double lat,double lng){Lat=lat;Lng=lng;} }

    public sealed class ZoneInfo { public string Name; public Position Center; public float RadiusMeters; }
}

namespace EtT.World.Poi
{
    using static EtT.Services.ServiceLocator;

    public struct Poi
    {
        public string id, name;
        public EtT.PoiKind type;
        public double lat, lng;
        public float radiusMeters;
        public EtT.PoiLayer layers;
        public EtT.PoiRequirements req;
        public EtT.PoiEffect effect;
        public bool locked;

        public float DistanceMeters(EtT.World.Position p)
        {
            return Haversine(p.Lat, p.Lng, lat, lng);
        }

        static float Haversine(double lat1,double lon1,double lat2,double lon2)
        {
            const double R=6371000.0;
            double dLat=(lat2-lat1)*Mathf.Deg2Rad, dLon=(lon2-lon1)*Mathf.Deg2Rad;
            double a=System.Math.Sin(dLat/2)*System.Math.Sin(dLat/2)+
                      System.Math.Cos(lat1*Mathf.Deg2Rad)*System.Math.Cos(lat2*Mathf.Deg2Rad)*
                      System.Math.Sin(dLon/2)*System.Math.Sin(dLon/2);
            double c=2*System.Math.Atan2(System.Math.Sqrt(a),System.Math.Sqrt(1-a));
            return (float)(R*c);
        }
    }

    public interface IPoiService
    {
        List<Poi> Active { get; }
        void RegisterRuntime(Poi poi);
        void OnPlayerPosition(EtT.World.Position p);
        Poi[] Nearest(EtT.World.Position center, EtT.PoiKind kind, int maxCount = 5);
    }

    // Prosta implementacja „na pamięci”
    public sealed class PoiService : IPoiService
    {
        public List<Poi> Active { get; } = new();
        private EtT.World.Position _last;

        public void RegisterRuntime(Poi poi) => Active.Add(poi);
        public void OnPlayerPosition(EtT.World.Position p) => _last = p;

        public Poi[] Nearest(EtT.World.Position center, EtT.PoiKind kind, int maxCount = 5)
        {
            var list = new List<(Poi poi, float dist)>();
            foreach (var p in Active)
            {
                if (p.type != kind) continue;
                list.Add((p, p.DistanceMeters(center)));
            }
            list.Sort((a,b)=>a.dist.CompareTo(b.dist));
            var outArr = new List<Poi>();
            for (int i=0;i<list.Count && i<maxCount;i++) outArr.Add(list[i].poi);
            return outArr.ToArray();
        }
    }

    public interface IPoiInteractionService
    {
        // wybór wg potrzeby (energia/zdrowie)
        Poi? RecommendByNeed(EtT.World.Position player, float energy01, float health01);
        // natychmiastowa interakcja
        void Interact(Poi p);
    }

    public sealed class PoiInteractionService : IPoiInteractionService
    {
        public Poi? RecommendByNeed(EtT.World.Position player, float energy01, float health01)
        {
            var svc = Get<IPoiService>();
            if (svc == null) return null;

            if (energy01 < 0.3f)
            {
                var cafes = svc.Nearest(player, EtT.PoiKind.Cafe, 1);
                if (cafes.Length > 0) return cafes[0];
            }
            if (health01 < 0.4f)
            {
                var ph = svc.Nearest(player, EtT.PoiKind.Pharmacy, 1);
                if (ph.Length > 0) return ph[0];
            }
            var any = svc.Nearest(player, EtT.PoiKind.Store, 1);
            return any.Length > 0 ? any[0] : (Poi?)null;
        }

        public void Interact(Poi p)
        {
            // Minimalny efekt: wypisz do LINK-13; realne efekty dodamy w Inventory/PlayerService
            Get<EtT.ILink13Service>()?.SendSystem($"[POI] Interakcja: {p.name} ({p.type})");
        }
    }
}

namespace EtT.Weather
{
    // ----- POGODA -----
    public sealed class WeatherSnapshot
    {
        public float humidity01;
        public float rain01;
        public float temperatureC;
        public float windMS;
        public float light01; // 0..1
    }

    public interface IWeatherService
    {
        float Humidity01 { get; }
        float Rain01 { get; }
        float TemperatureC { get; }
        float WindMS { get; }
        float Light01 { get; }
        WeatherSnapshot Snapshot();
        void Refresh(double lat, double lng);
        void Tick(float dt);
    }

    // Prosty stub – stała pogoda
    public sealed class DummyWeatherService : IWeatherService
    {
        float _h=0.5f,_r=0f,_t=15f,_w=1f,_l=1f;
        public float Humidity01 => _h;
        public float Rain01 => _r;
        public float TemperatureC => _t;
        public float WindMS => _w;
        public float Light01 => _l;
        public WeatherSnapshot Snapshot() => new WeatherSnapshot{humidity01=_h,rain01=_r,temperatureC=_t,windMS=_w,light01=_l};
        public void Refresh(double lat, double lng) { /* tu będzie OpenMeteo/OWM */ }
        public void Tick(float dt) { /* animuj pogodę jeśli chcesz */ }
    }
}

namespace EtT.TimeCompat
{
    // Dostęp jak w kodzie: EtT.TimeCompat.Clock.deltaTime / realtimeSinceStartupAsDouble
    public static class Clock
    {
        public static float deltaTime => UnityEngine.Time.deltaTime;
        public static double realtimeSinceStartupAsDouble => UnityEngine.Time.realtimeSinceStartupAsDouble;
    }
}