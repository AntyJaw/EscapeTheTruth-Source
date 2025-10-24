using UnityEngine;
using EtT.Weather;
using EtT.Services;

namespace EtT.Weathering
{
    public sealed class DummyWeatherBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            // Zarejestruj tylko jeśli nie ma innej implementacji
            try { ServiceLocator.Get<IWeatherService>(); }
            catch
            {
                ServiceLocator.Register<IWeatherService>(new DummyWeatherService());
            }
        }
    }
}