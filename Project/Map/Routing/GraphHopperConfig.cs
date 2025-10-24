using UnityEngine;

namespace EtT.Map.Routing
{
    [CreateAssetMenu(fileName = "GraphHopperConfig", menuName = "EtT/Routing/GraphHopper Config")]
    public class GraphHopperConfig : ScriptableObject
    {
        [Header("Endpoint API (np. https://graphhopper.com/api/1/route)")]
        public string apiBase = "https://graphhopper.com/api/1/route";

        [Header("Klucz API (opcjonalnie na MVP)")]
        public string apiKey = "";

        [Header("Limit punktów polylinii (uprość jeśli zbyt gęsta)")]
        public int maxPolylinePoints = 256;

        [Header("Timeout (sekundy)")]
        public int timeoutSec = 10;
    }
}