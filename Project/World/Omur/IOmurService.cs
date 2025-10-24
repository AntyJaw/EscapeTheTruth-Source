using UnityEngine;

namespace EtT.World.Omur
{
    /// <summary>
    /// Interfejs systemu O.M.U.R. (Oscillating Memory of Universal Resonance)
    /// – odbiera „puls” emocjonalny gry i reaguje zmianą nastroju, FX, dźwięku itd.
    /// </summary>
    public interface IOmurService
    {
        /// <summary>
        /// Wysyła impuls napięcia do świata.
        /// np. "clue", "hit", "fear", "truth", "lie"
        /// </summary>
        void Pulse(string eventKey);

        /// <summary>
        /// Aktualne napięcie świata (0–100)
        /// </summary>
        float CurrentTension { get; }

        /// <summary>
        /// Aktualizuje w każdej klatce – reaguje na naturalny spadek napięcia.
        /// </summary>
        void Update();

        /// <summary>
        /// Resetuje napięcie świata.
        /// </summary>
        void Reset();
    }
}