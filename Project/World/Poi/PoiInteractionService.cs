using UnityEngine;
using EtT.Services;

namespace EtT.World.Poi
{
    public sealed class PoiInteractionService : IPoiInteractionService
    {
        public bool Interact(Poi poi)
        {
            if (poi.isMissionLocked)
            {
                ServiceLocator.Get<ILink13Service>()?.SendSystem($"[POI] {poi.name} jest nieaktywne – obszar śledztwa zablokowany.");
                return false;
            }

            var player = ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService;
            var inv = ServiceLocator.Get<IInventoryService>();
            if (player == null || inv == null) return false;

            switch (poi.kind)
            {
                case EtT.PoiKind.Pharmacy:
                    player.AddHealth(+25);
                    player.AddSanity(+5);
                    inv.Add("medkit_small", 1);
                    ServiceLocator.Get<ILink13Service>()?.SendSystem($"[Apteka] Uleczono +25 HP, +5 psyche. Dodano apteczkę.");
                    return true;

                case EtT.PoiKind.Cafe:
                    player.AddEnergy(+30);
                    player.AddSanity(+5);
                    ServiceLocator.Get<ILink13Service>()?.SendSystem($"[Kawiarnia] +30 energii, +5 psyche. Odpoczynek.");
                    return true;

                case EtT.PoiKind.Store:
                    if (inv.Consume("coin", 1))
                    {
                        inv.Add("snack_bar", 1);
                        ServiceLocator.Get<ILink13Service>()?.SendSystem("[Sklep] Kupiono baton (snack_bar).");
                        return true;
                    }
                    ServiceLocator.Get<ILink13Service>()?.SendSystem("[Sklep] Brak środków (coin).");
                    return false;

                case EtT.PoiKind.Ritual:
                    if (!player.EsotericUnlocked)
                    {
                        player.UnlockEsoteric();
                        ServiceLocator.Get<ILink13Service>()?.SendSystem("[Rytuał] Odblokowano Ezoterykę.");
                        return true;
                    }
                    ServiceLocator.Get<ILink13Service>()?.SendSystem("[Rytuał] Już odblokowano Ezoterykę.");
                    return false;

                case EtT.PoiKind.Police:
                    ServiceLocator.Get<ILink13Service>()?.SendSystem("[Posterunek] Złożono raport. Reputacja +1.0");
                    player.AddReputation(+1.0f);
                    return true;

                case EtT.PoiKind.Base:
                    player.AddEnergy(+30); player.AddSanity(+10);
                    ServiceLocator.Get<ILink13Service>()?.SendSystem("[Baza] Regeneracja +30 EN, +10 psyche.");
                    return true;

                default:
                    ServiceLocator.Get<ILink13Service>()?.SendSystem($"[POI] {poi.name} ({poi.kind}) – brak dedykowanej akcji.");
                    return false;
            }
        }

        public Poi? RecommendByNeed(World.Position from, float energy01, float health01)
        {
            return ServiceLocator.Get<IPoiService>().BestByNeed(from, energy01, health01);
        }
    }
}