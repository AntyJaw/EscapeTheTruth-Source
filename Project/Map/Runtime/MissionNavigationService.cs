using System;
using System.Collections.Generic;
using System.Text;
using EtT.World;

namespace EtT.Map
{
    public sealed class MissionNavigationService : INavigationService
    {
        private Position _target;
        private float _radiusM;
        private Position _player;
        private readonly List<Poi> _pois = new();

        public double DistanceToTargetMeters { get; private set; }
        public bool IsInsideMissionArea { get; private set; }

        public void SetMissionTarget(Position center, float radiusMeters)
        {
            _target = center; _radiusM = radiusMeters;
            Recompute();
        }

        public void SetPois(IEnumerable<Poi> pois)
        {
            _pois.Clear();
            if (pois != null) _pois.AddRange(pois);
        }

        public void UpdatePlayerPos(Position p)
        {
            _player = p; Recompute();
        }

        private void Recompute()
        {
            DistanceToTargetMeters = HaversineMeters(_player, _target);
            IsInsideMissionArea = DistanceToTargetMeters <= _radiusM;
        }

        private static double HaversineMeters(Position a, Position b)
        {
            double R = 6371000.0;
            double dLat = Deg2Rad(b.Lat - a.Lat);
            double dLon = Deg2Rad(b.Lng - a.Lng);
            double la1 = Deg2Rad(a.Lat);
            double la2 = Deg2Rad(b.Lat);
            double sinDlat = Math.Sin(dLat/2);
            double sinDlon = Math.Sin(dLon/2);
            double t = sinDlat*sinDlat + Math.Cos(la1)*Math.Cos(la2)*sinDlon*sinDlon;
            double c = 2 * Math.Atan2(Math.Sqrt(t), Math.Sqrt(1-t));
            return R * c;
        }
        private static double Deg2Rad(double d) => d * Math.PI / 180.0;

        public string BuildExternalMapsUrl(bool appleMapsPreferred)
        {
#if UNITY_IOS
            appleMapsPreferred = true;
#endif
            var lat = _target.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var lng = _target.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (appleMapsPreferred) return $"http://maps.apple.com/?daddr={lat},{lng}&dirflg=d";
            return $"https://www.google.com/maps/dir/?api=1&destination={lat},{lng}&travelmode=walking";
        }
    }
}