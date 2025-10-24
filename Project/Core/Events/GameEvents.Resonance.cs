using System;

namespace EtT.Core
{
    public static partial class GameEvents
    {
        // SZEPT DOLI (rezonans emocji)
        public static event Action<ResonanceState> OnResonanceChanged;

        // BRAMA MOKOSZY (sny/wizje)
        public static event Action<DreamContext> OnDreamStart;
        public static event Action<DreamReport> OnDreamEnd;

        // TKACZKA NAWII (rytuały/global)
        public static event Action<GlobalRitualInfo> OnGlobalRitualAvailable;

        public static void RaiseResonanceChanged(ResonanceState s) => OnResonanceChanged?.Invoke(s);
        public static void RaiseDreamStart(DreamContext c)        => OnDreamStart?.Invoke(c);
        public static void RaiseDreamEnd(DreamReport r)           => OnDreamEnd?.Invoke(r);
        public static void RaiseGlobalRitual(GlobalRitualInfo i)  => OnGlobalRitualAvailable?.Invoke(i);
    }

    // Minimalne DTO
    public struct ResonanceState
    {
        public float mood01;       // 0..1 (0 = mrok, 1 = ukojenie)
        public float tension01;    // 0..1 (akcja)
        public float empathy01;    // 0..1 (świat sprzyja)
        public string tag;         // "storm_night", "calm_day", "guilt_high" itd.
    }

    public struct DreamContext
    {
        public string cause;       // "low_sanity", "post_case", "ritual"
        public int missionSeed;    // powiązanie z ostatnią sprawą
    }

    public struct DreamReport
    {
        public string outcome;     // "omen", "warning", "memory"
        public int deltaSanity;    // zmiana psyche po śnie
    }

    public struct GlobalRitualInfo
    {
        public string id;          // "echo_of_truth_v1"
        public string title;       // nazwa rytuału
        public string desc;        // opis
        public float requiredCharge01; // próg naładowania
    }
}