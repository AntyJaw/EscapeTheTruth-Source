namespace EtT.Witness
{
    public static class KnowledgeBase
    {
        public static string TrueAnswer(string intent, WitnessProfile p)
        {
            switch (intent)
            {
                case "ALIBI": return "Byłem w domu, sam. Oglądałem serial do północy.";
                case "KNOW_VICTIM": return "Znałem ją z osiedla, mówiła mi się kiedyś o kłótniach z kimś z pracy.";
                case "WHERE_WAS_NIGHT": return "Między 22 a 24 byłem w mieszkaniu na Wrzosowej.";
                case "WHAT_SEEN": return "Widziałem czarny samochód, stary model, kręcił się po ulicy.";
                case "WHO_DID_IT": return "Nie wiem. Ale słyszałem krzyk i trzask szkła.";
                default: return "Nie wiem, co jeszcze mogę powiedzieć…";
            }
        }

        public static string MisleadingAnswer(string intent, WitnessProfile p)
        {
            switch (intent)
            {
                case "ALIBI": return "Eee… wyszedłem z psem, ale nie pamiętam kiedy.";
                case "KNOW_VICTIM": return "Nie kojarzę, mało z kim rozmawiam.";
                case "WHERE_WAS_NIGHT": return "Byłem… gdzieś w centrum, nie pamiętam dokładnie.";
                case "WHAT_SEEN": return "Nic nie widziałem. Nic.";
                case "WHO_DID_IT": return "Na pewno ktoś z zewnątrz. Nikt z nas.";
                default: return "Nie mam pojęcia.";
            }
        }
    }
}