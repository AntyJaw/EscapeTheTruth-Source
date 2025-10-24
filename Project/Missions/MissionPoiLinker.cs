using UnityEngine;
using EtT.Services;

namespace EtT.Missions
{
    /// <summary>
    /// Automatycznie blokuje/odblokowuje POI w obszarze aktywnej misji (center + radius),
    /// gdzie radius jest już wyliczony z uwzględnieniem profilu gracza/utrudnień.
    /// </summary>
    public sealed class MissionPoiLinker : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEvents.OnMissionStatusChanged += OnMissionStatusChanged;
            GameEvents.OnMissionGenerated += OnMissionGenerated;
        }

        private void OnDisable()
        {
            GameEvents.OnMissionStatusChanged -= OnMissionStatusChanged;
            GameEvents.OnMissionGenerated -= OnMissionGenerated;
        }

        private void OnMissionGenerated(Mission m)
        {
            // Gdy misja dopiero powstała (Pending), nic nie blokujemy — blokada wejdzie przy Active.
        }

        private void OnMissionStatusChanged(Mission mission, MissionStatus status)
        {
            var poi = ServiceLocator.Get<EtT.IPoiService>();
            if (poi == null || mission == null) return;

            if (status == MissionStatus.Active)
            {
                // Blokujemy POI dokładnie w obszarze misji (centrum i promień misji)
                poi.LockInArea(mission.Center, mission.RadiusMeters);
                ServiceLocator.Get<ILink13Service>()?.SendSystem($"[POI] Zablokowano punkty w obszarze śledztwa (r={mission.RadiusMeters:0} m).");
            }
            else if (status == MissionStatus.Completed || status == MissionStatus.Abandoned || status == MissionStatus.Archived)
            {
                poi.UnlockAll();
                ServiceLocator.Get<ILink13Service>()?.SendSystem("[POI] Odblokowano punkty po zakończeniu misji.");
            }
        }
    }
}