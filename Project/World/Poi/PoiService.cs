using System.Collections.Generic;
using System.Linq;

namespace EtT.World.Poi
{
    public sealed class PoiService : IPoiService
    {
        private readonly List<(double lat,double lng,string name,PoiKind kind,bool locked)> _all =
            new()
            {
                (52.229,21.011,"Kawiarnia A", PoiKind.Cafe, false),
                (52.228,21.013,"Apteka B",   PoiKind.Pharmacy, false),
                (52.227,21.012,"Ławka C",    PoiKind.Bench, false),
            };

        public IReadOnlyList<(double lat,double lng,string name,PoiKind kind,bool locked)> All => _all;

        public void UnlockAll()
        {
            for (int i=0;i<_all.Count;i++) _all[i] = (_all[i].lat,_all[i].lng,_all[i].name,_all[i].kind,false);
        }

        public void LockInArea(World.Position center, float radiusMeters)
        {
            for (int i=0;i<_all.Count;i++)
            {
                var d = Haversine(center.Lat, center.Lng, _all[i].lat, _all[i].lng);
                bool lockIt = d <= radiusMeters;
                _all[i] = (_all[i].lat,_all[i].lng,_all[i].name,_all[i].kind, lockIt);
            }
        }

        public (double lat,double lng,string name)[] NearestIncludingLocked(World.Position from, PoiKind kind, int limit=10)
        {
            return _all.Where(p=>p.kind==kind)
                       .OrderBy(p=>Haversine(from.Lat,from.Lng,p.lat,p.lng))
                       .Take(limit)
                       .Select(p=>(p.lat,p.lng,p.name)).ToArray();
        }

        public (double lat,double lng,string name)[] Nearest(World.Position from, PoiKind kind, int limit=10)
        {
            return _all.Where(p=>p.kind==kind && !p.locked)
                       .OrderBy(p=>Haversine(from.Lat,from.Lng,p.lat,p.lng))
                       .Take(limit)
                       .Select(p=>(p.lat,p.lng,p.name)).ToArray();
        }

        public (double lat,double lng,string name)[] FindByType(PoiKind kind)
            => _all.Where(p=>p.kind==kind).Select(p=>(p.lat,p.lng,p.name)).ToArray();

        public (double lat,double lng,string name) FindNearest(World.Position from, PoiKind kind)
        {
            return Nearest(from, kind, 1).FirstOrDefault();
        }

        private static double Haversine(double lat1,double lon1,double lat2,double lon2)
        {
            const double R=6371000.0;
            double dLat=(lat2-lat1)*System.Math.PI/180.0, dLon=(lon2-lon1)*System.Math.PI/180.0;
            double a=System.Math.Sin(dLat/2)*System.Math.Sin(dLat/2)+System.Math.Cos(lat1*System.Math.PI/180.0)*System.Math.Cos(lat2*System.Math.PI/180.0)*System.Math.Sin(dLon/2)*System.Math.Sin(dLon/2);
            double c=2*System.Math.Atan2(System.Math.Sqrt(a),System.Math.Sqrt(1-a));
            return R*c;
        }
    }
}
