using UnityEngine;

namespace EtT.Evidence.Analysis.UI
{
    /// <summary>Prosty obrót/zoom modelu 3D na podglądzie analizy.</summary>
    public sealed class ModelOrbit : MonoBehaviour
    {
        public Transform target;         // ustaw automatycznie przez UI
        public float rotateSpeed = 120f; // deg/s
        public float zoomSpeed = 1.5f;   // scale/s
        public float minScale = 0.5f;
        public float maxScale = 2.2f;

        public void Bind(Transform t) => target = t;

        void Update()
        {
            if (!target) return;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButton(0))
            {
                float dx = Input.GetAxis("Mouse X");
                float dy = Input.GetAxis("Mouse Y");
                target.Rotate(Vector3.up, -dx * rotateSpeed * Time.deltaTime, Space.World);
                target.Rotate(Vector3.right, dy * rotateSpeed * Time.deltaTime, Space.World);
            }
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                float s = Mathf.Clamp(target.localScale.x * (1f + scroll * 0.1f * zoomSpeed), minScale, maxScale);
                target.localScale = new Vector3(s, s, s);
            }
#else
            // touch
            if (Input.touchCount == 1)
            {
                var t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved)
                {
                    var d = t.deltaPosition;
                    target.Rotate(Vector3.up, -d.x * 0.3f, Space.World);
                    target.Rotate(Vector3.right, d.y * 0.3f, Space.World);
                }
            }
            else if (Input.touchCount == 2)
            {
                var t0 = Input.GetTouch(0);
                var t1 = Input.GetTouch(1);
                var prev = (t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition);
                var now  = t0.position - t1.position;
                float diff = now.magnitude - prev.magnitude;
                float s = Mathf.Clamp(target.localScale.x * (1f + diff * 0.001f * zoomSpeed), minScale, maxScale);
                target.localScale = new Vector3(s, s, s);
            }
#endif
        }
    }
}