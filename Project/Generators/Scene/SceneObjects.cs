using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtT.Generators.SceneGen
{
    public enum SceneObjectKind
    {
        EvidencePhysical,
        EvidenceUV,
        EvidenceEM,
        EvidenceRunic,
        EvidencePsychic,
        BodyMannequin,
        PropHut,        // placeholder chaty
        PropCaveRock    // placeholder jaskini
    }

    [Serializable]
    public struct SceneObjectSpec
    {
        public SceneObjectKind kind;
        public string prefabId;     // klucz do PrefabRegistry (opcjonalny – gdy pusty, composer użyje domyślnego dla kind)
        public Vector3 localPos;    // lokalnie względem „kotwicy sceny” (AR anchor) w metrach
        public Vector3 localEuler;  // rotacja lokalna (stopnie)
        public Vector3 localScale;  // 1,1,1 domyślnie
    }

    [Serializable]
    public sealed class SceneBundle
    {
        public int seed;
        public List<SceneObjectSpec> objects = new List<SceneObjectSpec>();
    }
}