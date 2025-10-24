using UnityEngine;

namespace EtT.AR.Flairs
{
    [CreateAssetMenu(fileName = "FlairConfig", menuName = "EtT/AR/Flair Config")]
    public class FlairConfig : ScriptableObject
    {
        [Header("Włącz/wyłącz konkretne flairy")]
        public bool flair_001 = true;
        public bool flair_002 = true;
        public bool flair_003 = true;
        public bool flair_004 = true;
        public bool flair_005 = true;
        public bool flair_006 = true;
        public bool flair_007 = true;
        public bool flair_008 = true;
        public bool flair_009 = true;
    }
}