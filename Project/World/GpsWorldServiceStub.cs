using UnityEngine;

namespace EtT.World
{
    public sealed class GpsWorldServiceStub : IGpsWorldService
    {
        private Position _pos = new Position(52.2297, 21.0122);
        public void Start() {}
        public void Stop() {}

        public Position GetPlayerPosition() => _pos;
        public double AccuracyMeters => 5.0;
        public bool IsAvailable => true;

        public ZoneInfo[] GetRestrictedZones() => _restricted;

        public bool IsAllowed(Position p)
        {
            foreach (var z in _restricted)
            {
                if (Haversine(p.Lat,p.Lng,z.Center.Lat,z.Center.Lng) < z.RadiusMeters) return false;
            }
            return true;
        }

    #if UNITY_EDITOR
        public void SetSimulatedPosition(double lat, double lng, double accuracyMeters = 5.0)
        {
            _pos = new Position(lat,lng);
        }
    #endif

        private readonly ZoneInfo[] _restricted = {
            new ZoneInfo{ Name="Szpital", Center = new Position(52.231,21.005), RadiusMeters=120f }
        };

        private static float Haversine(double lat1,double lon1,double lat2,double lon2)
        {
            const double R=6371000.0;
            double dLat=(lat2-lat1)*Mathf.Deg2Rad, dLon=(lon2-lon1)*Mathf.Deg2Rad;
            double a=System.Math.Sin(dLat/2)*System.Math.Sin(dLat/2)+System.Math.Cos(lat1*Mathf.Deg2Rad)*System.Math.Cos(lat2*Mathf.Deg2Rad)*System.Math.Sin(dLon/2)*System.Math.Sin(dLon/2);
            double c=2*System.Math.Atan2(System.Math.Sqrt(a),System.Math.Sqrt(1-a));
            return (float)(R*c);
        }
    }
}