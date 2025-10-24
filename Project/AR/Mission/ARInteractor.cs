using UnityEngine;
using EtT.Services;
using EtT.Core;

namespace EtT.AR.Mission
{
    /// <summary>
    /// Nasłuchuje tapnięć i wykonuje raycast z kamery w świat.
    /// Jeśli trafimy w obiekt z EvidenceScanTarget, uruchamia "skan".
    /// </summary>
    public sealed class ARInteractor : MonoBehaviour
    {
        [SerializeField] private Camera arCamera; // możesz wskazać główną kamerę

        private void Update()
        {
            if (!ServiceLocator.Get<IARMissionService>().IsActive)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                var pos = Input.mousePosition;
                if (TryHitEvidence(pos, out var target))
                {
                    target.Scan();
                }
            }
#if UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                var pos = Input.GetTouch(0).position;
                if (TryHitEvidence(pos, out var target))
                {
                    target.Scan();
                }
            }
#endif
        }

        private bool TryHitEvidence(Vector2 screenPos, EvidenceScanTarget outTarget = null)
        {
            var cam = arCamera ? arCamera : Camera.main;
            var ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 100f, ~0, QueryTriggerInteraction.Collide))
            {
                var t = hit.collider.GetComponentInParent<EvidenceScanTarget>();
                if (t != null) { outTarget = t; return true; }
            }
            return false;
        }
    }
}