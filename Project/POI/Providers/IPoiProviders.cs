using System;
using System.Collections.Generic;

namespace EtT.POI
{
    public interface IPoiProvider
    {
        /// <summary>
        /// Skan okolicy po realnych mapach z filtrem typów (Cafe/Pharmacy/Bench/Store/...)
        /// </summary>
        /// <param name="lat">szerokość geogr.</param>
        /// <param name="lng">długość geogr.</param>
        /// <param name="radiusMeters">promień szukania</param>
        /// <param name="types">jakie typy POI zassać</param>
        /// <param name="onDone">callback: (lista, error) — error==null gdy OK</param>
        void Scan(double lat, double lng, int radiusMeters, EtT.Map.PoiType[] types, Action<List<PoiInstance>, string> onDone);
    }
}