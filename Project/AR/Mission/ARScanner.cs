using UnityEngine;
using UnityEngine.UI;
using EtT.Services;

namespace EtT.AR.Mission
{
    /// <summary>
    /// Na środku ekranu rzutuje ray w scenę AR.
    /// Jeżeli trafi w AREvidenceNode i warstwa się zgadza, trzymając dotyk w miejscu ładuje progress.
    /// Po osiągnięciu 100% -> Collect().
    /// </summary>
    public sealed class ARScanner : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Camera arCamera;
        [SerializeField] private Image progressFill;   // UI Image (Filled, radial lub horizontal)
        [SerializeField] private RectTransform reticle; // opcjonalny celownik
        [SerializeField] private float scanSeconds = 1.5f;

        private float _progress;
        private AREvidenceNode _current;

        private void Awake()
        {
            if (!arCamera) arCamera = Camera.main;
            SetProgress(0f);
        }

        private void Update()
        {
            if (!ServiceLocator.Get<IARMissionService>().IsActive) { SetProgress(0); return; }

            // ray ze środka ekranu
            var center = new Vector3(Screen.width/2f, Screen.height/2f, 0f);
            var ray = arCamera.ScreenPointToRay(center);

            if (Physics.Raycast(ray, out var hit, 5f))
            {
                var node = hit.collider.GetComponentInParent<AREvidenceNode>();
                if (node != null && CanSeeLayer(node))
                {
                    HandleScan(node);
                    return;
                }
            }

            // nic sensownego
            _current = null;
            SetProgress(0f);
        }

        private bool CanSeeLayer(AREvidenceNode node)
        {
            var det = ServiceLocator.Get<IDetectionService>();
            if (node.Layer == EvidenceLayer.Physical) return true;
            return (det != null) && (
                (det.Active == DetectionLayer.UV     && node.Layer == EvidenceLayer.UV) ||
                (det.Active == DetectionLayer.EM     && node.Layer == EvidenceLayer.EM) ||
                (det.Active == DetectionLayer.Runic  && node.Layer == EvidenceLayer.Runic) ||
                (det.Active == DetectionLayer.Psychic&& node.Layer == EvidenceLayer.Psychic)
            );
        }

        private void HandleScan(AREvidenceNode node)
        {
            // trzymanie dotyku = ładowanie
            bool holding = Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(0).phase == TouchPhase.Stationary || Input.GetTouch(0).phase == TouchPhase.Moved);
            if (!holding)
            {
                _current = null;
                SetProgress(0f);
                return;
            }

            if (_current != node)
            {
                _current = node;
                SetProgress(0f);
            }

            _progress += Time.deltaTime / Mathf.Max(0.1f, scanSeconds);
            SetProgress(_progress);

            if (_progress >= 1f)
            {
                node.Collect();
                _current = null;
                SetProgress(0f);
            }
        }

        private void SetProgress(float v)
        {
            _progress = Mathf.Clamp01(v);
            if (progressFill) progressFill.fillAmount = _progress;
        }
    }
}