using System;

namespace EtT.Skills
{
    public enum SkillKind
    {
        // Personalne
        Spostrzegawczosc,
        Technika,
        Psyche,
        Rytualy,

        // Klasowe (przykłady)
        HackerGPS,
        Toksykologia,
        AnalizaSymboli,
        Interrogation
    }

    public enum XpChannel { Personal, Class, Esoteric }

    [Serializable]
    public class SkillState
    {
        public string id;
        public SkillKind kind;
        public int level;
        public int xp;
        public long lastUsedUnix;
        public bool isEsoteric;

        public void TouchUsed() => lastUsedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    [Serializable]
    public class SkillDecayConfig
    {
        public int daysToStartDecay = 14;
        public float levelPenaltyPerWeek = 0.2f; // -20%/tydz bez użycia
        public int minEffectiveLevel = 1;
    }
}