using System.Text.RegularExpressions;

namespace EtT.Witness
{
    public static class IntentLexicon
    {
        // Arcade: przekazujesz już intencję ("ALIBI" itd.)
        // Free text: mapujemy regexami
        public static string Map(string input)
        {
            if (string.IsNullOrEmpty(input)) return "SMALL_TALK";
            var s = input.ToLowerInvariant();

            if (s.Contains("alibi") || Regex.IsMatch(s, "(gdzie|byłeś|byłaś).*\\?")) return "ALIBI";
            if (s.Contains("ofi") || s.Contains("victim")) return "KNOW_VICTIM";
            if (s.Contains("noc") || s.Contains("22") || s.Contains("24")) return "WHERE_WAS_NIGHT";
            if (s.Contains("widz") || s.Contains("zauwa")) return "WHAT_SEEN";
            if (s.Contains("kto") || s.Contains("spraw")) return "WHO_DID_IT";
            return "SMALL_TALK";
        }
    }
}