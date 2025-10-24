using UnityEngine;

namespace EtT.UI.Map.Compass
{
    /// <summary>
    /// Włącza kompas urządzenia i udostępnia wygładzony heading (0..360, 0 = północ).
    /// Działa też w Edytorze (fallback: integracja ruchu z GPS → heading z wektora ruchu).
    /// Umieść JEDEN w scenie lub w bootstrapie.
    /// </summary>
    public sealed class CompassDriver : MonoBehaviour
    {
        [Header("Wygładzanie")]
        [Range(0,1)] public float headingLerp = 0.15f;

        public static CompassDriver Instance { get; private set; }

        public bool HasHardware => SystemInfo.supportsGyroscope || SystemInfo.supportsAccelerometer;
        public bool CompassEnabled { get; private set; }
        public float HeadingDeg { get; private set; }  // 0..360 (0 = północ)

        Vector2 _lastLatLng;
        double _lastSimTime;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            TryEnableCompass();
        }

        void TryEnableCompass()
        {
            Input.compass.enabled = true;
            Input.location.Start();
            CompassEnabled = Input.compass.enabled;
        }

        void Update()
        {
#if UNITY_EDITOR
            SimulateHeadingFromGps();
#else
            if (CompassEnabled)
            {
                // trueHeading preferowane (uwzgl. deklinację); fallback do headingAccuracy < 0 ? .
                float raw = Input.compass.trueHeading;
                if (float.IsNaN(raw) || raw < 0 || raw > 360) raw = Input.compass.magneticHeading;
                if (float.IsNaN(raw) || raw < 0 || raw > 360) raw = HeadingDeg; // brak nowych danych
                HeadingDeg = SmoothAngle(HeadingDeg, raw, headingLerp);
            }
            else
            {
                SimulateHeadingFromGps();
            }
#endif
        }

        float SmoothAngle(float from, float to, float lerp)
        {
            // płynne przejście przez 0/360
            float delta = Mathf.DeltaAngle(from, to);
            return Mathf.Repeat(from + delta * Mathf.Clamp01(lerp), 360f);
        }

        void SimulateHeadingFromGps()
        {
            // heading z wektora ruchu (zmiany pozycji geograficznej), działa też w Edytorze
            var lat = PlayerPrefs.GetFloat("__sim_lat", 52.2297f);
            var lng = PlayerPrefs.GetFloat("__sim_lng", 21.0122f);
            var p = new Vector2(lat, lng);

            double t = Time.realtimeSinceStartupAsDouble;
            if (_lastSimTime <= 0) { _lastLatLng = p; _lastSimTime = t; return; }

            var d = p - _lastLatLng;
            if (d.sqrMagnitude > 1e-10f)
            {
                float a = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg; // uwaga: lat=y, lng=x → północ = 0
                if (a < 0) a += 360f;
                HeadingDeg = SmoothAngle(HeadingDeg, a, headingLerp);
            }

            _lastLatLng = p;
            _lastSimTime = t;
        }
    }
}