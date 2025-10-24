namespace EtT.Weather
{
    [System.Serializable]
    public class WeatherSnapshot
    {
        public string condition;   // np. "Rain", "Clear"
        public float temperature;  // °C
        public float humidity;     // %
        public float windSpeed;    // m/s
        public bool isNight;       // true jeśli noc

        public override string ToString()
            => $"{condition}, {temperature}°C, hum {humidity}%";
    }
}