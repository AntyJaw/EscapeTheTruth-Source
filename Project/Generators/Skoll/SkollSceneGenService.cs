using UnityEngine;
using EtT.Missions;
using EtT.Services;
using EtT.Generators.SceneGen;
using EtT.World.Biomes;

namespace EtT.Generators.Skoll
{
    public sealed class SkollSceneGenService : ISceneGenService
    {
        public SceneBundle Generate(Mission mission, int seed)
        {
            var rng = new System.Random(seed);
            var bundle = new SceneBundle { seed = seed };

            var biomeSvc = ServiceLocator.Get<IBiomeService>();
            var wx = ServiceLocator.Get<IWeatherService>();
            var biome = biomeSvc != null ? biomeSvc.GetBiomeAt(mission.Center, verboseLog:true) : Biome.Rural;
            bool heavyRain = wx != null && wx.Rain01 >= 0.6f;

            Debug.Log($"[W.Y.R.D→Sköll] Compose scene for mission={mission.Id}, biome={biome}, rain={(heavyRain?1:0)}");

            // Ofiara
            bundle.objects.Add(new SceneObjectSpec {
                kind = SceneObjectKind.BodyMannequin,
                prefabId = "body_mannequin",
                localPos = new Vector3(RandRange(rng,-1.2f,1.2f), 0f, RandRange(rng,-1.2f,1.2f)),
                localEuler = new Vector3(0f, RandRange(rng,0,360), 0f),
                localScale = Vector3.one
            });

            // Dowody
            int required = Mathf.Max(1, mission.RequiredEvidence);
            int extras   = Mathf.Clamp(Mathf.RoundToInt(required * 0.5f), 0, 4);
            int total    = required + extras;

            for (int i=0;i<total;i++)
            {
                var kind = (SceneObjectKind)(rng.Next(0, 5));
                float r = Mathf.Lerp(1.5f, mission.RadiusMeters * 0.55f, Mathf.Pow((float)rng.NextDouble(), 0.7f));
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;

                bundle.objects.Add(new SceneObjectSpec {
                    kind = kind,
                    prefabId = PrefabIdFor(kind),
                    localPos = new Vector3(Mathf.Cos(a)*r, 0f, Mathf.Sin(a)*r),
                    localEuler = new Vector3(0f, RandRange(rng,0,360), 0f),
                    localScale = Vector3.one
                });
            }

            // Dekor wg biomu
            switch (biome)
            {
                case Biome.Urban:
                    bundle.objects.Add(new SceneObjectSpec { kind=SceneObjectKind.PropHut, prefabId="prop_canopy", localPos=RandDecorPos(rng, mission.RadiusMeters, 0.35f) });
                    bundle.objects.Add(new SceneObjectSpec { kind=SceneObjectKind.PropHut, prefabId="prop_streetlight", localPos=RandDecorPos(rng, mission.RadiusMeters, 0.5f) });
                    break;
                case Biome.Park:
                    bundle.objects.Add(new SceneObjectSpec { kind=SceneObjectKind.PropHut, prefabId="prop_bench", localPos=RandDecorPos(rng, mission.RadiusMeters, 0.6f) });
                    bundle.objects.Add(new SceneObjectSpec { kind=SceneObjectKind.PropHut, prefabId="prop_tree", localPos=RandDecorPos(rng, mission.RadiusMeters, 0.7f) });
                    break;
                case Biome.Forest:
                    bundle.objects.Add(new SceneObjectSpec { kind=SceneObjectKind.PropCaveRock, prefabId="prop_caverock", localPos=RandDecorPos(rng, mission.RadiusMeters, 0.5f) });
                    bundle.objects.Add(new SceneObjectSpec { kind=SceneObjectKind.PropHut, prefabId="prop_tree", localPos=RandDecorPos(rng, mission.RadiusMeters, 0.8f) });
                    break;
                case Biome.Rural:
                    bundle.objects.Add(new SceneObjectSpec { kind=SceneObjectKind.PropHut, prefabId="prop_shed", localPos=RandDecorPos(rng, mission.RadiusMeters, 0.4f) });
                    break;
            }

            if (heavyRain)
            {
                bundle.objects.Add(new SceneObjectSpec { kind=SceneObjectKind.PropHut, prefabId="prop_canopy", localPos=RandDecorPos(rng, mission.RadiusMeters, 0.25f) });
            }

            return bundle;
        }

        private static Vector3 RandDecorPos(System.Random rng, float radius, float factor)
        {
            float r = Mathf.Lerp(1.2f, radius * factor, (float)rng.NextDouble());
            float a = (float)rng.NextDouble() * Mathf.PI * 2f;
            return new Vector3(Mathf.Cos(a)*r, 0f, Mathf.Sin(a)*r);
        }

        private static float RandRange(System.Random r, float a, float b) => a + (float)r.NextDouble() * (b - a);

        private static string PrefabIdFor(SceneObjectKind k)
        {
            switch (k)
            {
                case SceneObjectKind.EvidencePhysical: return "evidence_physical";
                case SceneObjectKind.EvidenceUV:       return "evidence_uv";
                case SceneObjectKind.EvidenceEM:       return "evidence_em";
                case SceneObjectKind.EvidenceRunic:    return "evidence_runic";
                case SceneObjectKind.EvidencePsychic:  return "evidence_psychic";
                case SceneObjectKind.BodyMannequin:    return "body_mannequin";
                default: return "";
            }
        }
    }
}