using UnityEngine;

namespace EtT.World.Omur
{
    [CreateAssetMenu(fileName = "OmurConfig", menuName = "EtT/World/Omur Config")]
    public class OmurConfig : ScriptableObject
    {
        [Header("Wagi wpływu (0..1)")]
        [Range(0,1)] public float wSanity   = 0.45f;
        [Range(0,1)] public float wWeather  = 0.30f;
        [Range(0,1)] public float wReputation = 0.15f;
        [Range(0,1)] public float wTime     = 0.10f;

        [Header("Tension (akcja)")]
        [Range(0,1)] public float tensionRainBoost  = 0.35f;
        [Range(0,1)] public float tensionNightBoost = 0.20f;

        [Header("Zdarzenia impulsowe")]
        public float pulseHit      = 0.25f;
        public float pulseClue     = -0.15f;
        public float pulseRitual   = 0.15f;

        [Header("Wygładzanie (lerp)")]
        public float smooth = 0.15f;
    }
}