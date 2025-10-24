using UnityEngine;
using EtT.Services;
using EtT.Core;

namespace EtT.World.Poi
{
    public static class PoiInteractions
    {
        public static void Interact(PoiInstance poi)
        {
            var ps = ServiceLocator.Get<IPlayerService>() as Player.PlayerService;
            if (ps == null) return;

            switch (poi.Type)
            {
                case PoiType.Pharmacy:
                    ps.AddHealth(+20);
                    ps.AddSanity(+10);
                    GameEvents.RaiseLink13($"[POI] Leczenie w aptece: +20 HP, +10 psyche");
                    break;

                case PoiType.Cafe:
                    ps.AddEnergy(+20);
                    ps.AddSanity(+5);
                    GameEvents.RaiseLink13($"[POI] Odpoczynek w kawiarni: +20 energia, +5 psyche");
                    break;

                case PoiType.Bench:
                    ps.AddEnergy(+10);
                    GameEvents.RaiseLink13($"[POI] Krótki odpoczynek: +10 energia");
                    break;

                case PoiType.Shop:
                    ps.AddEnergy(+5);
                    GameEvents.RaiseLink13($"[POI] Przekąska ze sklepu: +5 energia");
                    break;

                case PoiType.Ritual:
                    if (ps.EsotericUnlocked)
                    {
                        ps.AddEzoteryka(+10);
                        GameEvents.RaiseLink13($"[POI] Rytuał wykonany: +10 ezoteryka");
                    }
                    else
                        GameEvents.RaiseLink13("[POI] Nie posiadasz ezoteryki, nie możesz aktywować rytuału.");
                    break;

                case PoiType.CrimeScene:
                    GameEvents.RaiseLink13("[POI] Weszłeś w strefę śledztwa.");
                    break;

                case PoiType.Link13:
                    GameEvents.RaiseLink13("[POI] Synchronizacja z LINK-13.");
                    break;

                case PoiType.Office:
                    ps.AddSanity(+15);
                    GameEvents.RaiseLink13("[POI] Odpoczynek w biurze: psyche odnowione.");
                    break;

                case PoiType.Police:
                    GameEvents.RaiseLink13("[POI] Przesłuchania/analiza dowodów na posterunku.");
                    break;

                case PoiType.Haunted:
                    ps.AddSanity(-15);
                    GameEvents.RaiseLink13("[POI] Strefa obłędu! Psyche -15.");
                    break;

                case PoiType.Gathering:
                    GameEvents.RaiseLink13("[POI] Spotkanie drużyny.");
                    break;

                case PoiType.Restricted:
                    GameEvents.RaiseLink13("[POI] Dostęp wymaga klasy specjalnej.");
                    break;

                case PoiType.Anomaly:
                    ps.AddSanity(-5);
                    GameEvents.RaiseLink13("[POI] Anomalia – psyche -5.");
                    break;
            }
        }
    }
}