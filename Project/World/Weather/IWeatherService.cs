namespace EtT.Weather
{
    public sealed class WeatherSnapshot
    {
        public float humidity01;
        public float rain01;
        public float temperatureC;
        public float windMS;
        public float light01;
    }

    public interface IWeatherService
    {
        float Humidity01 { get; }
        float Rain01 { get; }
        float TemperatureC { get; }
        float WindMS { get; }
        float Light01 { get; }

        WeatherSnapshot Snapshot();
        void Refresh(double lat, double lng);
        void Tick(float dt);
    }
}
