using EtT.Generators.SceneGen;

namespace EtT.Missions
{
    public enum MissionStatus { Pending, Active, Completed, Abandoned, Archived }

    public sealed class Mission
    {
        public string Id;
        public string Title;
        public string Description;

        // Geometria/zakres
        public World.Position Center;
        public float RadiusMeters;

        // Czas / wymagania
        public int CreatedUnixUtc;
        public int DeadlineUnixUtc;
        public int RequiredEvidence;

        // Stan
        public MissionStatus Status;
        public bool Started;

        // Paczka sceny (od Sköll)
        public SceneBundle Scene;

        // Raport końcowy
        public MissionReport LastReport;
    }

    public sealed class MissionReport
    {
        public string MissionId;
        public MissionStatus FinalStatus;
        public int FinishedUnixUtc;
        public int CollectedEvidence;
        public int TotalEvidenceAtStart;
        public int TimeSpentSeconds;
        public int XpGained;
        public float ReputationDelta;
        public int EnergyDelta;
        public int HealthDelta;
    }
}