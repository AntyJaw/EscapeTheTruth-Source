using System;
using UnityEngine;

namespace EtT.Evidence
{
    public enum EvidenceLayer
    {
        Physical,
        UV,
        EM,
        Runic,
        Psychic
    }

    [Serializable]
    public sealed class EvidenceItem
    {
        public string Id;
        public string DisplayName;
        public EvidenceLayer Layer;
        public bool Collected;
        public float Integrity; // 0..1 (1=pełne, 0=zniszczone)

        public EvidenceItem(string id, string displayName, EvidenceLayer layer)
        {
            Id = id;
            DisplayName = displayName;
            Layer = layer;
            Collected = false;
            Integrity = 1f;
        }
    }
}