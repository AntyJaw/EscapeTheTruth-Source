using System;
using System.Collections.Generic;
using UnityEngine;
using EtT.Core;
using EtT.Services;
using EtT.World;
using EtT.Map;

namespace EtT.POI
{
    public sealed class PoiService : MonoBehaviour
    {
        [Header("Źródła danych")]
        [SerializeField] private PoiSet staticPoiSet;
        [SerializeField] private float enterThresholdMeters = 25f;

        private readonly List<PoiInstance> _active = new();
        private readonly Dictionary<string, double> _cooldownsUntil = new();

        private Position _lastPlayer;

        public IReadOnlyList<PoiInstance> Active => _active;

        public void LoadFromSet(PoiSet set, Vector2 shiftLatLng = default)
        {
            _active.Clear();
            if (!set) return;
            foreach (var d in set.items)
            {
                _active.Add(new PoiInstance{
                    id = d.poiId, name = d.displayName, type = d.type,
                    lat = 52.23 + shiftLatLng.x, lng = 21.01 + shiftLatLng.y,
                    radiusMeters = d.defaultRadius, layers = d.layers, req = d.requirements, effect = d.effect
                });
            }
            PushToMap();
        }

        public void RegisterRuntime(PoiInstance poi)
        {
            _active.Add(poi);
            PushToMap();
        }

        public void RegisterRuntimeMany(IEnumerable<PoiInstance> list)
        {
            _active.AddRange(list);
            PushToMap();
        }

        public void OnPlayerPosition(Position p)
        {
            _lastPlayer = p;
            foreach (var poi in _active)
            {
                var dist = Haversine(poi.lat, poi.lng, p.Lat, p.Lng);
                bool inside = dist <= Math.Max(enterThresholdMeters, poi.radiusMeters);
                bool wasInside = _entered.Contains(poi.id);
                if (inside && !wasInside) { _entered.Add(poi.id); GameEvents.RaisePoiEntered(poi); }
                else if (!inside && wasInside) { _entered.Remove(poi.id); GameEvents.RaisePoiLeft(poi); }
            }
        }

        public PoiResult Interact(string poiId)
        {
            var poi = _active.Find(x => x.id == poiId);
            if (poi == null) return Fail("POI nie istnieje.");
            double now = Now();
            if (_cooldownsUntil.TryGetValue(poi.id, out var until) && now < until) return Fail("Jeszcze chwilę...");

            var player = ServiceLocator.Get<IPlayerService>();
            if (poi.req.requiresEsoteric && !player.EsotericUnlocked) return Fail("Wymagana ezoteryka.");
            if (poi.req.requiredClass != 0 && player.Class != poi.req.requiredClass) return Fail("Wymagana klasa: " + poi.req.requiredClass);
            if (player.Ezoteryka < poi.req.minEzoter) return Fail("Za niska ezoteryka.");
            if (player.Level < poi.req.minPlayerLevel) return Fail("Za niski poziom.");
            if (player.Sanity < poi.req.minPsychika) return Fail("Za niska psychika.");
            if (player.Energy < poi.req.minEnergy) return Fail("Za niska energia.");

            var dist = Haversine(poi.lat, poi.lng, _lastPlayer.Lat, _lastPlayer.Lng);
            if (!poi.layers.HasFlag(PoiLayer.System) && dist > Math.Max(enterThresholdMeters, poi.radiusMeters))
                return Fail("Podejdź bliżej.");

            ApplyEffect(poi.effect, player);
            if (poi.effect.cooldownSeconds > 0) _cooldownsUntil[poi.id] = now + poi.effect.cooldownSeconds;

            var ok = new PoiResult{ ok=true, message="OK" };
            GameEvents.RaisePoiInteracted(poi, ok);
            return ok;

            static PoiResult Fail(string m) => new PoiResult{ ok=false, message=m };
        }

        private void ApplyEffect(PoiEffect fx, IPlayerService p)
        {
            if (fx.healthDelta != 0)   p.AddHealth(fx.healthDelta);
            if (fx.energyDelta != 0)   p.AddEnergy(fx.energyDelta);
            if (fx.psycheDelta != 0)   p.AddSanity(fx.psycheDelta);
            if (Math.Abs(fx.reputationDelta) > 0.001f) p.AddReputation(fx.reputationDelta);
            if (fx.classXp > 0)        p.AddClassXp(fx.classXp);
            if (fx.personalXp > 0)     p.AddPersonalXp(fx.personalXp);
            if (fx.esotericXp > 0)     p.AddEsotericXp(fx.esotericXp);
            if (!string.IsNullOrEmpty(fx.narrativeTag)) GameEvents.RaiseLink13($"[POI] {fx.narrativeTag}");
        }

        private void PushToMap()
        {
            var mv = FindObjectOfType<EtT.Map.SlippyTileMapView>();
            if (!mv) return;
            var list = new List<EtT.Map.Poi>();
            foreach (var p in _active)
                list.Add(new EtT.Map.Poi{ id=p.id, type=(EtT.Map.PoiType)p.type, title=p.name, pos=new Position(p.lat,p.lng), radiusMeters=p.radiusMeters });
            mv.SetPois(list);
        }

        private readonly HashSet<string> _entered = new();
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
        private static double Now() => System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}