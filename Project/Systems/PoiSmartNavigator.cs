using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EtT.Core;
using EtT.POI;
using EtT.Map;
using EtT.Systems;
using EtT.World;

namespace EtT.Smart
{
    public enum NeedPriority { None, Warning, Critical }

    /// <summary>
    /// Monitoruje paski gracza, proponuje/uruchamia nawigację do najbliższego POI odpowiedniego typu.
    /// WARNING -> pokazuje prompt; CRITICAL -> auto-prowadzenie (z komunikatem).
    /// Jeśli brak POI w pamięci -> auto skan przez Overpass (jeśli jest podpięty provider).
    /// </summary>
    public sealed class PoiSmartNavigator : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PoiService poiService;
        [SerializeField] private GeoMissionController geo;
        [SerializeField] private UI.Placeholder.SmartNeedPrompt promptUI;      // okienko dialogowe
        [SerializeField] private OverpassPoiProvider overpassProvider;         // opcjonalny auto-scan

        [Header("Progi (0..100)")]
        [SerializeField] private int energyWarn = 30;
        [SerializeField] private int energyCritical = 10;
        [SerializeField] private int healthWarn = 40;
        [SerializeField] private int healthCritical = 15;
        [SerializeField] private int psycheWarn = 25;
        [SerializeField] private int psycheCritical = 10;

        [Header("Inne")]
        [SerializeField] private int autoScanRadiusMeters = 1200;
        [SerializeField] private float promptCooldownSeconds = 120f;
        [SerializeField] private bool vibrateOnWarning = true;

        private double _cooldownUntil;
        private (PoiInstance poi, double meters)? _lastSuggestion;

        private void OnEnable()
        {
            GameEvents.OnEnergyChanged += OnEnergyChanged;
            GameEvents.OnHealthChanged += OnHealthChanged;
            GameEvents.OnSanityChanged += OnSanityChanged;
        }
        private void OnDisable()
        {
            GameEvents.OnEnergyChanged -= OnEnergyChanged;
            GameEvents.OnHealthChanged -= OnHealthChanged;
            GameEvents.OnSanityChanged -= OnSanityChanged;
        }

        private void OnEnergyChanged(int v)
        {
            EvaluateNeed("energia", v, energyWarn, energyCritical, NeedCategory.CoffeeBreak);
        }
        private void OnHealthChanged(int v)
        {
            EvaluateNeed("zdrowie", v, healthWarn, healthCritical, NeedCategory.Healing);
        }
        private void OnSanityChanged(float v)
        {
            EvaluateNeed("psyche", Mathf.RoundToInt(v), psycheWarn, psycheCritical, NeedCategory.QuickRest);
        }

        private void EvaluateNeed(string channel, int value, int warn, int crit, NeedCategory need)
        {
            var now = Now();
            var prio = value <= crit ? NeedPriority.Critical : (value <= warn ? NeedPriority.Warning : NeedPriority.None);
            if (prio == NeedPriority.None) return;

            // Anti-spam cooldown (tylko dla WARNING)
            if (prio == NeedPriority.Warning && now < _cooldownUntil) return;
            if (prio == NeedPriority.Warning) _cooldownUntil = now + promptCooldownSeconds;

            // Znajdź najbliższe kandydaty
            var playerPos = geo ? geo.LastPlayer : default;
            var candidates = PoiQueryService.FindNearestByNeed(poiService, playerPos, need, 5);

            // Jeśli brak POI i mamy Overpass -> auto-skan
            if ((candidates == null || candidates.Count == 0) && overpassProvider != null && playerPos.Lat != 0)
            {
                var types = PoiQueryService.NeedToTypes[need];
                overpassProvider.Scan(playerPos.Lat, playerPos.Lng, autoScanRadiusMeters, types, (list, err) =>
                {
                    if (err != null) { GameEvents.RaiseLink13($"[SmartPOI] Skan error: {err}"); return; }
                    poiService.RegisterRuntimeMany(list);
                    var res = PoiQueryService.FindNearestByNeed(poiService, playerPos, need, 5);
                    HandleSuggestion(prio, need, channel, res);
                });
                return;
            }

            HandleSuggestion(prio, need, channel, candidates);
        }

        private void HandleSuggestion(NeedPriority prio, NeedCategory need, string channel, List<(PoiInstance poi,double meters)> res)
        {
            if (res == null || res.Count == 0) { GameEvents.RaiseLink13($"[SmartPOI] Brak POI dla: {need}"); return; }

            _lastSuggestion = res[0];

            var km = _lastSuggestion.Value.meters/1000.0;
            var poi = _lastSuggestion.Value.poi;
            var msg = prio == NeedPriority.Critical
                ? $"ALERT ({channel})! Najbliższy {poi.type}: {poi.name} – {km:0.00} km. Prowadzę."
                : $"Niski poziom ({channel}). Najbliższy {poi.type}: {poi.name} – {km:0.00} km. Prowadzić?";

            if (prio == NeedPriority.Critical)
            {
                GameEvents.RaiseLink13(msg);
#if UNITY_ANDROID || UNITY_IOS
                if (vibrateOnWarning) Handheld.Vibrate();
#endif
                StartNavigationTo(poi);
                return;
            }

            // WARNING -> prompt
            if (promptUI != null)
            {
                promptUI.Show(
                    header: "Potrzeba: " + channel.ToUpperInvariant(),
                    body: msg,
                    onAccept: () => StartNavigationTo(poi),
                    onDecline: () => { /* noop */ },
                    onAdBoost: () => GrantAdBoost(channel)
                );
#if UNITY_ANDROID || UNITY_IOS
                if (vibrateOnWarning) Handheld.Vibrate();
#endif
            }
            else
            {
                GameEvents.RaiseLink13(msg + " (brak UI prompt)");
            }
        }

        private void StartNavigationTo(PoiInstance poi)
        {
            if (!geo) return;
            PoiQueryService.RouteToPoi(geo, poi); // rysuj trasę / ustaw cel
        }

        private void GrantAdBoost(string channel)
        {
            // Stub pod ads — tu wpinamy rewarded ad i callback sukcesu
            var p = Services.ServiceLocator.Get<IPlayerService>();
            switch (channel)
            {
                case "energia": p.AddEnergy(15); break;
                case "zdrowie": p.AddHealth(10); break;
                case "psyche":  p.AddSanity(+8); break;
            }
            GameEvents.RaiseLink13("[Bonus] Szybkie doładowanie zastosowane.");
        }

        private static double Now() => System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}