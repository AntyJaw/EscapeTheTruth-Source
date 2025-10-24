using System;
using EtT.Weather;

namespace EtT.Core
{
    // Uwaga: Twój bazowy GameEvents.cs musi być "public static partial class GameEvents".
    public static partial class GameEvents
    {
        // Pogoda
        public static event Action<WeatherSnapshot> OnWeatherChanged;
        public static void RaiseWeatherChanged(WeatherSnapshot snap) => OnWeatherChanged?.Invoke(snap);

        // Atrybuty / profil
        public static event Action OnAttributesChanged;
        public static void RaiseAttributesChanged() => OnAttributesChanged?.Invoke();

        public static event Action<string> OnActiveProfileChanged;
        public static void RaiseActiveProfileChanged(string id) => OnActiveProfileChanged?.Invoke(id);
    }
}