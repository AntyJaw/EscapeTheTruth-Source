using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EtT.Services;

namespace EtT.UI.Placeholder
{
    public sealed class ARMissionPanel : MonoBehaviour
    {
        [SerializeField] private Button btnStart;
        [SerializeField] private Button btnStop;
        [SerializeField] private TMP_Text progressText;

        private EtT.AR.Mission.ARMissionService _svc;

        private void Awake()
        {
            _svc = ServiceLocator.Get<IARMissionService>() as EtT.AR.Mission.ARMissionService;
            if (btnStart) btnStart.onClick.AddListener(()=>_svc.StartMission());
            if (btnStop)  btnStop.onClick.AddListener(()=>_svc.StopMission());
        }

        private void OnEnable()
        {
            if (_svc != null) _svc.OnStateChanged += OnState;
            OnState(_svc != null && _svc.IsActive);
        }

        private void OnDisable()
        {
            if (_svc != null) _svc.OnStateChanged -= OnState;
        }

        private void Update()
        {
            if (_svc == null) return;
            var (c, r) = _svc.Progress();
            if (progressText) progressText.text = $"Dowody: {c}/{r}";
        }

        private void OnState(bool active)
        {
            if (btnStart) btnStart.interactable = !active;
            if (btnStop)  btnStop.interactable  = active;
        }
    }
}