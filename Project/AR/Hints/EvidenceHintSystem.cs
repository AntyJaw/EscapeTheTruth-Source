using UnityEngine;
using EtT.Services;

namespace EtT.AR.Hints
{
    /// <summary>
    /// Co klatkę raycast ze środka ekranu:
    /// - jeśli celuje w AREvidenceNode i warstwa == aktywna warstwa detekcji -> MATCH (glow),
    /// - jeśli warstwa != aktywna -> MISMATCH (flicker + nudge),
    /// - jeśli nie celujesz w dowód -> OFF.
    /// Dodatkowo: opcjonalny tooltip tekstowy emitowany do LayerNudgePanel (przez public API).
    /// </summary>
    public sealed class EvidenceHintSystem : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Camera arCamera;
        [SerializeField] private float rayDistance = 6f;

        [Header("UI Nudge (opcjonalnie)")]
        [SerializeField] private LayerNudgePanel nudgePanel;

        private EtT.AR.Mission.AREvidenceNode _currentNode;
        private EvidenceHintVisual _currentVisual;

        private void Awake()
        {
            if (!arCamera) arCamera = Camera.main;
        }

        private void OnDisable() { ClearCurrent(); if (nudgePanel) nudgePanel.Hide(); }

        private void Update()
        {
            var arSvc = ServiceLocator.Get<IARMissionService>();
            if (arSvc == null || !arSvc.IsActive) { ClearCurrent(); if (nudgePanel) nudgePanel.Hide(); return; }

            var det = ServiceLocator.Get<IDetectionService>();
            var ray = arCamera.ScreenPointToRay(new Vector3(Screen.width/2f, Screen.height/2f));

            if (Physics.Raycast(ray, out var hit, rayDistance))
            {
                var node = hit.collider.GetComponentInParent<EtT.AR.Mission.AREvidenceNode>();
                if (node)
                {
                    if (_currentNode != node) SetCurrent(node);

                    var layer = node.Layer;
                    bool match = (layer == EtT.Evidence.EvidenceLayer.Physical) // Physical zawsze „widzialny”
                                 || (det != null && (
                                        (det.Active == DetectionLayer.UV     && layer == EtT.Evidence.EvidenceLayer.UV) ||
                                        (det.Active == DetectionLayer.EM     && layer == EtT.Evidence.EvidenceLayer.EM) ||
                                        (det.Active == DetectionLayer.Runic  && layer == EtT.Evidence.EvidenceLayer.Runic) ||
                                        (det.Active == DetectionLayer.Psychic&& layer == EtT.Evidence.EvidenceLayer.Psychic)
                                    ));

                    var cfg = Resources.Load<LayerHintConfig>("LayerHintConfig");
                    var col = cfg != null ? cfg.ColorFor(layer) : Color.white;

                    if (_currentVisual != null)
                    {
                        if (match) _currentVisual.ShowMatch(col);
                        else       _currentVisual.ShowMismatch(col);
                    }

                    if (nudgePanel)
                    {
                        if (!match)
                            nudgePanel.Suggest(layer);
                        else
                            nudgePanel.Hide();
                    }
                    return;
                }
            }

            // brak trafienia
            ClearCurrent();
            if (nudgePanel) nudgePanel.Hide();
        }

        private void SetCurrent(EtT.AR.Mission.AREvidenceNode node)
        {
            ClearCurrent();
            _currentNode = node;
            _currentVisual = node.GetComponent<EvidenceHintVisual>();
            if (_currentVisual == null) _currentVisual = node.gameObject.AddComponent<EvidenceHintVisual>();
        }

        private void ClearCurrent()
        {
            if (_currentVisual != null) _currentVisual.Hide();
            _currentVisual = null;
            _currentNode = null;
        }
    }
}