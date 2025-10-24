using UnityEngine;

namespace EtT.Systems.Bond
{
    [CreateAssetMenu(fileName = "BondConfig", menuName = "EtT/Systems/Bond Config")]
    public class BondConfig : ScriptableObject
    {
        [Header("Wagi zapisu karmy")]
        public int okClueValue       = 2;
        public int badChoiceValue    = -3;
        public int abandonCaseValue  = -5;
        public int completeCaseValue = +8;

        [Header("Zanik w czasie (na sesję)")]
        public float decayPerSession = 0.05f;

        [Header("Progi efektów (sumaryczny karmaScore)")]
        public int omenThreshold      = -20;
        public int blessingThreshold  = +25;
    }
}