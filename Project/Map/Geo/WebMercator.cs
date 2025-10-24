using UnityEngine;

namespace EtT.Map.Geo
{
    public static class WebMercator
    {
        const double R_MAJOR = 6378137.0;

        public static Vector2 Project(double lat, double lon)
        {
            double x = R_MAJOR * Mathf.Deg2Rad * lon;
            double y = R_MAJOR * System.Math.Log(System.Math.Tan((90.0 + lat) * Mathf.Deg2Rad * 0.5));
            return new Vector2((float)x, (float)y);
        }

        public static Vector2 Project(World.Position p) => Project(p.Lat, p.Lng);

        public static Vector2 NormalizeToRect(Vector2 p, Vector2 min, Vector2 max, Rect rect)
        {
            // normalizuj do [0..1] po bbox, potem przeskaluj do rect
            var n = new Vector2(
                Mathf.InverseLerp(min.x, max.x, p.x),
                Mathf.InverseLerp(min.y, max.y, p.y)
            );
            return new Vector2(
                rect.xMin + n.x * rect.width,
                rect.yMin + n.y * rect.height
            );
        }
    }
}