using System;

namespace EtT.Core
{
    public static partial class GameEvents
    {
        // Player
        public static event Action<float> OnSanityChanged;
        public static event Action<float> OnReputationChanged;
        public static event Action<int>   OnHealthChanged;
        public static event Action<int>   OnEnergyChanged;
        public static event Action<int>   OnLevelChanged;

        // Missions
        public static event Action<Missions.Mission> OnMissionGenerated;
        public static event Action<Missions.Mission, Missions.MissionStatus> OnMissionStatusChanged;

        // POI
        public static event Action<string> OnPoiEntered;
        public static event Action<string> OnPoiLeft;
        public static event Action<string> OnPoiInteracted;

        // Evidence / Team / Link-13 / Localization
        public static event Action<string> OnLink13Message;
        public static event Action OnTeamUpdated;
        public static event Action<string> OnEvidenceReceived;
        public static event Action<string> OnCoordinatorPing;
        public static event Action<string> OnLanguageChanged;

        // World
        public static event Action<World.ZoneInfo, bool> OnZoneGate;

        public static void RaiseSanityChanged(float v)=>OnSanityChanged?.Invoke(v);
        public static void RaiseReputationChanged(float v)=>OnReputationChanged?.Invoke(v);
        public static void RaiseHealthChanged(int v)=>OnHealthChanged?.Invoke(v);
        public static void RaiseEnergyChanged(int v)=>OnEnergyChanged?.Invoke(v);
        public static void RaiseLevelChanged(int v)=>OnLevelChanged?.Invoke(v);

        public static void RaiseMissionGenerated(Missions.Mission m)=>OnMissionGenerated?.Invoke(m);
        public static void RaiseMissionStatusChanged(Missions.Mission m, Missions.MissionStatus s)=>OnMissionStatusChanged?.Invoke(m,s);

        public static void RaisePoiEntered(string id)=>OnPoiEntered?.Invoke(id);
        public static void RaisePoiLeft(string id)=>OnPoiLeft?.Invoke(id);
        public static void RaisePoiInteracted(string id)=>OnPoiInteracted?.Invoke(id);

        public static void RaiseLink13(string msg)=>OnLink13Message?.Invoke(msg);
        public static void RaiseTeamUpdated()=>OnTeamUpdated?.Invoke();
        public static void RaiseEvidenceReceived(string id)=>OnEvidenceReceived?.Invoke(id);
        public static void RaiseCoordinatorPing(string msg)=>OnCoordinatorPing?.Invoke(msg);
        public static void RaiseLanguageChanged(string code)=>OnLanguageChanged?.Invoke(code);

        public static void RaiseZoneGate(World.ZoneInfo z, bool enter)=>OnZoneGate?.Invoke(z, enter);
    }
}