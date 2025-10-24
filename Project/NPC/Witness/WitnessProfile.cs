using UnityEngine;

namespace EtT.NPC.Witness
{
    [CreateAssetMenu(fileName = "WitnessProfile", menuName = "EtT/NPC/Witness Profile")]
    public class WitnessProfile : ScriptableObject
    {
        [Header("Identyfikator (unikalny w projekcie)")]
        public string witnessId = "witness_001";

        [Header("Cechy psychologiczne 0..1")]
        [Range(0,1)] public float honestyBase   = 0.55f; // skłonność do mówienia prawdy
        [Range(0,1)] public float nervousness   = 0.35f;
        [Range(0,1)] public float hostility     = 0.15f;
        [Range(0,1)] public float knowledge     = 0.60f; // ile faktycznie „wie”
        [Range(0,1)] public float fearOfPerp    = 0.20f; // lęk przed sprawcą

        [Header("Nastrój wobec gracza -1..1 (na start)")]
        [Range(-1,1)] public float attitudeToPlayer = 0.0f;

        [Header("Zakres tematów (odblokowane intencje)")]
        public bool allowAlibi       = true;
        public bool allowWhereNight  = true;
        public bool allowKnowVictim  = true;
        public bool allowSuspicious  = true;
        public bool allowPhoneData   = false; // wymaga dowodu/pozwolenia itd.

        [Header("Blokady fabularne (opcjonalnie)")]
        public bool lockCriticalUntilClue = false;
    }
}