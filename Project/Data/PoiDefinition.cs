using UnityEngine;
using EtT.Map;

namespace EtT.POI
{
    [CreateAssetMenu(fileName = "PoiDefinition", menuName = "EtT/POI/Definition")]
    public class PoiDefinition : ScriptableObject
    {
        public string poiId = "poi_id";
        public string displayName = "Nowy POI";
        public PoiType type = PoiType.Cafe;
        public float defaultRadius = 25f;
        public PoiLayer layers = PoiLayer.GPS;
        public PoiRequirements requirements;
        public PoiEffect effect;
    }

    [CreateAssetMenu(fileName = "PoiSet", menuName = "EtT/POI/Set")]
    public class PoiSet : ScriptableObject
    {
        [Tooltip("Lista definicji możliwych do spawnowania w scenie/regionie.")]
        public PoiDefinition[] items;
    }
}