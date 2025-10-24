using EtT.Services;
using EtT.Missions;
using EtT.Generators.SceneGen;

namespace EtT.Generators
{
    public readonly struct AnomalySample { public readonly float Intensity; public readonly int Seed; public AnomalySample(float i,int s){Intensity=i;Seed=s;} }

    public sealed class AnomalyService : IAnomalyService
    {
        private readonly System.Random _rng = new(12345);
        public AnomalySample Sample() => new AnomalySample((float)_rng.NextDouble(), _rng.Next());
    }

    /// <summary>
    /// Szata Moran – modulacja tytułu/opisu + zlecenie wygenerowania sceny (Sköll).
    /// </summary>
    public sealed class NarrativeWeaver : INarrativeWeaver
    {
        public void Modulate(Mission m, AnomalySample a, IPlayerService p)
        {
            // Narracyjne „zabarwienie”
            m.Title = a.Intensity > 0.7f ? "Echo Prawdy" : "Zniekształcenie";
            m.Description = "Zidentyfikuj źródło anomalii i zabezpiecz ślady.";
            if (p is EtT.Player.PlayerService pl && pl.Sanity < 30f)
                m.Description += " (Uwaga: niestabilna psyche)";

            // Zlecenie do Sköll: zbuduj paczkę sceny
            var skoll = ServiceLocator.Get<EtT.Generators.SceneGen.ISceneGenService>();
            if (skoll == null)
            {
                // jeśli nie zarejestrowano – zostaw scena==null (composer użyje fallbacku)
                return;
            }

            m.Scene = skoll.Generate(m, a.Seed ^ m.Id.GetHashCode());
        }
    }
}