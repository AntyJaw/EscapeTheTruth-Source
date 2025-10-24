using UnityEngine;
using EtT.Services;

namespace EtT.AR.Compose
{
    /// <summary>
    /// Steruje parametrami materiału dowodu w czasie (zanik, wilgoć, światło) zależnie od pogody i Ó.M.U.R.
    /// Wymaga materiału używającego właściwości: _Visibility, _Wetness, _Light.
    /// </summary>
    public sealed class EvidenceVisibilityController : MonoBehaviour
    {
        public float baseVisibility = 1f;
        public float decayPerMinute = 0.015f;
        public Renderer target;

        private float _visibility;
        private float _t0;

        void Start(){ _visibility = baseVisibility; _t0 = Time.time; }

        void Update()
        {
            if (target == null) return;

            var wx = ServiceLocator.Get<IWeatherService>();
            var om = ServiceLocator.Get<EtT.World.Omur.IOmurService>();

            float rain = wx != null ? Mathf.Clamp01(wx.Rain01) : 0f;
            float humid = wx != null ? Mathf.Clamp01(wx.Humidity01) : 0.3f;
            float light = (om != null) ? Mathf.Clamp01(om.Current.mood01) : 0.5f;

            float minutes = (Time.time - _t0) / 60f;
            float weatherFactor =
                  Mathf.Lerp(1f, 0.4f, rain)
                * Mathf.Lerp(1f, 0.7f, humid)
                * 1f;

            _visibility = Mathf.Clamp01(baseVisibility - decayPerMinute * minutes / Mathf.Max(0.2f, weatherFactor));

            var mat = target.material;
            if (mat.HasProperty("_Visibility")) mat.SetFloat("_Visibility", _visibility);
            if (mat.HasProperty("_Wetness"))    mat.SetFloat("_Wetness", Mathf.Lerp(0f, 1f, rain * humid));
            if (mat.HasProperty("_Light"))      mat.SetFloat("_Light", light);
        }
    }
}