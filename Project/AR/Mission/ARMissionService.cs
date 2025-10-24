using UnityEngine;
using EtT.Services;

namespace EtT.AR.Mission
{
    public sealed class ARMissionService : IARMissionService
    {
        private GameObject _rootInstance;
        private bool _inAR, _ready;

        public bool IsInAR => _inAR;
        public bool IsReady => _ready;

        public void Init(){ _inAR=false; _ready=false; _rootInstance=null; }

        public void EnterARScene(EtT.Missions.Mission mission)
        {
            if (_inAR) return;
            var prefab = Resources.Load<GameObject>("EtT/AR/ARCrimeRoot");
            if (!prefab) { Debug.LogError("Missing Resources/EtT/AR/ARCrimeRoot prefab"); return; }
            _rootInstance = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(_rootInstance);

            var placer = _rootInstance.GetComponentInChildren<EtT.AR.Placement.ARCrimeScenePlacer>(true);
            if (placer) placer.Bind(this, mission);

            ServiceLocator.Get<IARFlairService>()?.OnAREnter();
            _inAR = true; _ready = false;
        }

        public void ExitARScene()
        {
            ServiceLocator.Get<IARFlairService>()?.OnARExit();
            if (_rootInstance) Object.Destroy(_rootInstance);
            _rootInstance = null; _inAR=false; _ready=false;
        }

        internal void MarkReady(){ _ready = true; }
    }
}