using UnityEngine;
using EtT.Services;

namespace EtT.AR.Mission
{
    /// <summary>
    /// Włącza/wyłącza warstwę AR (feed kamery + interaktor + reticle),
    /// bazując na stanie IARMissionService.
    /// </summary>
    public sealed class ARMissionController : MonoBehaviour
    {
        [SerializeField] private GameObject arLayerRoot; // rodzic: RawImage (ARCameraFeed), ARInteractor, reticle, itp.

        private void OnEnable()
        {
            ServiceLocator.Get<EtT.IARMissionService>().OnStateChanged += ApplyState;
            ApplyState(ServiceLocator.Get<EtT.IARMissionService>().IsActive);
        }

        private void OnDisable()
        {
            ServiceLocator.Get<EtT.IARMissionService>().OnStateChanged -= ApplyState;
        }

        private void ApplyState(bool active)
        {
            if (arLayerRoot) arLayerRoot.SetActive(active);
        }
    }
}