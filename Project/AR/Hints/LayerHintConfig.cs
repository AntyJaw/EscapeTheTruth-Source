using UnityEngine;

namespace EtT.AR.Hints
{
    [CreateAssetMenu(fileName = "LayerHintConfig", menuName = "EtT/AR/Layer Hint Config")]
    public class LayerHintConfig : ScriptableObject
    {
        [Header("Glow (poprawna warstwa)")]
        public Color physicalColor = new Color(0.90f, 0.90f, 0.90f);   // neutralny biały
        public Color uvColor       = new Color(0.35f, 0.65f, 1.00f);
        public Color emColor       = new Color(0.25f, 1.00f, 0.65f);
        public Color runicColor    = new Color(0.95f, 0.45f, 0.95f);
        public Color psychicColor  = new Color(1.00f, 0.70f, 0.25f);

        [Range(0f,5f)] public float glowIntensity = 1.4f;

        [Header("Flicker (zła warstwa)")]
        [Range(0f,1f)] public float flickerBase = 0.15f;
        [Range(0f,3f)] public float flickerAmplitude = 0.35f;
        [Range(0.1f,10f)] public float flickerSpeed = 3.0f;

        public Color ColorFor(EtT.Evidence.EvidenceLayer layer)
        {
            switch (layer)
            {
                case EtT.Evidence.EvidenceLayer.UV:      return uvColor;
                case EtT.Evidence.EvidenceLayer.EM:      return emColor;
                case EtT.Evidence.EvidenceLayer.Runic:   return runicColor;
                case EtT.Evidence.EvidenceLayer.Psychic: return psychicColor;
                default:                                  return physicalColor;
            }
        }
    }
}