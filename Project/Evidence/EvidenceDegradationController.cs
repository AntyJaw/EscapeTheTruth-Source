using UnityEngine;
using EtT.Services;

namespace EtT.Evidence
{
    /// <summary>
    /// Prost y kontroler scenowy – w Update zmniejsza integralność dowodów wg pogody.
    /// Podepnij do dowolnego GameObject w scenie (np. SystemsRoot).
    /// </summary>
    public sealed class EvidenceDegradationController : MonoBehaviour
    {
        private IWeatherService _weather;
        private EvidenceService _evidence;

        private void Start()
        {
            _weather = ServiceLocator.Get<IWeatherService>();
            _evidence = ServiceLocator.Get<EvidenceService>();
        }

        private void Update()
        {
            if (_weather == null || _evidence == null) return;
            float perSec = _weather.EvidenceDegradePerSecond();
            if (perSec <= 0f) return;
            _evidence.DegradeAll(perSec, Time.deltaTime);
        }
    }
}