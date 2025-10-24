using UnityEngine;

namespace EtT.Generators.Wyrd
{
    [CreateAssetMenu(fileName = "WyrdConfig", menuName = "EtT/Generators/Wyrd Config")]
    public class WyrdConfig : ScriptableObject
    {
        [Header("Dystans od gracza (metry) wg mobilności")]
        public Vector2 walkDistanceRange = new Vector2(250, 900);
        public Vector2 bikeDistanceRange = new Vector2(600, 2000);
        public Vector2 carDistanceRange  = new Vector2(1200, 5000);

        [Header("Modyfikator dystansu wg poziomu (1..30 → 0.9..1.3)")]
        public AnimationCurve distanceByLevel = AnimationCurve.Linear(1, 0.9f, 30, 1.3f);

        [Header("Sampling")]
        [Tooltip("Ile prób wyznaczenia sensownego punktu, zanim użyjemy fallbacku")]
        public int maxSamples = 64;

        [Header("Reguły świata")]
        [Tooltip("Minimalna odległość od strefy restricted (metry)")]
        public float minDistanceFromRestricted = 80f;

        [Header("Bias po potrzebach gracza")]
        public bool biasByPlayerNeeds = true;
        public float needPoiSearchRadius = 600f;

        [Header("Bias pogodowy / pora doby")]
        public bool biasByWeatherAndTime = true;
        [Range(0,1)] public float rainBiasThreshold = 0.6f;
        [Range(0,1)] public float nightBiasLightThreshold = 0.35f;
        [Range(0,1)] public float biasBearingShare = 0.7f;
        [Range(1,90)] public float biasBearingSpreadDeg = 45f;

        [Header("Telemetria (uczenie heurystyczne)")]
        public bool useTelemetry = true;
        public float telemetryInfluenceRadius = 500f;
        [Range(0,1)] public float telemetryAcceptanceBoost = 0.35f;
    }
}