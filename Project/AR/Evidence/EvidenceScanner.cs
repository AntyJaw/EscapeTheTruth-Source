using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EtT.Services;
using EtT.AR.Layers;

namespace EtT.AR.Evidence
{
    /// <summary>
    /// Skaner dowodów (gaze & charge) z obsługą trybów warstw AR.
    /// Jeśli aktywny tryb nie pasuje do wymogów celu → skan wstrzymany i pojawia się podpowiedź.
    /// </summary>
    public sealed class EvidenceScanner : MonoBehaviour
    {
        [Header("Źródła")]
        public Camera arCamera;

        [Header("Zasięg / selekcja")]
        [Range(0.5f, 15f)] public float maxDistance = 6f;
        [Range(1f, 30f)]  public float aimConeDeg = 8f;
        public LayerMask targetMask = ~0;

        [Header("Ładowanie (sekundy do 100%)")]
        [Range(0.2f, 5f)] public float baseChargeSeconds = 1.8f;

        [Header("UI / Zdarzenia")]
        public UnityEvent<float> onChargeChanged; // 0..1
        public UnityEvent<string> onHintChanged;  // np. "Włącz UV"
        public UnityEvent<EvidenceTarget> onTargetChanged;
        public UnityEvent<EvidenceTarget> onRevealed;

        EvidenceTarget _current;
        float _charge;
        readonly Collider[] _overlaps = new Collider[32];

        IARLayerModeService _layers;

        void Awake()
        {
            _layers = ServiceLocator.Get<IARLayerModeService>();
            if (!arCamera) arCamera = Camera.main;
        }

        void Update()
        {
            // tylko w aktywnym AR
            var ar = ServiceLocator.Get<EtT.AR.Mission.IARMissionService>();
            if (ar == null || !ar.IsInAR || !ar.IsReady) { ClearTarget(); return; }
            if (!arCamera) { ClearTarget(); return; }

            // 1) Wybór celu
            var best = FindBestTarget();
            if (best != _current)
            {
                _current = best;
                _charge = 0f;
                onChargeChanged?.Invoke(_charge);
                onTargetChanged?.Invoke(_current);

                if (_current == null) onHintChanged?.Invoke("");
                else { _current.Ping(); onHintChanged?.Invoke(NeedHint(_current, _layers.Current)); }
            }

            // 2) Ładowanie / blokada, jeśli tryb nie pasuje
            if (_current != null && !_current.isRevealed)
            {
                var mode = _layers.Current;
                bool modeOk = IsModeValidForTarget(mode, _current);

                if (!modeOk)
                {
                    // zatrzymaj ładowanie, lekki spadek
                    _charge = Mathf.MoveTowards(_charge, 0f, Time.deltaTime * 0.75f);
                    onChargeChanged?.Invoke(_charge);
                    // odśwież hint
                    onHintChanged?.Invoke(NeedHint(_current, mode));
                    return;
                }

                // tryb pasuje → normalne ładowanie z uwzględnieniem trudności
                float need = Mathf.Lerp(0.6f, 1.2f, _current.EffectiveDifficulty01());
                float seconds = Mathf.Max(0.2f, baseChargeSeconds * need) * ModeSpeedFactor(mode, _current);

                _charge = Mathf.MoveTowards(_charge, 1f, Time.deltaTime / seconds);
                onChargeChanged?.Invoke(_charge);

                if (_charge >= 1f)
                {
                    _current.Reveal();
                    onRevealed?.Invoke(_current);
                    _charge = 0f;
                    onChargeChanged?.Invoke(_charge);
                }
            }
            else
            {
                if (_charge > 0f) { _charge = Mathf.MoveTowards(_charge, 0f, Time.deltaTime * 2f); onChargeChanged?.Invoke(_charge); }
            }
        }

        EvidenceTarget FindBestTarget()
        {
            Vector3 origin = arCamera.transform.position;
            Vector3 forward = arCamera.transform.forward;
            int n = Physics.OverlapSphereNonAlloc(origin + forward * (maxDistance * 0.5f), maxDistance * 0.6f, _overlaps, targetMask, QueryTriggerInteraction.Ignore);

            EvidenceTarget best = null;
            float bestScore = 0f;

            for (int i = 0; i < n; i++)
            {
                var col = _overlaps[i];
                if (!col) continue;

                var t = col.GetComponentInParent<EvidenceTarget>();
                if (!t) continue;

                Vector3 to = t.transform.position - origin;
                float dist = to.magnitude;
                if (dist > maxDistance) continue;

                float angle = Vector3.Angle(forward, to);
                if (angle > aimConeDeg) continue;

                if (Physics.Raycast(origin, to.normalized, out var hit, dist + 0.05f, targetMask, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider != col && hit.collider.transform.IsChildOf(t.transform) == false)
                        continue;
                }

                float score = Mathf.Lerp(1f, 0.6f, angle / aimConeDeg) * Mathf.Lerp(1f, 0.7f, dist / maxDistance);
                if (score > bestScore) { bestScore = score; best = t; }
            }

            return best;
        }

        void ClearTarget()
        {
            if (_current != null) _current.isAimed = false;
            _current = null;
            if (_charge != 0f) { _charge = 0f; onChargeChanged?.Invoke(_charge); }
        }

        // === Warstwowe zasady ===
        static bool IsModeValidForTarget(ARLayerMode mode, EvidenceTarget t)
        {
            // Jeśli cel nie wymaga specjalnej warstwy → każdy tryb OK (preferuj Physical)
            bool requiresSpecial = t.requiresUV || t.requiresEM || t.requiresRunic;
            if (!requiresSpecial) return true;

            // Wymogi
            if (t.requiresUV   && mode == ARLayerMode.UV) return true;
            if (t.requiresEM   && mode == ARLayerMode.EM) return true;
            if (t.requiresRunic && mode == ARLayerMode.Runic) return true;

            // Psychiczna: pozwalamy tylko na cel typu Psychic (jeśli tak oznaczysz w kind)
            if (mode == ARLayerMode.Psychic && t.kind == EvidenceKind.Psychic) return true;

            return false;
        }

        static float ModeSpeedFactor(ARLayerMode mode, EvidenceTarget t)
        {
            // Minimalna różnica w szybkości, by nagrodzić „dobry tryb” nawet bez requires.
            if (t.requiresUV   && mode == ARLayerMode.UV)    return 0.85f;
            if (t.requiresEM   && mode == ARLayerMode.EM)    return 0.85f;
            if (t.requiresRunic && mode == ARLayerMode.Runic) return 0.85f;
            if (t.kind == EvidenceKind.Psychic && mode == ARLayerMode.Psychic) return 0.85f;

            // Jeżeli tryb nie jest idealny, ale cel nie wymaga specjalnego → lekko wolniej
            bool requiresSpecial = t.requiresUV || t.requiresEM || t.requiresRunic;
            if (!requiresSpecial && mode != ARLayerMode.Physical) return 1.05f;

            return 1.0f;
        }

        string NeedHint(EvidenceTarget t, ARLayerMode mode)
        {
            if (t.requiresRunic && mode != ARLayerMode.Runic) return "Włącz RUNY";
            if (t.requiresEM    && mode != ARLayerMode.EM)    return "Włącz EM";
            if (t.requiresUV    && mode != ARLayerMode.UV)    return "Włącz UV";
            if (t.kind == EvidenceKind.Psychic && mode != ARLayerMode.Psychic) return "Włącz PSYCH";
            return "";
        }
    }
}