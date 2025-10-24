using UnityEngine;

namespace EtT.World.Poi
{
    [System.Serializable]
    public class PoiInstance
    {
        public string Id;
        public string Name;
        public PoiType Type;
        public double Lat;
        public double Lng;

        public float DistanceMeters; // wyliczane runtime
    }
}