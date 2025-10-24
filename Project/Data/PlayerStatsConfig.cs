using UnityEngine;

namespace EtT.Data
{
    [CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "EtT/Configs/PlayerStats")]
    public class PlayerStatsConfig : ScriptableObject
    {
        [Header("Progi szaleństwa")]
        public float HighSanityThreshold = 80f;
        public float LowSanityThreshold  = 25f;

        [Tooltip("Próg trwałego szaleństwa: liczba misji z wysokim poziomem szaleństwa utrzymywana przez 5 misji (z pamięci projektu).")]
        public int PermanentMadnessSpan = 5;
    }
}