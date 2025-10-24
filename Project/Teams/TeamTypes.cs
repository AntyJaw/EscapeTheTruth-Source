using System;
using System.Collections.Generic;

namespace EtT.Teams
{
    public enum PlatformKind { Mobile, Steam, Hybrid }
    public enum TeamRole { AgentTerenowy, Koordynator, Analityk, Informatyk, Ezoteryk }

    [Serializable]
    public class Member
    {
        public string userId;
        public string displayName;
        public PlatformKind platform;
        public TeamRole role;
        public bool ready;
        public int psyche;      // snapshot pasków
        public int energy;
        public int reputation;
    }

    [Serializable]
    public class Synergy
    {
        public float compatibility; // 0..1
        public int successes;
        public int conflicts;
        public float cooperationScore; // rolling
    }

    [Serializable]
    public class Team
    {
        public string teamId;
        public string name;
        public List<Member> members = new();
        public Synergy synergy = new();
        public string coordinatorUserId; // Steam/PC
        public DateTime updatedAt;
    }

    // ====== Evidence flow MVP ======
    [Serializable]
    public class EvidencePacket
    {
        public string id;          // guid
        public string teamId;
        public string fromUserId;
        public string kind;        // photo/audio/text/dna/…
        public string metaJson;    // np. {gps, timestamp, tags}
        public byte[] payload;     // thumbnail / bytes (MVP: może być null – tylko meta)
        public long unixTime;
    }

    // ====== Snapshots for cloud ====
    [Serializable]
    public class TeamSnapshot
    {
        public Team team;
    }
}