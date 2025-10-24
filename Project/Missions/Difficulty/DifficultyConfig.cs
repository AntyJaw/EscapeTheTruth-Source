using UnityEngine;

namespace EtT.Missions.Difficulty
{
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "EtT/Missions/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Promień bazowy misji (m) przed modyfikatorami")]
        public float baseRadiusMeters = 120f;

        [Header("Wymagane dowody (bazowo)")]
        public int baseRequiredEvidence = 3;

        [Header("Czas na misję (minuty) bazowo")]
        public int baseMinutes = 45;

        [Header("Skalowanie po poziomie gracza")]
        public AnimationCurve radiusByLevel = AnimationCurve.Linear(1, 1.0f, 30, 1.25f);
        public AnimationCurve evidenceByLevel = AnimationCurve.Linear(1, 1.0f, 30, 1.5f);
        public AnimationCurve timeByLevel = AnimationCurve.Linear(1, 1.0f, 30, 1.2f);

        [Header("Skalowanie po skuteczności (0..1) z ostatnich spraw")]
        public AnimationCurve radiusBySuccess = AnimationCurve.Linear(0, 0.9f, 1, 1.2f);
        public AnimationCurve evidenceBySuccess = AnimationCurve.Linear(0, 0.9f, 1, 1.4f);
        public AnimationCurve timeBySuccess = AnimationCurve.Linear(0, 1.1f, 1, 0.9f);

        [Header("Mobilność (tryb poruszania)")]
        [Tooltip("Mnożnik promienia przy chodzeniu.")]
        public float mobilityWalk = 0.85f;
        [Tooltip("Mnożnik promienia przy rowerze.")]
        public float mobilityBike = 1.05f;
        [Tooltip("Mnożnik promienia przy samochodzie.")]
        public float mobilityCar = 1.2f;

        [Header("Zaokrąglenia / klamry")]
        public float minRadiusMeters = 60f;
        public float maxRadiusMeters = 450f;
        public int minEvidence = 2;
        public int maxEvidence = 8;
        public int minMinutes = 20;
        public int maxMinutes = 90;
    }
}