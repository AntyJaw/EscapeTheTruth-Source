using UnityEngine;
using EtT.Services;

namespace EtT.NPC.Witness
{
    [System.Serializable]
    public struct DialogueTurn
    {
        public string playerUtterance;
        public string intent;
        public string response;
        public float truthScore;   // 0..1 (1 = prawda)
    }

    /// <summary>
    /// Steruje odpowiedziami świadka:
    /// - liczy szansę prawdy: honesty + reputacja gracza - strach/hostility,
    /// - zapisuje karmę (Bond): truthful +2, blatant_lie -3,
    /// - uderza w O.M.U.R. (pulse) po kłamstwie/prawdzie.
    /// </summary>
    public sealed class WitnessDialogueController : MonoBehaviour
    {
        [Header("Dane świadka")]
        public WitnessProfile profile;

        [Header("Parametry rozmowy (runtime)")]
        [Range(0,1)] public float lastTruthScore = 0.5f;

        System.Random _rng = new System.Random();

        public DialogueTurn AskArcade(string canonicalIntent)
        {
            return Process(canonicalIntent, canonicalIntent);
        }

        public DialogueTurn AskDetective(string freeText)
        {
            var intent = IntentLexicon.Map(freeText);
            return Process(freeText, intent);
        }

        private DialogueTurn Process(string userText, string intent)
        {
            if (!KnowledgeBase.Allowed(intent, profile))
            {
                return new DialogueTurn
                {
                    playerUtterance = userText,
                    intent = intent,
                    response = "Nie mogę o tym rozmawiać. To nie jest jeszcze ustalone.",
                    truthScore = 0f
                };
            }

            var player = ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService;
            float rep01 = player != null ? Mathf.Clamp01(player.Reputation / 100f) : 0.5f;
            float sanity01 = player != null ? Mathf.Clamp01(player.Sanity / 100f) : 0.8f;

            // szansa prawdy: bazowa uczciwość + wpływ reputacji - strach - wrogość
            float honesty = profile ? profile.honestyBase : 0.5f;
            float fear = profile ? profile.fearOfPerp : 0.2f;
            float hostility = profile ? profile.hostility : 0.1f;
            float attitude = profile ? (profile.attitudeToPlayer * 0.2f) : 0f; // -0.2..+0.2

            float truthChance = honesty + 0.25f * (rep01 - 0.5f) - 0.25f * fear - 0.2f * hostility + attitude;
            truthChance = Mathf.Clamp01(truthChance);

            bool tellsTruth = _rng.NextDouble() < truthChance;
            string resp = tellsTruth ? KnowledgeBase.TrueAnswer(intent, profile)
                                     : KnowledgeBase.MisleadingAnswer(intent, profile);
            float truth = tellsTruth ? 1f : 0f;

            lastTruthScore = truth;

            // Karmiczny zapis (Bond)
            var bond = ServiceLocator.Get<EtT.Systems.Bond.IBondKarmaService>();
            if (bond != null)
            {
                if (tellsTruth) bond.Record("witness_truthful", +2);
                else bond.Record("witness_blatant_lie", -3);
            }

            // Puls do O.M.U.R.
            var omur = ServiceLocator.Get<EtT.World.Omur.IOmurService>();
            omur?.Pulse(tellsTruth ? "clue" : "hit"); // prawda uspokaja, kłamstwo podbija napięcie

            return new DialogueTurn
            {
                playerUtterance = userText,
                intent = intent,
                response = resp,
                truthScore = truth
            };
        }
    }
}