using UnityEngine;

namespace EtT.Systems.Draum
{
    [CreateAssetMenu(fileName = "DraumConfig", menuName = "EtT/Systems/Draum Config")]
    public class DraumConfig : ScriptableObject
    {
        [Header("Triggery")]
        [Range(0,1)] public float lowSanityThreshold01 = 0.28f;
        public bool triggerAfterCase = true;

        [Header("Efekty snu")]
        public int sanityDeltaOmen = -5;
        public int sanityDeltaBlessing = +4;
        public int baselineDelta = +1;

        [Header("Szanse (post-case)")]
        [Range(0,1)] public float chanceAfterCase = 0.4f;
    }
}