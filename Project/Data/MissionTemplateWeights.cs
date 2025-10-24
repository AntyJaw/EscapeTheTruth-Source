using UnityEngine;

namespace EtT.Data
{
    [CreateAssetMenu(fileName = "MissionTemplateWeights", menuName = "EtT/Configs/MissionWeights")]
    public class MissionTemplateWeights : ScriptableObject
    {
        [Range(0,1)] public float IllusionWeight = 0.3f;
        [Range(0,1)] public float TemptationWeight = 0.2f;
        [Range(0,1)] public float TrialWeight = 0.5f;
    }
}