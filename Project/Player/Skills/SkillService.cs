using System;
using System.Collections.Generic;
using EtT.Skills;
using EtT.Services;
using UnityEngine;

namespace EtT.SkillSystem
{
    public sealed class SkillService
    {
        private const string KEY_PREFIX = "skills_";
        private string Key => KEY_PREFIX + ServiceLocator.Get<IProfilesService>().ActiveId;

        private readonly Dictionary<string, SkillState> _skills = new();
        private SkillDecayConfig _decay = new SkillDecayConfig();

        public void Init(SkillDecayConfig decayConfig = null)
        {
            if (decayConfig != null) _decay = decayConfig;
            Load();
            EtT.Core.GameEvents.OnActiveProfileChanged += _ => Load();
        }

        public IReadOnlyDictionary<string, SkillState> Skills => _skills;

        public SkillState Ensure(string id, SkillKind kind, bool esoteric=false)
        {
            if (_skills.TryGetValue(id, out var s)) return s;
            s = new SkillState { id=id, kind=kind, level=1, xp=0, lastUsedUnix=Now(), isEsoteric=esoteric };
            _skills[id] = s; Persist(); return s;
        }

        public void GainXp(string id, SkillKind kind, int amount, XpChannel channel, bool esoteric=false)
        {
            var s = Ensure(id, kind, esoteric);
            if (channel == XpChannel.Esoteric && !s.isEsoteric) return;

            s.xp += Mathf.Max(0, amount);
            while (s.xp >= 100 * s.level) { s.xp -= 100 * s.level; s.level++; }
            s.TouchUsed(); Persist();
        }

        public float EffectiveLevel(string id)
        {
            if (!_skills.TryGetValue(id, out var s)) return 0f;
            var days = (Now() - s.lastUsedUnix) / 86400f;
            if (days <= _decay.daysToStartDecay) return s.level;
            var weeks = (days - _decay.daysToStartDecay) / 7f;
            var penalty = 1f - Mathf.Clamp01(_decay.levelPenaltyPerWeek * weeks);
            return Mathf.Max(_decay.minEffectiveLevel, s.level * penalty);
        }

        public void Touch(string id) { if (_skills.TryGetValue(id, out var s)) { s.TouchUsed(); Persist(); } }

        private void Load()
        {
            var data = ServiceLocator.Get<ISaveService>().Load<Wrapper>(Key) ?? new Wrapper();
            _skills.Clear();
            if (data.items != null) foreach (var s in data.items) _skills[s.id] = s;
        }
        private void Persist() => ServiceLocator.Get<ISaveService>().Store(Key, new Wrapper{ items = new List<SkillState>(_skills.Values).ToArray() });
        private static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        [Serializable] private class Wrapper { public SkillState[] items; }
    }
}