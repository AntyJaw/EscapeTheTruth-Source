using System.Collections.Generic;
using UnityEngine;
using EtT.Generators.SceneGen;

namespace EtT.Content
{
    [CreateAssetMenu(fileName = "PrefabRegistry", menuName = "EtT/Content/Prefab Registry")]
    public class PrefabRegistry : ScriptableObject
    {
        [System.Serializable] public struct Entry { public string id; public GameObject prefab; }

        [Header("Mapowanie prefabId → prefab")]
        public List<Entry> entries = new List<Entry>();

        public GameObject Resolve(SceneObjectKind kind, string idFallbackOrEmpty = "")
        {
            // 1) po id
            if (!string.IsNullOrWhiteSpace(idFallbackOrEmpty))
            {
                foreach (var e in entries) if (e.id == idFallbackOrEmpty) return e.prefab;
            }

            // 2) według kind – domyślne
            string k = kind switch
            {
                SceneObjectKind.EvidencePhysical => "evidence_physical",
                SceneObjectKind.EvidenceUV       => "evidence_uv",
                SceneObjectKind.EvidenceEM       => "evidence_em",
                SceneObjectKind.EvidenceRunic    => "evidence_runic",
                SceneObjectKind.EvidencePsychic  => "evidence_psychic",
                SceneObjectKind.BodyMannequin    => "body_mannequin",
                SceneObjectKind.PropHut          => "prop_hut",
                SceneObjectKind.PropCaveRock     => "prop_caverock",
                _ => ""
            };

            if (!string.IsNullOrEmpty(k))
            {
                foreach (var e in entries) if (e.id == k) return e.prefab;
            }

            return null;
        }
    }
}