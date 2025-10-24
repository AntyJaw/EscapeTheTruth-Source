using System;
using UnityEngine;
using EtT.Evidence;

namespace EtT.Generators.Crime
{
    public enum EnvKind { Field, Cave }

    [Serializable]
    public class EnvParams
    {
        public EnvKind kind = EnvKind.Field;
        public float areaRadius = 6f; // promień „podwórka” sceny
    }

    [Serializable]
    public class HutParams
    {
        public Vector2 size = new Vector2(4f, 3f); // XZ
        public float height = 2.4f;
        public float roofHeight = 1.2f;
        public float doorWidth = 0.8f;
        public float doorHeight = 2.0f;
        public int windowCount = 2;
    }

    [Serializable]
    public class CaveParams
    {
        public float length = 8f;
        public float radius = 2f;
        public float noiseAmp = 0.6f;
        public float noiseFreq = 0.8f;
        public int sides = 16;
        public int segments = 24;
    }

    [Serializable]
    public class EvidenceSpawn
    {
        public string id = "evidence_generic";
        public string display = "Ślad";
        public EvidenceLayer layer = EvidenceLayer.Physical;
        public Vector3 localPos; // względem kompozytora
    }

    [Serializable]
    public class BodyParams
    {
        public Vector3 localPos = new Vector3(0f, 0.02f, 0f);
        public float rotationY = 0f; // obrócenie manekina
    }

    [CreateAssetMenu(fileName = "CrimeSceneSeed", menuName = "EtT/Crime/Scene Seed")]
    public class CrimeSceneSeed : ScriptableObject
    {
        [Header("Losowość/ID")]
        public int seed = 12345;

        [Header("Otoczenie")]
        public EnvParams env = new EnvParams();

        [Header("Chata")]
        public HutParams hut = new HutParams();

        [Header("Jaskinia")]
        public CaveParams cave = new CaveParams();

        [Header("Dowody (lokalne pozycje)")]
        public EvidenceSpawn[] evidences;

        [Header("Manekin-ofiara")]
        public BodyParams body = new BodyParams();
    }
}