using UnityEngine;
using EtT.Services;

namespace EtT.Generators.Crime
{
    /// <summary>
    /// Steruje parametrami materiału dowodu (_Visibility, _Wetness, _Light) na podstawie pogody/czasu.
    /// Użyj z materiałem URP (np. nasz EtT/EvidenceLit) albo zwykłym – wtedy tylko _Color.a spada.
    /// </summary>
    public sealed class EvidenceVisibilityController : MonoBehaviour
    {
        public float baseVisibility = 1f;
        public float decayPerMinute = 0.02f;
        public Renderer targetRenderer;

        float _startTime;
        IWeatherService _weather;
        ITimeService _time;

        void Start()
        {
            if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
            _weather = ServiceLocator.Get<IWeatherService>();
            _time = ServiceLocator.Get<ITimeService>();
            _startTime = Time.time;
        }

        void Update()
        {
            if (!targetRenderer || _weather == null) return;

            float minutes = (Time.time - _startTime) / 60f;
            var w = _weather.Current;

            float rain01  = Mathf.Clamp01(w.precipitationMm / 5f);
            float wind01  = Mathf.Clamp01(w.windKph / 50f);
            float humid01 = Mathf.Clamp01(w.cloudCoverPct / 100f * 0.7f + (rain01>0?0.3f:0f));
            float light01 = w.isNight ? 0.1f : Mathf.Clamp01(1f - w.cloudCoverPct/100f*0.6f);

            float weatherFactor = Mathf.Lerp(1f, 0.4f, rain01) * Mathf.Lerp(1f, 0.7f, humid01) * Mathf.Lerp(1f, 0.85f, wind01);
            float vis = Mathf.Clamp01(baseVisibility - decayPerMinute * minutes / Mathf.Max(0.2f, weatherFactor));

            var mat = targetRenderer.material;
            if (mat.HasProperty("_Visibility")) mat.SetFloat("_Visibility", vis);
            else
            {
                var c = mat.color; c.a = vis; mat.color = c;
            }
            if (mat.HasProperty("_Wetness")) mat.SetFloat("_Wetness", Mathf.Lerp(0f, 1f, rain01*humid01));
            if (mat.HasProperty("_Light"))   mat.SetFloat("_Light",   light01);
        }
    }
}