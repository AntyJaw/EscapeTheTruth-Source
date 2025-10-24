using UnityEngine;

namespace EtT.Inventory
{
    public enum ItemType
    {
        Consumable,   // zużywalne: apteczka, kawa, tonik
        Tool,         // narzędzia: UV, EM, wytrychy
        Quest,        // dowody/specjalne
        Ritual        // rytuały/artefakty
    }

    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "EtT/Inventory/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identyfikator")]
        public string id;            // np. "medkit", "coffee", "uv_lamp"
        public string displayName;
        [TextArea] public string description;
        public ItemType type = ItemType.Consumable;
        public Sprite icon;

        [Header("Stackowanie")]
        public bool stackable = true;
        public int maxStack = 10;

        [Header("Efekty (MVP: proste)")]
        public int healthDelta;     // +HP
        public int energyDelta;     // +ENERGIA
        public int sanityDelta;     // +PSYCHE
        public bool requireEsoteric;// jeśli true: działa tylko gdy odblokowana ezoteryka
    }
}