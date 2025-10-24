using UnityEngine;
using TMPro;
using EtT.Services;

namespace EtT.UI.Placeholder
{
    public sealed class GpsStatusPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text coordsText;
        [SerializeField] private TMP_Text accText;
        [SerializeField] private TMP_Text availText;

        private EtT.World.GpsWorldServiceUnity _gps;

        private void Start()
        {
            _gps = Services.ServiceLocator.Get<EtT.IGpsWorldService>() as EtT.World.GpsWorldServiceUnity;
            Services.ServiceLocator.Get<EtT.IGpsWorldService>()?.Start();
        }

        private void Update()
        {
            var gps = Services.ServiceLocator.Get<EtT.IGpsWorldService>();
            var pos = gps.GetPlayerPosition();
            var acc = gps.AccuracyMeters;
            var ok = gps.IsAvailable;

            if (coordsText) coordsText.text = $"Lat/Lng: {pos.Lat:0.000000}, {pos.Lng:0.000000}";
            if (accText)    accText.text    = $"Dokładność: {acc:0.0} m";
            if (availText)  availText.text  = ok ? "<color=#5cff7a>RUNNING</color>" : "<color=#ff6b6b>OFF</color>";
        }

#if UNITY_EDITOR
        // Przydatne do testów: klik w scenie -> przesuń symulację
        public void SetSimLatLng(string lat, string lng)
        {
            if (double.TryParse(lat, out var la) && double.TryParse(lng, out var ln))
                (Services.ServiceLocator.Get<EtT.IGpsWorldService>() as EtT.World.GpsWorldServiceUnity)?.SetSimulatedPosition(la, ln);
        }
#endif
    }
}