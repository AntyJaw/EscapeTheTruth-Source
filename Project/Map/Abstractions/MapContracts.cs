using System.Collections.Generic;
using EtT.World;

namespace EtT.Map
{
    // Rozszerzona lista rodzajów POI
    public enum PoiType
    {
        Mission,
        Pharmacy,
        Cafe,
        Bench,
        Store,
        RitualSite,
        CrimeScene,
        Link13Station,
        Base,
        Police,
        CursedPlace,
        TeamRally,
        ClassLocked,       // laboratorium/serwerownia/biblioteka itd.
        Anomaly
    }

    public struct Poi
    {
        public string id;
        public PoiType type;
        public string title;
        public Position pos;
        public float radiusMeters;
    }

    public interface IMapView
    {
        void Show();
        void Hide();
        void SetPlayer(Position p);
        void SetMissionArea(Position center, float radiusMeters);
        void SetRoute(List<Position> polyline);
        void ClearRoute();
        void SetPois(IEnumerable<Poi> pois); // ikony/markery; implementacja może być „no-op” w placeholderze
    }

    public interface INavigationService
    {
        void SetMissionTarget(Position center, float radiusMeters);
        void SetPois(IEnumerable<Poi> pois);
        void UpdatePlayerPos(Position p);
        double DistanceToTargetMeters { get; }
        bool IsInsideMissionArea { get; }
        string BuildExternalMapsUrl(bool appleMapsPreferred);
    }
}