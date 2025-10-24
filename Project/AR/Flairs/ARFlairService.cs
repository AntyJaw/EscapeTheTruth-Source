using UnityEngine;

namespace EtT.AR.Flairs
{
    public sealed class ARFlairService : IARFlairService
    {
        public void Init(){ /* w przyszłości audio/FX */ }
        public void OnAREnter(){ Debug.Log("[AR] Enter"); }
        public void OnARExit(){ Debug.Log("[AR] Exit"); }
    }
}