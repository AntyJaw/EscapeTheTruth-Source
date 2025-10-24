using UnityEngine;

namespace EtT.Data
{
    [CreateAssetMenu(fileName = "PlayerBalanceConfig", menuName = "EtT/Configs/PlayerBalance")]
    public class PlayerBalanceConfig : ScriptableObject
    {
        [Header("Punkty startowe i limity")]
        [Range(1,10)] public int startingAttributePoints = 5;
        [Range(5,20)] public int maxAttributeValue = 10;
        [Range(1,5)]  public int pointsPerLevel = 1;

        [Header("Soft cap (malejące korzyści powyżej tego progu)")]
        [Range(1,10)] public int softCap = 5;
        [Range(0.1f, 1f)] public float softCapDiminish = 0.5f;

        [Header("Przeliczniki efektów")]
        public float missionRadiusReductionPerPerceptionPct = 2.5f;
        public float evidenceDegradationReductionPerComposurePct = 2.0f;
        public float analysisBoostPerTechniquePct = 3.0f;

        
    }
}