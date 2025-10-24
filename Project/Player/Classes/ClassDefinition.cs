using UnityEngine;
using EtT.PlayerClasses;
using EtT.PlayerStats;

namespace EtT.Data
{
    [CreateAssetMenu(fileName = "ClassDef", menuName = "EtT/Player/Class Definition")]
    public class ClassDefinition : ScriptableObject
    {
        public CharacterClass characterClass;

        [Header("Startowe statystyki (paski 0..100)")]
        [Range(1,100)] public int startHealth = 80;
        [Range(1,100)] public int startPsychika = 100;
        [Range(1,100)] public int startEnergia = 80;
        [Range(0,100)] public int startReputacja = 50;

        [Header("Startowe atrybuty (Perception/Technique/Composure)")]
        public Attributes startAttributes;

        [Header("Startowe punkty do rozdania")]
        [Min(0)] public int extraPoints = 5;

        [Header("Umiejętności klasowe (ID)")]
        public string[] classSkillIds;

        [Header("Klasa ukryta do czasu Przesilenia?")]
        public bool hiddenUntilRite = false;
    }
}