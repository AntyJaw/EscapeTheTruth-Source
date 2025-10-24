using UnityEngine;
using EtT.Services;

namespace EtT.Core
{
    public sealed class ServiceTicker : MonoBehaviour
    {
        void Update()
        {
            var t = ServiceLocator.Get<EtT.Time.TimeService>();
            t?.Tick();

            var omur = ServiceLocator.Get<EtT.World.Omur.IOmurService>();
            omur?.Tick(Time.deltaTime);
        }
    }
}