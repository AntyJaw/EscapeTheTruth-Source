using UnityEngine;

namespace EtT.World.Poi
{
    [System.Serializable]
    public struct Poi
    {
        public string id;
        public string name;
        public EtT.PoiKind kind;
        public double lat, lng;
        public float radiusMeters;
        public bool isMissionLocked;  // 🔒 nowa flaga

        public Poi(string id, string name, EtT.PoiKind kind, double lat, double lng, float radiusMeters = 20f)
        {
            this.id = id;
            this.name = name;
            this.kind = kind;
            this.lat = lat;
            this.lng = lng;
            this.radiusMeters = radiusMeters;
            this.isMissionLocked = false;
        }
    }
}