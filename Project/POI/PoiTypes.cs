using System;
using UnityEngine;
using EtT.Map;
using EtT.PlayerClasses;

namespace EtT.POI
{
    [Flags] public enum PoiLayer { GPS=1<<0, AR=1<<1, Runic=1<<2, System=1<<3, Dynamic=1<<4 }

    [Serializable]
    public struct PoiRequirements
    {
        [Header("Wejście / dostęp")]
        public bool requiresEsoteric;
        public CharacterClass requiredClass; // 0 = żadna
        [Range(0,100)] public int minEzoter; // ezoteryka (0..100)
        [Range(0,100)] public int minPsychika;
        [Range(0,100)] public int minEnergy;
        [Tooltip("Jeżeli >0 - dostęp tylko od tego poziomu gracza.")]
        public int minPlayerLevel;
    }

    [Serializable]
    public struct PoiEffect
    {
        [Header("Modyfikatory pasków 0..100")]
        public int healthDelta;
        public int energyDelta;
        public int psycheDelta;
        public float reputationDelta;

        [Header("Nagrody/XP")]
        public int classXp;
        public int personalXp;
        public int esotericXp;

        [Header("Inne")]
        public string narrativeTag; // np. „coffee_with_informant”
        public float cooldownSeconds; // anti-spam
    }

    // Wynik interakcji zwracany przez serwis
    public struct PoiResult
    {
        public bool ok;
        public string message;
    }

    // Instancja POI w świecie (runtime)
    [Serializable]
    public class PoiInstance
    {
        public string id;
        public string name;
        public PoiType type;
        public double lat;
        public double lng;
        public float radiusMeters;
        public PoiLayer layers;
        public PoiRequirements req;
        public PoiEffect effect;

        public Vector2d LatLng => new(lat, lng);
    }

    // Pomocnicza krotka do pozycji double
    public readonly struct Vector2d { public readonly double x,y; public Vector2d(double x,double y){this.x=x; this.y=y;} }
}