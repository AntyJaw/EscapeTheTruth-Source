using UnityEngine;
using EtT.Services;
using EtT.Core;

namespace EtT.Systems.Tradr
{
    /// Tkaczka Nawii – scala stan Ó.M.U.R. + karmę B.Ö.N.D. i ogłasza rytuały.
    public sealed class WorldWeaverService : IWorldWeaverService
    {
        private readonly TradrConfig _cfg;
        private bool _announced;

        public WorldWeaverService(TradrConfig cfg = null) { _cfg = cfg ?? ScriptableObject.CreateInstance<TradrConfig>(); }
        public void Init() { _announced = false; }

        public void Tick(float dt)
        {
            var omur = ServiceLocator.Get<EtT.World.Omur.IOmurService>();
            var bond = ServiceLocator.Get<EtT.Systems.Bond.IBondKarmaService>();
            if (omur == null || bond == null) return;

            var st = omur.Current;
            int karma = bond.Score;

            if (!_announced &&
                st.mood01 >= _cfg.requiredMood &&
                st.empathy01 >= _cfg.requiredEmpathy &&
                karma >= _cfg.requiredKarma)
            {
                _announced = true;
                GameEvents.RaiseGlobalRitual(new GlobalRitualInfo{
                    id=_cfg.ritualId, title=_cfg.ritualTitle, desc=_cfg.ritualDesc, requiredCharge01=1f
                });
                Debug.Log("[T.R.A.Ð.R.] Global ritual available: " + _cfg.ritualTitle);
            }
        }
    }
}