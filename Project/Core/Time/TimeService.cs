using System;
using System.Collections;
using UnityEngine;
using EtT.Core;

namespace EtT.Time
{
    public sealed class TimeService : ITimeService
    {
        private bool _running;
        private bool _isNight;

        public DateTime NowLocal => DateTime.Now;
        public int NowUnixUtc() => (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public bool IsNight => _isNight;

        public event Action<DateTime> OnTick;

        public void Init()
        {
            if (_running) return;
            _running = true;
            CoroutineRunner.Run(TickRoutine());
        }

        private IEnumerator TickRoutine()
        {
            while (_running)
            {
                var now = DateTime.Now;
                OnTick?.Invoke(now);
                GameEvents.RaiseTimeTick(now);
                yield return new WaitForSeconds(1f);
            }
        }

        // Ustawiane z WeatherService (gdy znamy słońce)
        public void SetNight(bool night)
        {
            if (_isNight == night) return;
            _isNight = night;
            GameEvents.RaiseDayNightChanged(_isNight);
        }

        private static class CoroutineRunner
        {
            private class Host : MonoBehaviour { }
            private static Host _host;
            public static void Run(IEnumerator r)
            {
                if (_host == null)
                {
                    var go = new GameObject("[TimeCoroutineRunner]");
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    _host = go.AddComponent<Host>();
                }
                _host.StartCoroutine(r);
            }
        }
    }
}