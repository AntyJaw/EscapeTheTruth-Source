using UnityEngine;

namespace EtT.World.Weather
{
    public enum WeatherProviderKind
    {
        OpenMeteo_NoKey, // domyślny – brak klucza, darmowy
        OpenWeather      // opcjonalnie – wymaga klucza
    }

    [CreateAssetMenu(fileName = "WeatherConfig", menuName = "EtT/Configs/Weather")]
    public class WeatherConfig : ScriptableObject
    {
        [Header("Provider")]
        public WeatherProviderKind provider = WeatherProviderKind.OpenMeteo_NoKey;

        [Header("OpenWeather (opcjonalnie)")]
        public string openWeatherApiKey = "";

        [Header("Update")]
        [Tooltip("Co ile minut odświeżać pogodę")]
        public int refreshMinutes = 10;

        [Header("Degradacja dowodów (współczynniki)")]
        [Tooltip("Bazowa degradacja na sekundę (bez opadów)")]
        public float baseDegradePerSec = 0.0005f;
        [Tooltip("Dodatkowo za opad (mm/h przeskalowane do mm/min)")]
        public float rainBoostPerSec = 0.0015f;
        [Tooltip("Dodatkowo przy silnym wietrze (>= 30 km/h)")]
        public float windBoostPerSec = 0.0006f;
        [Tooltip("Mnożnik w nocy (gorsza ochrona, niższa temperatura)")]
        public float nightMultiplier = 1.15f;
    }
}