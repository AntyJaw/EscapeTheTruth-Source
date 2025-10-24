using UnityEngine;
using UnityEngine.UI;
using EtT.Core;

namespace EtT.UI.Placeholder
{
    public sealed class PulsingBar : MonoBehaviour
    {
        [SerializeField] private Slider bar;
        [SerializeField] private int warn = 30;
        [SerializeField] private int critical = 10;
        [SerializeField] private float pulseSpeed = 6f;
        [SerializeField] private float pulseAmount = 0.15f;

        private float _target01 = 1f;
        private bool _pulse;

        private void OnEnable(){ GameEvents.OnEnergyChanged += OnEnergy; }
        private void OnDisable(){ GameEvents.OnEnergyChanged -= OnEnergy; }

        private void OnEnergy(int v)
        {
            _target01 = Mathf.Clamp01(v/100f);
            _pulse = v <= warn;
        }

        private void Update()
        {
            if (!bar) return;
            bar.value = Mathf.Lerp(bar.value, _target01, Time.deltaTime*8f);

            if (_pulse)
            {
                var s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                bar.transform.localScale = new Vector3(s, s, 1f);
            }
            else
            {
                bar.transform.localScale = Vector3.one;
            }
        }
    }
}