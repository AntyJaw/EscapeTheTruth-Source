using UnityEngine;

namespace EtT.World.Biomes
{
    public interface IBiomeService
    {
        Biome GetBiomeAt(EtT.World.Position pos, bool verboseLog = false);
    }

    /// <summary>
    /// Heurystyka MVP: gęstość POI i „charakter” najbliższych punktów.
    /// • dużo Cafe/Store/Police → Urban
    /// • Bench/Base → Park (jeśli dominuje)
    /// • Ritual → Forest (jeśli dominuje)
    /// • brak POI → Rural
    /// </summary>
    public sealed class BiomeService : IBiomeService
    {
        public Biome GetBiomeAt(EtT.World.Position pos, bool verboseLog = false)
        {
            var poiSvc = EtT.Services.ServiceLocator.Get<EtT.IPoiService>() as EtT.World.Poi.PoiService;
            if (poiSvc == null)
            {
                if (verboseLog) Debug.Log("[W.Y.R.D][Biome] No POI service → fallback Rural.");
                return Biome.Rural;
            }

            int urbanScore = 0, parkScore = 0, forestScore = 0;

            var urbanA = poiSvc.NearestIncludingLocked(pos, EtT.PoiKind.Cafe, 5);
            var urbanB = poiSvc.NearestIncludingLocked(pos, EtT.PoiKind.Store, 5);
            var urbanC = poiSvc.NearestIncludingLocked(pos, EtT.PoiKind.Police, 3);
            urbanScore += urbanA.Length + urbanB.Length + urbanC.Length;

            var parkA = poiSvc.NearestIncludingLocked(pos, EtT.PoiKind.Bench, 6);
            var parkB = poiSvc.NearestIncludingLocked(pos, EtT.PoiKind.Base, 2);
            parkScore += parkA.Length + Mathf.Max(0, parkB.Length - 1);

            var forestA = poiSvc.NearestIncludingLocked(pos, EtT.PoiKind.Ritual, 4);
            forestScore += forestA.Length;

            Biome biome;
            if (urbanScore >= Mathf.Max(3, parkScore + forestScore)) biome = Biome.Urban;
            else if (forestScore > parkScore) biome = Biome.Forest;
            else if (parkScore > 0) biome = Biome.Park;
            else biome = Biome.Rural;

            if (verboseLog)
            {
                Debug.Log($"[W.Y.R.D][Biome] @({pos.Lat:0.000000},{pos.Lng:0.000000}) → {biome}  (urban={urbanScore}, park={parkScore}, forest={forestScore})");
            }

            return biome;
        }
    }
}