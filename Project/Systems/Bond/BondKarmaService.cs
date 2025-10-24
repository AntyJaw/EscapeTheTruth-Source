using System.Collections.Generic;
using UnityEngine;
using EtT.Services;

namespace EtT.Systems.Bond
{
    public sealed class BondKarmaService : IBondKarmaService
    {
        private const string KEY = "bond_karma";
        private readonly BondConfig _cfg;
        private int _score;
        private List<StringEntry> _log = new();

        public int Score => _score;

        [System.Serializable] private class State { public int score; public List<StringEntry> log; }
        [System.Serializable] private class StringEntry { public string v; public StringEntry(string s){ v=s; } }

        public BondKarmaService(BondConfig cfg = null) { _cfg = cfg ?? ScriptableObject.CreateInstance<BondConfig>(); }

        public void Init()
        {
            var data = ServiceLocator.Get<ISaveService>().Load<State>(KEY);
            if (data != null) { _score = data.score; _log = data.log ?? new(); }
        }

        public void Record(string tag, int value)
        {
            _score += value;
            _log.Add(new StringEntry($"{tag}:{value}"));
            Persist();
            // Debug.Log($"[B.Ö.N.D.] Record {tag}={value}, total={_score}");
        }

        public void DecayOncePerSession()
        {
            _score = Mathf.RoundToInt(_score * (1f - _cfg.decayPerSession));
            Persist();
        }

        public string Qualitative()
        {
            if (_score <= _cfg.omenThreshold) return "omen";
            if (_score >= _cfg.blessingThreshold) return "blessing";
            return "neutral";
        }

        private void Persist()
        {
            ServiceLocator.Get<ISaveService>().Store(KEY, new State{ score=_score, log=_log });
        }
    }
}