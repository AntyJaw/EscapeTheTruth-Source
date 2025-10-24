namespace EtT
{
    // ========== SAVE ==========
    public interface ISaveService
    {
        void Init();
        void Save();
        T Load<T>(string key) where T : class, new();
        void Store<T>(string key, T data) where T : class;
    }

    // ========== TIME ==========
    public interface ITimeService
    {
        int  NowUnixUtc();
        float DeltaTime { get; }  // używane w kilku miejscach jako Time.deltaTime
        bool IsNightLocal();      // stub – możesz później podmienić na realne słońce/astronomię
    }

    // ========== WEATHER (OMUR) ==========
    public interface IWeatherService
    {
        float Humidity01 { get; }   // 0..1
        float Rain01 { get; }       // 0..1
        float Light01 { get; }      // 0..1 (dzień/noc/chmury)
        float TemperatureC { get; }
        float WindMS { get; }
        void  Tick(float dt);
    }

    // ========== PLAYER ==========
    public interface IPlayerService
    {
        void Init();

        // paski / status
        float Sanity { get; }
        float Reputation { get; }
        int   Health { get; }
        int   Energy { get; }
        int   Ezoteryka { get; }
        int   Level { get; }

        void AddSanity(float delta);
        void AddReputation(float delta);
        void AddHealth(int delta);
        void AddEnergy(int delta);

        // XP / skills (część kodu to wywołuje)
        void AddClassXp(int amount);
        void AddPersonalXp(int amount);
        void AddEsotericXp(int amount);
        float GetSkillEffectiveLevel(string id);
        void GainSkillXp(string id, Skills.SkillKind kind, int amount, Skills.XpChannel channel, bool esoteric=false);

        // wybory startowe
        bool EsotericUnlocked { get; }
        PlayerClasses.CharacterClass Class { get; }
    }

    // ========== MISSIONS ==========
    public interface IMissionService
    {
        void Init();
        Missions.Mission GenerateDaily();
        void SetStatus(Missions.Mission m, Missions.MissionStatus status);
        // (opcjonalnie w przyszłości: void StartMission(Mission m); void TickTimers();)
    }

    // ========== GENERATORS ==========
    public interface IAnomalyService { Generators.AnomalySample Sample(); }
    public interface INarrativeWeaver
    {
        void Modulate(Missions.Mission m, Generators.AnomalySample a, IPlayerService p);
    }

    // ========== WORLD / GPS ==========
    public interface IGpsWorldService
    {
        void Start();
        void Stop();

        World.Position GetPlayerPosition();
        double AccuracyMeters { get; }
        bool IsAvailable { get; }

        World.ZoneInfo[] GetRestrictedZones();
        bool IsAllowed(World.Position p);

    #if UNITY_EDITOR
        void SetSimulatedPosition(double lat, double lng, double accuracyMeters = 5.0);
    #endif
    }

    // ========== POI ==========
    public interface IPoiService
    {
        void UnlockAll();
        void LockInArea(World.Position center, float radiusMeters);

        // szukanie – używane w UI
        (double lat,double lng,string name)[] NearestIncludingLocked(World.Position from, PoiKind kind, int limit=10);
        (double lat,double lng,string name)[] Nearest(World.Position from, PoiKind kind, int limit=10);

        // uogólnione:
        (double lat,double lng,string name)[] FindByType(PoiKind kind);
        (double lat,double lng,string name)  FindNearest(World.Position from, PoiKind kind);

        // minimalna lista POI
        System.Collections.Generic.IReadOnlyList<(double lat,double lng,string name,PoiKind kind,bool locked)> All { get; }
    }

    public enum PoiKind { Cafe, Pharmacy, Bench, Grocery, Ritual, CrimeScene, Link13, Base, Police, Cemetery, TeamSpot, ClassLocked, Anomaly }

    // ========== LINK-13 ==========
    public interface ILink13Service { void SendSystem(string msg); }
}