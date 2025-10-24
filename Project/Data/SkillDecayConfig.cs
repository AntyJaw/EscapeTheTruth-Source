using UnityEngine;
using EtT.Skills;

namespace EtT.Data
{
    [CreateAssetMenu(fileName = "SkillDecayConfig", menuName = "EtT/Skills/Decay Config")]
    public class SkillDecayConfigAsset : ScriptableObject
    {
        public SkillDecayConfig config = new SkillDecayConfig();
    }
}