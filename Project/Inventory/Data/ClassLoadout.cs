using UnityEngine;
using EtT.PlayerClasses;

namespace EtT.Inventory
{
    [System.Serializable]
    public class LoadoutEntry
    {
        public ItemDefinition item;
        public int quantity = 1;
    }

    [CreateAssetMenu(fileName = "ClassLoadout", menuName = "EtT/Inventory/Class Loadout")]
    public class ClassLoadout : ScriptableObject
    {
        public CharacterClass characterClass;
        public LoadoutEntry[] items;
    }
}