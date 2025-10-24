using UnityEngine;
using UnityEngine.Events;
using EtT.Services;

namespace EtT.AR.Evidence
{
    public enum EvidenceKind { Physical, UV, EM, Runic, Psychic }

    /// <summary>
    /// Komponent na obiekcie AR, który może być wykryty przez skaner.
    /// </summary>
    public sealed class EvidenceTarget : MonoBehaviour
    {
        [Header("Identyfikacja")]
        public string evidenceId = "ev_001";
        public EvidenceKind kind = EvidenceKind.Physical;

        [Header("Wymagania / trudność")]
        [Range(0f,1f)] public float baseRevealDifficulty = 0.6f; // 0=łatwo, 1=trudno
        public bool requiresUV;
        public bool requiresEM;
        public bool requiresRunic;

        [Header("FX")]
        public Renderer highlightRenderer;
        [ColorUsage(true,true)] public Color glowColor = new Color(1f, 0.9f, 0.2f, 1f);

        [Header("Zdarzenia")]
        public UnityEvent onRevealed;
        public UnityEvent onPing;

        // runtime
        [System.NonSerialized] public bool isRevealed;
        [System.NonSerialized] public bool isAimed;

        Material _matInstance;

        void Awake()
        {
            if (highlightRenderer != null)
                _matInstance = highlightRenderer.material; // instancja
        }

        void OnDestroy()
        {
            if (_matInstance != null) Destroy(_matInstance);
        }

        /// <summary>Wywołaj, gdy skaner „trafi” w obiekt – np. do lekkiego pinga.</summary>
        public void Ping()
        {
            onPing?.Invoke();
            if (_matInstance && _matInstance.HasProperty("_EmissionColor"))
                _matInstance.SetColor("_EmissionColor", glowColor * 2.0f);
        }

        /// <summary>Trwałe odkrycie obiektu (XP + O.M.U.R. + eventy).</summary>
        public void Reveal()
        {
            if (isRevealed) return;
            isRevealed = true;
            onRevealed?.Invoke();

            // XP (jeśli PlayerService dostępny)
            var player = ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService;
            if (player != null)
                player.GainSkillXp($"scan_{kind}", EtT.Skills.SkillKind.Technika, 8, EtT.Skills.XpChannel.Investigation);

            // Puls O.M.U.R.
            ServiceLocator.Get<EtT.World.Omur.IOmurService>()?.Pulse("clue");
        }

        /// <summary>
        /// Oblicza wymagany „ładunek” skanu po uwzględnieniu umiejętności gracza, napięcia i pogody.
        /// Zwraca wartość 0..1 (im mniejsza, tym łatwiej).
        /// </summary>
        public float EffectiveDifficulty01()
        {
            float d = baseRevealDifficulty;

            // umiejętności / atrybuty
            var player = ServiceLocator.Get<IPlayerService>() as EtT.Player.PlayerService;
            if (player != null)
            {
                float tech = Mathf.Clamp01(player.Attr.Technique / 10f);
                d -= 0.25f * tech; // lepsza technika => niższa trudność
                float perc = Mathf.Clamp01(player.Attr.Perception / 10f);
                d -= 0.15f * perc;
            }

            // napięcie świata (OMUR) utrudnia
            var omur = ServiceLocator.Get<EtT.World.Omur.IOmurService>();
            if (omur != null)
                d += Mathf.Clamp01(omur.CurrentTension / 100f) * 0.1f;

            // pogoda: deszcz/wilgoć utrudniają UV/EM/Runic
            var wx = ServiceLocator.Get<IWeatherService>();
            if (wx != null)
            {
                float wet = Mathf.Clamp01(wx.Rain01 * 0.6f + wx.Humidity01 * 0.4f);
                if (requiresUV || requiresEM || requiresRunic) d += wet * 0.15f;
            }

            return Mathf.Clamp01(d);
        }
    }
}