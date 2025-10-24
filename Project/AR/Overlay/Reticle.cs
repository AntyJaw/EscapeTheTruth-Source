using UnityEngine;

namespace EtT.AR.Overlay
{
    public sealed class Reticle : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float pulse = 0.05f;
        [SerializeField] private float speed = 2f;
        private Vector3 _base;

        private void Start(){ if (target) _base = target.localScale; }
        private void Update()
        {
            if (!target) return;
            float s = 1f + Mathf.Sin(Time.time * speed) * pulse;
            target.localScale = _base * s;
        }
    }
}
