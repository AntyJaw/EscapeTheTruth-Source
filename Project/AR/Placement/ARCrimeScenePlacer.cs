using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EtT.Services;

namespace EtT.AR.Placement
{
    /// <summary>
    /// Jednorazowo stawia anchor po tapnięciu w płaszczyznę i generuje scenę dowodową.
    /// Prefab root powinien zawierać: ARSessionOrigin z ARRaycastManager + ARCamera.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public sealed class ARCrimeScenePlacer : MonoBehaviour
    {
        public GameObject debugPlacementMarker; // opcjonalny wizualny marker pozycji anchoru

        private ARRaycastManager _raycaster;
        private readonly List<ARRaycastHit> _hits = new();
        private EtT.AR.Mission.ARMissionService _arService;
        private EtT.Missions.Mission _mission;
        private bool _placed;

        void Awake() => _raycaster = GetComponent<ARRaycastManager>();

        public void Bind(EtT.AR.Mission.ARMissionService svc, EtT.Missions.Mission m)
        {
            _arService = svc; _mission = m; _placed = false;
        }

        void Update()
        {
            if (_placed || _arService == null || _mission == null) return;

            if (Input.touchCount == 0) return;
            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;

            if (_raycaster.Raycast(touch.position, _hits, TrackableType.PlaneWithinPolygon))
            {
                var pose = _hits[0].pose;

                // Utwórz "kotwicę" (tu jako zwykły rodzic – w MVP bez ARAnchor dla prostoty)
                var anchorRoot = new GameObject("AR_CrimeAnchor");
                anchorRoot.transform.SetPositionAndRotation(pose.position, pose.rotation);

                if (debugPlacementMarker != null)
                    Instantiate(debugPlacementMarker, anchorRoot.transform);

                // Wygeneruj scenę Sköll na tym anchorze
                var composer = anchorRoot.AddComponent<EtT.AR.Compose.CrimeSceneComposer>();
                composer.Compose(_mission);

                _placed = true;
                _arService.MarkReady();

                // Jednorazowo działamy → wyłączamy komponent
                enabled = false;
            }
        }
    }
}