using UnityEngine;
using TMPro;
using EtT.Services;
using EtT.Core;

namespace EtT.UI.Placeholder
{
    public sealed class WeatherPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text weatherText;

        private void OnEnable()
        {
            var ws = ServiceLocator.Get<EtT.IWeatherService>();
            if (ws != null) UpdateText(ws.Current);

            GameEvents.OnWeatherChanged += UpdateText;
        }

        private void OnDisable()
        {
            GameEvents.OnWeatherChanged -= UpdateText;
        }

        private void UpdateText(WeatherSnapshot w)
        {
            if (!weatherText) return;
            weatherText.text = $"{w.temperatureC:0.#}°C • {w.windKph:0.#} km/h • opad {w.precipitationMm:0.0} • {w.cloudCoverPct}% • {(w.isNight ? "NOC" : "DZIEŃ")}";
        }
    }
}