namespace EtT.World.Omur
{
    public interface IOmurService : IWeatherService
    {
        // tu możemy dodać źródło (OpenWeatherMap) gdy będzie klucz
    }

    public sealed class OmurService : IOmurService
    {
        public float Humidity01 { get; private set; }   = 0.35f;
        public float Rain01 { get; private set; }       = 0.0f;
        public float Light01 { get; private set; }      = 0.85f;
        public float TemperatureC { get; private set; } = 18f;
        public float WindMS { get; private set; }       = 2.0f;

        public void Tick(float dt)
        {
            // prosta doba: sinusoida światła
            var h = System.DateTime.Now.Hour + System.DateTime.Now.Minute/60f;
            Light01 = UnityEngine.Mathf.Clamp01(0.15f + 0.85f * UnityEngine.Mathf.Sin((h-6f)/24f * UnityEngine.Mathf.PI*2f) * 0.5f + 0.5f);
        }
    }
}