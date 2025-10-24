namespace EtT.Systems.Draum
{
    public interface IDreamGateService
    {
        void Init();
        void TryOpenAfterCase(int missionSeed);
        void TryOpenBySanity(float sanity01, int missionSeed);
    }
}