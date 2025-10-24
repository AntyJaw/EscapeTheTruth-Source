using UnityEngine;

namespace EtT.Weather
{
    // Na razie stub zgodny z interfejsem – później podepniemy realne API.
    public sealed class WeatherService_OpenMeteo : IWeatherService
    {
        float _h=0.55f,_r=0.0f,_t=16f,_w=1.2f,_l=1.0f;

        public float Humidity01   => _h;
        public float Rain01       => _r;
        public float TemperatureC => _t;
        public float WindMS       => _w;
        public float Light01      => _l;

        public WeatherSnapshot Snapshot() => new WeatherSnapshot{
            humidity01=_h, rain01=_r, temperatureC=_t, windMS=_w, light01=_l
        };

        public void Refresh(double lat, double lng)
        {
            // TODO: pobranie z OpenMeteo/OWM — teraz tylko lekka fluktuacja
            _h = Mathf.Clamp01(_h + Random.Range(-0.02f, 0.02f));
            _r = Mathf.Clamp01(_r + Random.Range(-0.05f, 0.05f));
            _t = Mathf.Clamp(_t + Random.Range(-0.5f, 0.5f), -10f, 35f);
            _w = Mathf.Clamp(_w + Random.Range(-0.2f, 0.2f), 0f, 15f);
            _l = Mathf.Clamp01(_l + Random.Range(-0.05f, 0.05f));

            EtT.Core.GameEvents.RaiseWeatherChanged(Snapshot());
        }

        public void Tick(float dt)
        {
            // Możesz animować dzienny cykl światła itd.
        }
    }
}