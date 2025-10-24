using UnityEngine;

namespace EtT.Systems.Tradr
{
    [CreateAssetMenu(fileName = "TradrConfig", menuName = "EtT/Systems/Tradr Config")]
    public class TradrConfig : ScriptableObject
    {
        [Header("Progi globalne rytuału")]
        [Range(0,1)] public float requiredMood = 0.6f;      // z Ó.M.U.R.
        [Range(0,1)] public float requiredEmpathy = 0.55f;  // z Ó.M.U.R.
        public int    requiredKarma = 15;                    // z B.Ö.N.D.

        [Header("Id rytuału")]
        public string ritualId = "echo_of_truth_v1";
        public string ritualTitle = "Echo Prawdy";
        [TextArea(2,4)] public string ritualDesc = "Zbiorowy rezonans odsłania ukryte ślady.";
    }
}