using System.Collections.Generic;

namespace EtT.Map.Routing
{
    public sealed class RouteResult
    {
        public List<World.Position> points = new List<World.Position>();
        public float distanceMeters;
        public float durationSeconds;
        public string mode;
        public string source; // "graphhopper" / "fallback"
    }
}