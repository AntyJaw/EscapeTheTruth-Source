using UnityEngine;
using TMPro;
using EtT.Services;
using EtT.Skills;

namespace EtT.UI.Placeholder
{
    public sealed class SkillsPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text output;

        public void GainClassXP_HackerGPS()
        {
            var p = ServiceLocator.Get<IPlayerService>();
            p.GainSkillXp("hacker_gps", SkillKind.HackerGPS, 25, XpChannel.Class);
            p.AddClassXp(25);
            Refresh();
        }

        public void GainPersonalXP_Spostrzegawczosc()
        {
            var p = ServiceLocator.Get<IPlayerService>();
            p.GainSkillXp("Spostrzegawczosc", SkillKind.Spostrzegawczosc, 15, XpChannel.Personal);
            p.AddPersonalXp(15);
            Refresh();
        }

        public void GainEsotericXP_Rytualy()
        {
            var p = ServiceLocator.Get<IPlayerService>();
            p.GainSkillXp("ritual_sight", SkillKind.Rytualy, 20, XpChannel.Esoteric, esoteric:true);
            p.AddEsotericXp(20);
            Refresh();
        }

        public void Refresh()
        {
            var p = ServiceLocator.Get<IPlayerService>();
            float h = p.GetSkillEffectiveLevel("hacker_gps");
            float s = p.GetSkillEffectiveLevel("Spostrzegawczosc");
            float r = p.GetSkillEffectiveLevel("ritual_sight");
            output.text = $"HackerGPS eff: {h:0.0}\nSpostrzegawczość eff: {s:0.0}\nRytuały eff: {r:0.0}";
        }
    }
}