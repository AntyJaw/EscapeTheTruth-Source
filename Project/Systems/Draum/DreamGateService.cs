using UnityEngine;
using EtT.Services;
using EtT.Core;

namespace EtT.Systems.Draum
{
    public sealed class DreamGateService : IDreamGateService
    {
        private readonly DraumConfig _cfg;
        private System.Random _rng = new();

        public DreamGateService(DraumConfig cfg = null) { _cfg = cfg ?? ScriptableObject.CreateInstance<DraumConfig>(); }
        public void Init(){}

        public void TryOpenAfterCase(int missionSeed)
        {
            if (!_cfg.triggerAfterCase) return;
            if (_rng.NextDouble() > _cfg.chanceAfterCase) return;
            Open("post_case", missionSeed);
        }

        public void TryOpenBySanity(float sanity01, int missionSeed)
        {
            if (sanity01 <= _cfg.lowSanityThreshold01)
                Open("low_sanity", missionSeed);
        }

        private void Open(string cause, int seed)
        {
            var bond   = ServiceLocator.Get<EtT.Systems.Bond.IBondKarmaService>();
            var player = ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService;

            GameEvents.RaiseDreamStart(new DreamContext{ cause=cause, missionSeed=seed });

            string q = bond?.Qualitative() ?? "neutral";
            var report = new DreamReport { outcome = q, deltaSanity = _cfg.baselineDelta };

            if (q == "omen") report.deltaSanity += _cfg.sanityDeltaOmen;
            else if (q == "blessing") report.deltaSanity += _cfg.sanityDeltaBlessing;

            if (player != null && report.deltaSanity != 0) player.AddSanity(report.deltaSanity);
            GameEvents.RaiseDreamEnd(report);
        }
    }
}