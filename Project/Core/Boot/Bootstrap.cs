using UnityEngine;
using EtT.Services;

namespace EtT.Core
{
    public sealed class Bootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            ServiceLocator.Clear();

            // Core
            ServiceLocator.Register<ISaveService>(new Save.SaveServiceJson());
            ServiceLocator.Register<IPlayerService>(new Player.PlayerService());
            ServiceLocator.Register<IMissionService>(new Missions.MissionService());
            ServiceLocator.Register<IAnomalyService>(new Generators.AnomalyService());
            ServiceLocator.Register<INarrativeWeaver>(new Generators.NarrativeWeaver());

            // GPS/POI/Routing
            ServiceLocator.Register<IGpsWorldService>(new World.GpsWorldServiceUnity());
            ServiceLocator.Register<EtT.World.Poi.IPoiService>(new EtT.World.Poi.PoiService());
            ServiceLocator.Register<EtT.World.Poi.IPoiInteractionService>(new EtT.World.Poi.PoiInteractionService());
            ServiceLocator.Register<IRoutingProvider>(new SimpleRoutingProvider());

            // AR
            ServiceLocator.Register<EtT.AR.Mission.IARMissionService>(new EtT.AR.Mission.ARMissionService());
            ServiceLocator.Register<EtT.AR.Flairs.IARFlairService>(new EtT.AR.Flairs.ARFlairService());

            // Pogoda (stub – realny provider możesz zarejestrować zamiast tego)
            ServiceLocator.Register<EtT.Weather.IWeatherService>(new EtT.Weather.WeatherService_OpenMeteo());

            Debug.Log("[Bootstrap] Init OK (Core + GPS + POI + Routing + AR + Weather).");
        }
    }
}ę