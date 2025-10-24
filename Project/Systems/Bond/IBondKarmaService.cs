namespace EtT.Systems.Bond
{
    public interface IBondKarmaService
    {
        void Init();
        void Record(string tag, int value);   // "found_evidence", "lied_to_witness", ...
        int  Score { get; }
        void DecayOncePerSession();
        string Qualitative();                 // "omen", "neutral", "blessing"
    }
}