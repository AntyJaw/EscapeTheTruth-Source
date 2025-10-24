using System;
using System.Text.RegularExpressions;

namespace EtT.NPC.Witness
{
    public static class IntentLexicon
    {
        // Kanoniczne intencje – używaj takich kluczy w UI Arcade
        public const string INTENT_ALIBI         = "ALIBI";
        public const string INTENT_WHERE_NIGHT   = "WHERE_WAS_NIGHT";
        public const string INTENT_KNOW_VICTIM   = "KNOW_VICTIM";
        public const string INTENT_SUSPICIOUS    = "SUSPICIOUS_PEOPLE";
        public const string INTENT_PHONE_DATA    = "PHONE_DATA";

        /// <summary>Mapowanie swobodnego tekstu (tryb Detektyw) do intencji.</summary>
        public static string Map(string userText)
        {
            if (string.IsNullOrWhiteSpace(userText)) return INTENT_ALIBI;
            var s = userText.Trim().ToLowerInvariant();

            if (Regex.IsMatch(s, @"alibi|gdzie (byłeś|byłaś)|co robiłeś|co robiłaś")) return INTENT_ALIBI;
            if (Regex.IsMatch(s, @"noc|22|23|24|północ|wieczor")) return INTENT_WHERE_NIGHT;
            if (Regex.IsMatch(s, @"ofiara|znałeś|znałaś|znaliście|kim (on|ona)|relacja")) return INTENT_KNOW_VICTIM;
            if (Regex.IsMatch(s, @"podejrzan|dziwn|obcy|kręcił|kręciła|zachowanie")) return INTENT_SUSPICIOUS;
            if (Regex.IsMatch(s, @"telefon|sms|połączen|lokalizacj|gps|dane")) return INTENT_PHONE_DATA;

            return INTENT_ALIBI;
        }
    }
}