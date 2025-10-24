using UnityEngine;
using EtT.Services;

namespace EtT.World.Poi
{
    /// <summary>
    /// Co kilka sekund sprawdza, czy jesteś blisko POI i podpowiada akcję (np. „Odpocznij w kawiarni”).
    /// </summary>
    public sealed class PoiScanner : MonoBehaviour
    {
        public float hintRadiusMeters = 40f;
        public float checkInterval = 3f;
        private float _t;

        void Update()
        {
            _t += Time.deltaTime;
            if (_t < checkInterval) return;
            _t = 0f;

            var gps = ServiceLocator.Get<EtT.IGpsWorldService>();
            var poi = ServiceLocator.Get<EtT.IPoiService>();
            var pos = gps.GetPlayerPosition();

            foreach (var p in poi.All)
            {
                float d = Haversine(pos.Lat,pos.Lng, p.lat,p.lng);
                if (d <= Mathf.Max(hintRadiusMeters, p.radiusMeters))
                {
                    ServiceLocator.Get<EtT.ILink13Service>()?.SendSystem($"[POI] Jesteś blisko: {p.name} ({d:0} m). Kliknij interakcję.");
                    break;
                }
            }
        }

        private static float Haversine(double lat1,double lon1,double lat2,double lon2)
        {
            const double R=6371000.0;
            double dLat=(lat2-lat1)*Mathf.Deg2Rad; double dLon=(lon2-lon1)*Mathf.Deg2Rad;
            double a=System.Math.Sin(dLat/2)*System.Math.Sin(dLat/2)+System.Math.Cos(lat1*Mathf.Deg2Rad)*System.Math.Cos(lat2*Mathf.Deg2Rad)*System.Math.Sin(dLon/2)*System.Math.Sin(dLon/2);
            double c=2*System.Math.Atan2(System.Math.Sqrt(a),System.Math.Sqrt(1-a));
            return (float)(R*c);
        }
    }
}