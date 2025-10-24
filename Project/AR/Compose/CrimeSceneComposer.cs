using UnityEngine;
using EtT.Services;

namespace EtT.AR.Compose
{
    /// <summary>
    /// Buduje scenę dowodową na bazie SceneBundle zwróconego przez Sköll.
    /// Mapuje prefabId → rzeczywisty prefab z Resources/EtT/AR/Props/.
    /// Dla MVP: w razie braku prefabu – tworzy prymityw.
    /// </summary>
    public sealed class CrimeSceneComposer : MonoBehaviour
    {
        public void Compose(EtT.Missions.Mission mission)
        {
            if (mission == null) { Debug.LogWarning("[AR] Composer: mission null"); return; }

            var skoll = ServiceLocator.Get<EtT.Generators.SceneGen.ISceneGenService>();
            var seed  = SafeMissionSeed(mission);
            var bundle = skoll.Generate(mission, seed);

            foreach (var spec in bundle.objects)
            {
                var go = SpawnFor(spec);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = spec.localPos;
                go.transform.localEulerAngles = spec.localEuler;
                go.transform.localScale = spec.localScale;

                // Podpinamy kontroler widoczności dowodów (shader parametry) – opcjonalnie
                var rend = go.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    var ctrl = go.AddComponent<EtT.AR.Compose.EvidenceVisibilityController>();
                    ctrl.target = rend;
                    ctrl.baseVisibility = 1f;
                    ctrl.decayPerMinute = 0.02f;
                }
            }

            Debug.Log($"[AR] CrimeSceneComposer: built {bundle.objects.Count} objects (seed={seed}).");
        }

        private static int SafeMissionSeed(EtT.Missions.Mission m)
        {
            unchecked
            {
                int h1 = m.Id != null ? m.Id.GetHashCode() : 0;
                int h2 = m.CreatedUnixUtc;
                return (h1 * 486187739) ^ h2;
            }
        }

        private static GameObject SpawnFor(EtT.Generators.SceneGen.SceneObjectSpec spec)
        {
            // Spróbuj pobrać z Resources (np. Resources/EtT/AR/Props/body_mannequin)
            if (!string.IsNullOrEmpty(spec.prefabId))
            {
                var res = Resources.Load<GameObject>($"EtT/AR/Props/{spec.prefabId}");
                if (res) return Object.Instantiate(res);
            }

            // MVP fallback: generuj proste bryły
            PrimitiveType type = PrimitiveType.Cube;
            switch (spec.kind)
            {
                case EtT.Generators.SceneGen.SceneObjectKind.BodyMannequin: type = PrimitiveType.Capsule; break;
                case EtT.Generators.SceneGen.SceneObjectKind.EvidencePhysical: type = PrimitiveType.Cube; break;
                case EtT.Generators.SceneGen.SceneObjectKind.EvidenceUV: type = PrimitiveType.Sphere; break;
                case EtT.Generators.SceneGen.SceneObjectKind.EvidenceEM: type = PrimitiveType.Cylinder; break;
                case EtT.Generators.SceneGen.SceneObjectKind.EvidenceRunic: type = PrimitiveType.Sphere; break;
                case EtT.Generators.SceneGen.SceneObjectKind.EvidencePsychic: type = PrimitiveType.Capsule; break;
                default: type = PrimitiveType.Cube; break;
            }
            var go = GameObject.CreatePrimitive(type);
            go.name = $"AR_{spec.kind}";
            return go;
        }
    }
}