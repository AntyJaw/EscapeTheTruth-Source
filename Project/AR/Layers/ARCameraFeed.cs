using UnityEngine;
using UnityEngine.UI;

namespace EtT.AR.Mission
{
    /// <summary>
    /// Prosty feed z kamery systemowej (WebCamTexture) – działa w edytorze i na urządzeniu.
    /// Podpinamy pod RawImage na pełnym ekranie.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public sealed class ARCameraFeed : MonoBehaviour
    {
        private WebCamTexture _tex;
        private RawImage _img;

        private void Awake()
        {
            _img = GetComponent<RawImage>();
        }

        private void OnEnable()
        {
            if (_tex == null)
            {
                var devices = WebCamTexture.devices;
                if (devices != null && devices.Length > 0)
                {
                    // Preferuj tylną kamerę jeśli dostępna
                    int idx = 0;
                    for (int i = 0; i < devices.Length; i++)
                        if (!devices[i].isFrontFacing) { idx = i; break; }

                    _tex = new WebCamTexture(devices[idx].name, Screen.width, Screen.height, 30);
                }
                else
                {
                    _tex = new WebCamTexture();
                }
            }
            _img.texture = _tex;
            if (!_tex.isPlaying) _tex.Play();
        }

        private void OnDisable()
        {
            if (_tex != null && _tex.isPlaying) _tex.Stop();
        }
    }
}