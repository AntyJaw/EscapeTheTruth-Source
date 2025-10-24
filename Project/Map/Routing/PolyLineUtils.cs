using System.Collections.Generic;
using EtT.World;

namespace EtT.Map.Routing
{
    // Dekodowanie Google/GraphHopper polyline
    public static class PolylineUtils
    {
        public static List<Position> Decode(string encoded)
        {
            var poly = new List<Position>();
            int index = 0, len = encoded.Length;
            int lat = 0, lng = 0;

            while (index < len)
            {
                int b, shift = 0, result = 0;
                do { b = encoded[index++] - 63; result |= (b & 0x1f) << shift; shift += 5; }
                while (b >= 0x20);
                int dlat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lat += dlat;

                shift = 0; result = 0;
                do { b = encoded[index++] - 63; result |= (b & 0x1f) << shift; shift += 5; }
                while (b >= 0x20);
                int dlng = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lng += dlng;

                double latF = lat / 1E5;
                double lngF = lng / 1E5;
                poly.Add(new Position(latF, lngF));
            }
            return poly;
        }
    }
}