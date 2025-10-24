using EtT.Core;
using EtT.Services;
using EtT.PlayerStats;
using EtT.PlayerClasses;
using EtT.Data;
using EtT.Skills;
using EtT.SkillSystem;
using UnityEngine;

namespace EtT.Player
{
    public sealed class PlayerService : IPlayerService
    {
        private const string KEY_PREFIX = "player_";
        private string Key => KEY_PREFIX + Services.ServiceLocator.Get<IProfilesService>().ActiveId;

        [SerializeField] private PlayerBalanceConfig _balance;
        private readonly SkillService _skillService = new SkillService();

        // Płeć/klasa
        private GenderType _gender;
        private CharacterClass _class = CharacterClass.Agent;

        // Paski
        private int _health = 80;
        private int _psychika = 100;
        private int _energia = 80;
        private float _reputation = 50f;
        private int _ezoteryka = 0;
        private bool _ezoterUnlocked = false;

        // Postęp
        private int _level = 1;
        private int _xp = 0;
        private int _classXp = 0;
        private int _personalXp = 0;
        private int _esotericXp = 0;

        // Atrybuty
        private Attributes _attributes;
        private int _unspentPoints = 0;

        // ==== Properties ====
        public float Sanity => _psychika;
        public float Reputation => _reputation;
        public int Health => _health;
        public int Energy => _energia;
        public int Ezoteryka => _ezoteryka;
        public int Level => _level;
        public int Xp => _xp;
        public Attributes Attr => _attributes;
        public int UnspentPoints => _unspentPoints;
        public CharacterClass Class => _class;
        public GenderType Gender => _gender;
        public bool EsotericUnlocked => _ezoterUnlocked;

        public void Init()
        {
            if (_balance == null)
            {
                _balance = Resources.Load<PlayerBalanceConfig>("PlayerBalanceConfig");
                if (_balance == null) _balance = ScriptableObject.CreateInstance<PlayerBalanceConfig>();
            }

            var decayAsset = Resources.Load<SkillDecayConfigAsset>("SkillDecayConfig");
            _skillService.Init(decayAsset ? decayAsset.config : null);

            Load();

            GameEvents.RaiseSanityChanged(_psychika);
            GameEvents.RaiseReputationChanged(_reputation);
            GameEvents.RaiseEnergyChanged(_energia);
            GameEvents.RaiseHealthChanged(_health);
            GameEvents.RaiseLevelChanged(_level);
            GameEvents.RaiseAttributesChanged();

            GameEvents.OnActiveProfileChanged += _=>Load();
        }

        // === Kreacja postaci ===
        public void SelectClass(ClassDefinition def, GenderType gender)
        {
            _class = def.id switch
            {
                "agent" => CharacterClass.Agent,
                "laborant" => CharacterClass.Laborant,
                "informatyk" => CharacterClass.Informatyk,
                "antropolog" => CharacterClass.Antropolog,
                "ezoteryk" => CharacterClass.Ezoteryk,
                _ => CharacterClass.Agent
            };
            _gender = gender;

            // Bazowe
            _health = def.startHealth;
            _psychika = def.startPsychika;
            _energia = def.startEnergia;
            _reputation = def.startReputacja;

            _attributes = new Attributes
            {
                Perception = def.perception,
                Technique  = def.technique,
                Composure  = def.composure
            };

            // Bonusy płci (subtelne)
            if (_gender == GenderType.Male)      { _health += 5;      _attributes.Composure += 1; }
            else /* Female */                    { _psychika += 5;    _attributes.Perception += 1; }

            // Punkty do rozdania
            _unspentPoints = Mathf.Max(0, _balance.startingAttributePoints);

            // Umiejętności klasowe
            if (def.classSkillIds != null)
                foreach (var id in def.classSkillIds)
                    _skillService.Ensure(id, MapClassSkillIdToKind(id), esoteric:false);

            Persist();
            EmitAll();
        }

        private static Skills.SkillKind MapClassSkillIdToKind(string id)
        {
            return id switch
            {
                "hacker_gps"     => Skills.SkillKind.HackerGPS,
                "lab_toxicology" => Skills.SkillKind.Toksykologia,
                "symbols"        => Skills.SkillKind.AnalizaSymboli,
                "interrogation"  => Skills.SkillKind.Interrogation,
                _                => Skills.SkillKind.Technika
            };
        }

        public void UnlockEsoteric(){ _ezoterUnlocked = true; Persist(); }

        // === XP / Level ===
        public void AddXp(int amount){ _xp += amount; HandleLevel(); Persist(); }
        public void AddClassXp(int amount){ _classXp += amount; AddXp(amount/2); Persist(); }
        public void AddPersonalXp(int amount){ _personalXp += amount; AddXp(amount/2); Persist(); }
        public void AddEsotericXp(int amount){ if (_ezoterUnlocked){ _esotericXp += amount; AddXp(amount/2); Persist(); } }

        private void HandleLevel()
        {
            while (_xp >= XpToNext())
            {
                _xp -= XpToNext();
                _level++;
                _unspentPoints += _balance.pointsPerLevel;
                GameEvents.RaiseLevelChanged(_level);
            }
        }

        // === Paski ===
        public void AddSanity(float d){ _psychika = Mathf.Clamp(_psychika + (int)d, 0, 100); GameEvents.RaiseSanityChanged(_psychika); Persist(); }
        public void AddReputation(float d){ _reputation = Mathf.Clamp(_reputation + d, 0f, 100f); GameEvents.RaiseReputationChanged(_reputation); Persist(); }
        public void AddHealth(int d){ _health = Mathf.Clamp(_health + d, 0, 100); GameEvents.RaiseHealthChanged(_health); Persist(); }
        public void AddEnergy(int d){ _energia = Mathf.Clamp(_energia + d, 0, 100); GameEvents.RaiseEnergyChanged(_energia); Persist(); }
        public void AddEzoteryka(int d){ if (_ezoterUnlocked){ _ezoteryka = Mathf.Clamp(_ezoteryka + d, 0, 100); Persist(); } }

        // === Przydział punktów ===
        public bool AllocatePoint(EtT.PlayerStats.AttributeType type)
        {
            if (_unspentPoints <= 0) return false;
            var v = Mathf.Min(_balance.maxAttributeValue, _attributes.Get(type) + 1);
            if (v == _attributes.Get(type)) return false;
            _attributes.Set(type, v);
            _unspentPoints--;
            GameEvents.RaiseAttributesChanged(); Persist(); return true;
        }

        public bool RefundPoint(EtT.PlayerStats.AttributeType type)
        {
            var v = _attributes.Get(type);
            if (v <= 0) return false;
            _attributes.Set(type, v-1);
            _unspentPoints++;
            GameEvents.RaiseAttributesChanged(); Persist(); return true;
        }

        public void RespecAll()
        {
            _unspentPoints += _attributes.Perception + _attributes.Technique + _attributes.Composure;
            _attributes = new Attributes();
            GameEvents.RaiseAttributesChanged(); Persist();
        }

        // === Efektywne atrybuty (soft cap) ===
        private float EffectiveAttr(int raw)
        {
            if (_balance == null) return raw;
            if (raw <= _balance.softCap) return raw;
            var above = raw - _balance.softCap;
            return _balance.softCap + above * _balance.softCapDiminish;
        }

        // === Modyfikatory używają "efektywnych" wartości ===
        public float MissionRadiusModifier(float baseRadiusMeters)
        {
            float effPer = EffectiveAttr(_attributes.Perception);
            float reductionPct = _balance.missionRadiusReductionPerPerceptionPct * effPer;
            float factor = 1f - (reductionPct / 100f);
            return Mathf.Max(10f, baseRadiusMeters * Mathf.Clamp(factor, 0.5f, 1f));
        }

        public float EvidenceDegradationModifier(float baseFactor)
        {
            float effComp = EffectiveAttr(_attributes.Composure);
            float reductionPct = _balance.evidenceDegradationReductionPerComposurePct * effComp;
            float factor = 1f - (reductionPct / 100f);
            return baseFactor * Mathf.Clamp(factor, 0.5f, 1f);
        }

        public float AnalysisBoostPercent()
        {
            float effTech = EffectiveAttr(_attributes.Technique);
            return _balance.analysisBoostPerTechniquePct * effTech;
        }

        // === Skills ===
        public void GainSkillXp(string id, Skills.SkillKind kind, int amount, Skills.XpChannel channel, bool esoteric=false)
            => _skillService.GainXp(id, kind, amount, channel, esoteric);
        public float GetSkillEffectiveLevel(string id) => _skillService.EffectiveLevel(id);

        public void ResetDailyFlags() { Persist(); }

        // === Save/Load ===
        private void Load()
        {
            var save = ServiceLocator.Get<ISaveService>();
            var data = save.Load<PlayerProfile>(Key);
            if (data == null)
            {
                _health = 80; _psychika = 100; _energia = 80; _reputation = 50;
                _level = 1; _xp = 0; _classXp=0; _personalXp=0; _esotericXp=0;
                _attributes = new Attributes(); _unspentPoints = _balance != null ? _balance.startingAttributePoints : 5;
                _class = CharacterClass.Agent; _ezoterUnlocked=false; _ezoteryka=0;
                _gender = GenderType.Male;
                Persist();
            }
            else
            {
                _health = data.Health; _psychika = data.Psychika; _energia = data.Energia;
                _reputation = data.Reputation; _ezoteryka = data.Ezoteryka;
                _level = data.Level; _xp = data.Xp; _classXp=data.ClassXp; _personalXp=data.PersonalXp; _esotericXp=data.EsotericXp;
                _attributes = data.Attr; _unspentPoints = data.UnspentPoints;
                _class = data.Class; _ezoterUnlocked = data.EzotericUnlocked;
                _gender = data.Gender;
            }
        }

        private void Persist()
        {
            ServiceLocator.Get<ISaveService>().Store(Key, new PlayerProfile{
                Health=_health, Psychika=_psychika, Energia=_energia,
                Reputation=_reputation, Ezoteryka=_ezoteryka,
                Level=_level, Xp=_xp, ClassXp=_classXp, PersonalXp=_personalXp, EsotericXp=_esotericXp,
                Attr=_attributes, UnspentPoints=_unspentPoints,
                Class=_class, EzotericUnlocked=_ezoterUnlocked, Gender=_gender
            });
        }

        private int XpToNext()=> 100 + (_level-1)*50;

        [System.Serializable]
        private class PlayerProfile
        {
            public int Health;
            public int Psychika;
            public int Energia;
            public float Reputation;
            public int Ezoteryka;
            public int Level;
            public int Xp;
            public int ClassXp;
            public int PersonalXp;
            public int EsotericXp;
            public Attributes Attr;
            public int UnspentPoints;
            public CharacterClass Class;
            public bool EzotericUnlocked;
            public GenderType Gender;
        }

        private void EmitAll()
        {
            GameEvents.RaiseAttributesChanged();
            GameEvents.RaiseEnergyChanged(_energia);
            GameEvents.RaiseHealthChanged(_health);
            GameEvents.RaiseSanityChanged(_psychika);
            GameEvents.RaiseReputationChanged(_reputation);
            GameEvents.RaiseLevelChanged(_level);
        }
    }
}