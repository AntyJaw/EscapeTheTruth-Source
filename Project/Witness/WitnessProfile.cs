using UnityEngine;

namespace EtT.Witness
{
    [CreateAssetMenu(fileName = "WitnessProfile", menuName = "EtT/Witness/Profile")]
    public class WitnessProfile : ScriptableObject
    {
        public string witnessId = "npc_001";
        [Range(0,1)] public float honestyBase = 0.6f;
        [Range(0,1)] public float nervousness = 0.4f;
        [Range(0,1)] public float hostility = 0.2f;
        [Range(0,1)] public float knowledgeScope = 0.6f;
        [Range(0,1)] public float fearOfPerp = 0.3f;
        [Range(-1,1)] public float attitudeToPlayer = 0f;
    }
}