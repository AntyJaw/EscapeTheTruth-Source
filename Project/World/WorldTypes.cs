namespace EtT.World
{
    public readonly struct Position { public readonly double Lat, Lng; public Position(double lat,double lng){Lat=lat;Lng=lng;} }
    public sealed class ZoneInfo { public string Name; public Position Center; public float RadiusMeters; }
}