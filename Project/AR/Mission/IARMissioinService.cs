namespace EtT.AR.Mission
{
    public interface IARMissionService
    {
        bool IsInAR { get; }
        bool IsReady { get; } // gotowy po osadzeniu anchoru
        void Init();
        void EnterARScene(EtT.Missions.Mission mission);
        void ExitARScene();
    }
}