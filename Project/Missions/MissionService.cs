using System.Collections.Generic;
using System.Linq;
using EtT.Core;
using EtT.Services;
using EtT.Missions.Difficulty;

namespace EtT.Missions
{
    /// <summary>
    /// Serwis misji: generowanie, statusy, archiwizacja, nagrody/kary.
    /// Center → W.Y.R.D. (IWyrdService). Scena → Sköll (SceneGen) przez NarrativeWeaver.
    /// Promień/timer/dowody → DifficultyTuner (+ mody atrybutów gracza).
    ///
    /// Dodatkowo:
    /// - B.Ö.N.D. (Bond) zapisuje karmę przy zakończeniu/porzuceniu.
    /// - D.R.A.U.M. (DreamGate) może otworzyć sen po sprawie lub przy niskiej psyche.
    /// - Telemetry zapisuje outcome (dla W.Y.R.D. boostów).
    /// </summary>
    public sealed class MissionService : IMissionService
    {
        private readonly List<Mission> _active  = new();
        private readonly List<Mission> _archive = new();

        private const string SAVE_ACTIVE  = "missions_active";
        private const string SAVE_ARCHIVE = "missions_archive";

        public void Init() => Load();

        public Mission GenerateDaily()
        {
            var anomaly = ServiceLocator.Get<IAnomalyService>().Sample();
            var player  = ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService;
            var gps     = ServiceLocator.Get<IGpsWorldService>();
            var time    = ServiceLocator.Get<ITimeService>();

            var nowUnix  = time.NowUnixUtc();
            var playerPos = gps.GetPlayerPosition();

            // --- SKALOWANIE TRUDNOŚCI ---
            var dcfg = UnityEngine.Resources.Load<DifficultyConfig>("DifficultyConfig")
                       ?? UnityEngine.ScriptableObject.CreateInstance<DifficultyConfig>();
            float success = DifficultyTuner.SuccessRatio(_archive.ToArray(), lastN: 10);

            var mobility = DifficultyTuner.InferMobility(); // Walk/Bike/Car (na bazie ekwipunku)
            int level    = player != null ? player.Level : 1;

            var tune = DifficultyTuner.Compute(dcfg, level, success, mobility);

            // --- WYBÓR LOKALIZACJI: W.Y.R.D. ---
            var wyrd = ServiceLocator.Get<EtT.Generators.Wyrd.IWyrdService>();
            var wMob = mobility switch
            {
                DifficultyTuner.MobilityKind.Walk => EtT.Generators.Wyrd.IWyrdService.MobilityKind.Walk,
                DifficultyTuner.MobilityKind.Bike => EtT.Generators.Wyrd.IWyrdService.MobilityKind.Bike,
                _                                  => EtT.Generators.Wyrd.IWyrdService.MobilityKind.Car
            };

            var target = wyrd != null
                ? wyrd.ChooseCrimeCenter(playerPos, wMob, level)
                : OffsetPosition(playerPos, 600.0 + Rand01()*1200.0, Rand01()*360f); // fallback

            // --- PROMIEŃ (z atrybutami gracza) ---
            float radius = tune.radiusMeters;
            if (player != null) radius = player.MissionRadiusModifier(radius);

            // --- KONSTRUKCJA MISJI ---
            var m = new Mission
            {
                Id               = System.Guid.NewGuid().ToString("N"),
                Title            = "Zniekształcenie",
                Description      = "Zidentyfikuj źródło anomalii i zabezpiecz ślady.",
                Center           = target,
                RadiusMeters     = radius,
                CreatedUnixUtc   = nowUnix,
                DeadlineUnixUtc  = nowUnix + tune.minutes * 60,
                RequiredEvidence = tune.requiredEvidence,
                Status           = MissionStatus.Pending,
                Started          = false
            };

            // Narracja (Moran) → dołoży SceneBundle via Sköll
            ServiceLocator.Get<INarrativeWeaver>().Modulate(m, anomaly, player);

            _active.Add(m);
            GameEvents.RaiseMissionGenerated(m);
            Persist();
            return m;
        }

        public void SetStatus(Mission m, MissionStatus status)
        {
            if (m == null) return;
            m.Status = status;
            GameEvents.RaiseMissionStatusChanged(m, status);

            if (status == MissionStatus.Completed || status == MissionStatus.Abandoned)
                FinalizeAndArchive(m);
        }

        // === API ===
        public IReadOnlyList<Mission> Active  => _active;
        public IReadOnlyList<Mission> Archive => _archive;

        public void StartMission(Mission m)
        {
            if (m == null || m.Started || m.Status != MissionStatus.Pending) return;
            m.Started = true;
            m.Status  = MissionStatus.Active;
            GameEvents.RaiseMissionStatusChanged(m, MissionStatus.Active);
            Persist();
        }

        public void CompleteMission(Mission m)
        {
            if (m == null || m.Status != MissionStatus.Active) return;
            m.Status = MissionStatus.Completed;
            FinalizeAndArchive(m);
        }

        public void AbandonMission(Mission m, bool toArchive = true)
        {
            if (m == null || (m.Status != MissionStatus.Pending && m.Status != MissionStatus.Active)) return;
            m.Status = MissionStatus.Abandoned;
            FinalizeAndArchive(m);
        }

        // === Raport / Archiwum + integracje: Bond / Draum / Telemetry ===
        private void FinalizeAndArchive(Mission m)
        {
            var timeSvc     = ServiceLocator.Get<ITimeService>();
            var evidenceSvc = ServiceLocator.Get<EtT.Evidence.EvidenceService>();
            int now         = timeSvc.NowUnixUtc();

            int collected = 0;
            int total     = 0;
            if (evidenceSvc != null)
            {
                var list = evidenceSvc.Active;
                total = list.Count;
                foreach (var ev in list) if (ev.Collected) collected++;
            }

            var player = ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService;
            int   xp  = 0;
            float rep = 0;
            int   en  = 0;
            int   hp  = 0;

            if (m.Status == MissionStatus.Completed)
            {
                xp = 50 + (int)(10 * collected);
                rep = +2.0f;
                en = -10;
            }
            else if (m.Status == MissionStatus.Abandoned)
            {
                xp = 5;
                rep = -1.5f;
                en = -5;
            }

            if (player != null)
            {
                if (xp  != 0) player.AddXp(xp);
                if (rep != 0) player.AddReputation(rep);
                if (en  != 0) player.AddEnergy(en);
                if (hp  != 0) player.AddHealth(hp);
            }

            m.LastReport = new MissionReport
            {
                MissionId             = m.Id,
                FinalStatus           = m.Status,
                FinishedUnixUtc       = now,
                CollectedEvidence     = collected,
                TotalEvidenceAtStart  = total,
                TimeSpentSeconds      = System.Math.Max(0, now - m.CreatedUnixUtc),
                XpGained              = xp,
                ReputationDelta       = rep,
                EnergyDelta           = en,
                HealthDelta           = hp
            };

            // === B.Ö.N.D. – zapis karmy ===
            var bond = ServiceLocator.Get<EtT.Systems.Bond.IBondKarmaService>();
            if (bond != null)
            {
                if (m.Status == MissionStatus.Completed) bond.Record("case_completed", +8);
                else if (m.Status == MissionStatus.Abandoned) bond.Record("case_abandoned", -5);
            }

            // === D.R.A.U.M. – potencjalne otwarcie snu ===
            var draum = ServiceLocator.Get<EtT.Systems.Draum.IDreamGateService>();
            if (draum != null)
            {
                int seed = SafeMissionSeed(m);
                // po sprawie (losowo wg configu)
                draum.TryOpenAfterCase(seed);

                // oraz – jeśli niska psyche – natychmiastowa próba (na podstawie aktualnej psyche gracza)
                float sanity01 = player != null ? UnityEngine.Mathf.Clamp01(player.Sanity / 100f) : 1f;
                draum.TryOpenBySanity(sanity01, seed);
            }

            // === Telemetria (dla W.Y.R.D. boostów w przyszłości) ===
            var tel = ServiceLocator.Get<ITelemetryService>();
            tel?.RecordMissionOutcome(m);

            // === przenosimy do archiwum ===
            m.Status = MissionStatus.Archived;
            _active.Remove(m);
            _archive.Add(m);
            GameEvents.RaiseMissionStatusChanged(m, MissionStatus.Archived);

            Persist();
        }

        private void Load()
        {
            var save = ServiceLocator.Get<ISaveService>();
            var a = save.Load<List<Mission>>(SAVE_ACTIVE);
            var b = save.Load<List<Mission>>(SAVE_ARCHIVE);
            if (a != null) _active.AddRange(a);
            if (b != null) _archive.AddRange(b);
        }

        private void Persist()
        {
            var save = ServiceLocator.Get<ISaveService>();
            save.Store(SAVE_ACTIVE,  _active);
            save.Store(SAVE_ARCHIVE, _archive);
        }

        private static int SafeMissionSeed(Mission m)
        {
            unchecked
            {
                int h1 = m.Id != null ? m.Id.GetHashCode() : 0;
                int h2 = m.CreatedUnixUtc;
                return (h1 * 486187739) ^ h2;
            }
        }

        private static double Rand01() => UnityEngine.Random.value;

        private static World.Position OffsetPosition(World.Position origin, double meters, double bearingDeg)
        {
            double R = 6371000.0;
            double br = bearingDeg * UnityEngine.Mathf.Deg2Rad;

            double lat1 = origin.Lat * UnityEngine.Mathf.Deg2Rad;
            double lon1 = origin.Lng * UnityEngine.Mathf.Deg2Rad;

            double lat2 = System.Math.Asin(System.Math.Sin(lat1) * System.Math.Cos(meters / R) +
                                           System.Math.Cos(lat1) * System.Math.Sin(meters / R) * System.Math.Cos(br));
            double lon2 = lon1 + System.Math.Atan2(System.Math.Sin(br) * System.Math.Sin(meters / R) * System.Math.Cos(lat1),
                                                   System.Math.Cos(meters / R) - System.Math.Sin(lat1) * System.Math.Sin(lat2));

            return new World.Position(lat2 * UnityEngine.Mathf.Rad2Deg, lon2 * UnityEngine.Mathf.Rad2Deg);
        }
    }
}