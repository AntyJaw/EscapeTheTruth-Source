using System.Linq;
using UnityEngine;
using EtT.Services;

namespace EtT.Missions.Difficulty
{
    public struct MissionTuning
    {
        public float radiusMeters;
        public int requiredEvidence;
        public int minutes;
    }

    /// <summary>
    /// Wylicza tuning misji: promień, dowody, czas — z uwzględnieniem profilu gracza (poziom),
    /// skuteczności (success ratio z archiwum) i mobilności (walk/bike/car).
    /// </summary>
    public static class DifficultyTuner
    {
        public static MissionTuning Compute(DifficultyConfig cfg, int playerLevel, float success01, MobilityKind mobility)
        {
            float r = cfg.baseRadiusMeters;
            int ev  = cfg.baseRequiredEvidence;
            int min = cfg.baseMinutes;

            // Level
            r  *= Mathf.Clamp(cfg.radiusByLevel.Evaluate(playerLevel),   0.5f, 2.0f);
            ev = Mathf.RoundToInt(ev * Mathf.Clamp(cfg.evidenceByLevel.Evaluate(playerLevel), 0.5f, 2.0f));
            min = Mathf.RoundToInt(min * Mathf.Clamp(cfg.timeByLevel.Evaluate(playerLevel), 0.5f, 2.0f));

            // Success
            r  *= Mathf.Clamp(cfg.radiusBySuccess.Evaluate(success01),   0.5f, 2.0f);
            ev = Mathf.RoundToInt(ev * Mathf.Clamp(cfg.evidenceBySuccess.Evaluate(success01), 0.5f, 2.0f));
            min = Mathf.RoundToInt(min * Mathf.Clamp(cfg.timeBySuccess.Evaluate(success01), 0.5f, 2.0f));

            // Mobility
            switch (mobility)
            {
                case MobilityKind.Walk: r *= cfg.mobilityWalk; break;
                case MobilityKind.Bike: r *= cfg.mobilityBike; break;
                case MobilityKind.Car:  r *= cfg.mobilityCar;  break;
            }

            // Klamry
            r   = Mathf.Clamp(r, cfg.minRadiusMeters, cfg.maxRadiusMeters);
            ev  = Mathf.Clamp(ev, cfg.minEvidence, cfg.maxEvidence);
            min = Mathf.Clamp(min, cfg.minMinutes, cfg.maxMinutes);

            return new MissionTuning { radiusMeters = r, requiredEvidence = ev, minutes = min };
        }

        /// <summary> Success ratio z archiwum: Completed / (Completed+Abandoned) w ostatnich N (domyślnie 10). </summary>
        public static float SuccessRatio(EtT.Missions.Mission[] archive, int lastN = 10)
        {
            if (archive == null || archive.Length == 0) return 0.5f;
            var last = archive.Reverse().Take(lastN).ToArray();
            int done = last.Count(m => m.LastReport != null && m.LastReport.FinalStatus == MissionStatus.Completed);
            int tot  = last.Count(m => m.LastReport != null &&
                                       (m.LastReport.FinalStatus == MissionStatus.Completed || m.LastReport.FinalStatus == MissionStatus.Abandoned));
            if (tot <= 0) return 0.5f;
            return Mathf.Clamp01(done / (float)tot);
        }

        public enum MobilityKind { Walk, Bike, Car }

        /// <summary> Wnioskowanie mobilności na podstawie ekwipunku (MVP). </summary>
        public static MobilityKind InferMobility()
        {
            var inv = ServiceLocator.Get<IInventoryService>();
            if (inv == null) return MobilityKind.Walk;
            if (inv.QuantityOf("mob_car") > 0)  return MobilityKind.Car;
            if (inv.QuantityOf("mob_bike") > 0) return MobilityKind.Bike;
            return MobilityKind.Walk;
        }
    }
}