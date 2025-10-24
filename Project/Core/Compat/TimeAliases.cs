namespace EtT
{
    // Umożliwia wywołania w starym stylu: EtT.Time.deltaTime, EtT.Time.realtimeSinceStartupAsDouble
    public static class Time
    {
        public static float  deltaTime => UnityEngine.Time.deltaTime;
        public static double realtimeSinceStartupAsDouble => UnityEngine.Time.realtimeSinceStartupAsDouble;
    }
}

namespace EtT.Time
{
    // Dla części referencji, które traktowały 'Time' jako przestrzeń nazw i szukały
    // klasy/obiektu 'time' – podajemy duplikat.
    public static class time
    {
        public static float  deltaTime => UnityEngine.Time.deltaTime;
        public static double realtimeSinceStartupAsDouble => UnityEngine.Time.realtimeSinceStartupAsDouble;
    }
}