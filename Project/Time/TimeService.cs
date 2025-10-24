using UnityEngine;

namespace EtT.Time
{
    public sealed class TimeService : ITimeService
    {
        public float DeltaTime { get; private set; }

        public int NowUnixUtc() => (int)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public bool IsNightLocal()
        {
            var hour = System.DateTime.Now.Hour;
            return hour < 6 || hour >= 21;
        }

        public void Init() { DeltaTime = 0f; }

        public void Tick() => DeltaTime = UnityEngine.Time.deltaTime;
    }
}
