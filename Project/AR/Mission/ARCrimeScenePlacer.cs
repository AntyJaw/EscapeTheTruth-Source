using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace EtT.AR.Mission
{
    [RequireComponent(typeof(ARRaycastManager))]
    public sealed class ARCrimeScenePlacer : MonoBehaviour
    {
        [Header("Prefaby")]
        [SerializeField] private GameObject composerPrefab; // zawiera CrimeSceneComposer
        [SerializeField] private GameObject placementHint;  // np. sprite/reticle na ziemi (opcjonalnie)

        private ARRaycastManager _raycaster;
        private static List<ARRaycastHit> _hits = new();

        private void Awake() => _raycaster = GetComponent<ARRaycastManager>();

        private void Update()
        {
            if (Input.touchCount == 0) return;
            var t = Input.GetTouch(0);
            if (t.phase != TouchPhase.Began) return;

            if (_raycaster.Raycast(t.position, _hits, TrackableType.PlaneWithinPolygon))
            {
                var pose = _hits[0].pose;
                var go = Instantiate(composerPrefab, pose.position, pose.rotation);
                if (placementHint) placementHint.SetActive(false);
                enabled = false; // jednorazowe postawienie
            }
        }
    }
}