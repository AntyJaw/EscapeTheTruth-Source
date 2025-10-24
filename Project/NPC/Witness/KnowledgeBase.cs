using UnityEngine;

namespace EtT.NPC.Witness
{
    public static class KnowledgeBase
    {
        /// <summary>Generuje odpowiedź „prawdziwą” (na potrzeby MVP – losowo z wariantów).</summary>
        public static string TrueAnswer(string intent, WitnessProfile p)
        {
            switch (intent)
            {
                case IntentLexicon.INTENT_ALIBI:
                    return "Byłem w domu, sam. Oglądałem mecz, mogę pokazać historię przeglądarki.";
                case IntentLexicon.INTENT_WHERE_NIGHT:
                    return "Wyszłam około 22:15 z baru przy rynku. Potem prosto do domu, trasa parkiem.";
                case IntentLexicon.INTENT_KNOW_VICTIM:
                    return "Znałem go z pracy – nie gadaliśmy wiele. Ostatnio był podenerwowany.";
                case IntentLexicon.INTENT_SUSPICIOUS:
                    return "Widziałem czarny sedan krążący kilka razy. Siedział w nim ktoś w kapturze.";
                case IntentLexicon.INTENT_PHONE_DATA:
                    return "Telefon miał wyłączone GPS. Ale widziałem go z powerbankiem, ładował często.";
                default:
                    return "Nie jestem pewien. Mogę spróbować sobie przypomnieć.";
            }
        }

        /// <summary>Generuje odpowiedź mylącą/niepełną.</summary>
        public static string MisleadingAnswer(string intent, WitnessProfile p)
        {
            switch (intent)
            {
                case IntentLexicon.INTENT_ALIBI:
                    return "Byłem... chyba z kolegą. Może. Nie pamiętam dokładnie godziny.";
                case IntentLexicon.INTENT_WHERE_NIGHT:
                    return "To było dawno, nie pamiętam. Wyszedłem i... wróciłem późno.";
                case IntentLexicon.INTENT_KNOW_VICTIM:
                    return "Nie znałem go. Może kiedyś mignął mi na osiedlu.";
                case IntentLexicon.INTENT_SUSPICIOUS:
                    return "Nie, nic podejrzanego. Wszystko normalnie.";
                case IntentLexicon.INTENT_PHONE_DATA:
                    return "Nie mam telefonu przy sobie. Nie pamiętam PIN-u. Przepraszam.";
                default:
                    return "Nie wiem o co chodzi.";
            }
        }

        /// <summary>Sprawdza, czy temat jest dostępny dla profilu (blokady fabularne).</summary>
        public static bool Allowed(string intent, WitnessProfile p)
        {
            if (!p) return true;
            return intent switch
            {
                IntentLexicon.INTENT_ALIBI        => p.allowAlibi,
                IntentLexicon.INTENT_WHERE_NIGHT  => p.allowWhereNight,
                IntentLexicon.INTENT_KNOW_VICTIM  => p.allowKnowVictim,
                IntentLexicon.INTENT_SUSPICIOUS   => p.allowSuspicious,
                IntentLexicon.INTENT_PHONE_DATA   => p.allowPhoneData,
                _ => true
            };
        }
    }
}