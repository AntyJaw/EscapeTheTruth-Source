using System.Collections.Generic;
using UnityEngine;
using EtT.Services;
using EtT.PlayerClasses;
using EtT.Core;

namespace EtT.Inventory
{
    public sealed class InventoryService : IInventoryService
    {
        private string _saveKey = "inv_default";
        private readonly List<ItemStack> _items = new();
        private ItemDatabase _db;

        [System.Serializable]
        private class SaveData { public List<ItemStack> items = new(); }

        public void Init(string profileKeyPrefix)
        {
            _db = ItemDatabase.LoadOrCreate();
            _saveKey = $"{profileKeyPrefix}{Services.ServiceLocator.Get<IProfilesService>().ActiveId}";
            Load();

            // reaguj na zmianę profilu
            EtT.Core.GameEvents.OnActiveProfileChanged += _ => {
                _saveKey = $"{profileKeyPrefix}{Services.ServiceLocator.Get<IProfilesService>().ActiveId}";
                Load();
            };
        }

        public void ClearAll() { _items.Clear(); Persist(); }

        public void Add(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return;
            int idx = _items.FindIndex(s => s.itemId == itemId);
            if (idx >= 0) { var s = _items[idx]; s.quantity += amount; _items[idx] = s; }
            else _items.Add(new ItemStack(itemId, amount));
            Persist();
        }

        public bool Consume(string itemId, int amount = 1)
        {
            int idx = _items.FindIndex(s => s.itemId == itemId);
            if (idx < 0) return false;
            var s = _items[idx];
            if (s.quantity < amount) return false;
            s.quantity -= amount;
            if (s.quantity == 0) _items.RemoveAt(idx); else _items[idx] = s;
            Persist();
            return true;
        }

        public int QuantityOf(string itemId)
        {
            int idx = _items.FindIndex(s => s.itemId == itemId);
            return idx < 0 ? 0 : _items[idx].quantity;
        }

        public System.Collections.Generic.IReadOnlyList<ItemStack> All() => _items;

        public void ApplyDefaultLoadout(CharacterClass cls)
        {
            // szukamy w Resources wszystkich ClassLoadout
            var loadouts = Resources.LoadAll<ClassLoadout>("");
            foreach (var lo in loadouts)
            {
                if (!lo || lo.characterClass != cls || lo.items == null) continue;
                foreach (var e in lo.items)
                {
                    if (e == null || e.item == null || e.quantity <= 0) continue;
                    Add(e.item.id, e.quantity);
                }
                GameEvents.RaiseLink13($"[Ekwipunek] Przyznano ekwipunek startowy dla {cls}.");
                return;
            }
            GameEvents.RaiseLink13($"[Ekwipunek] Brak skonfigurowanego loadoutu dla {cls}.");
        }

        public bool Use(string itemId)
        {
            var def = _db.Get(itemId);
            if (def == null) { GameEvents.RaiseLink13($"[Item] Nieznany przedmiot: {itemId}"); return false; }

            var ps = Services.ServiceLocator.Get<IPlayerService>() as Player.PlayerService;
            if (def.requireEsoteric && (ps == null || !ps.EsotericUnlocked))
            {
                GameEvents.RaiseLink13("[Item] Wymagana ezoteryka.");
                return false;
            }

            bool consumed = false;

            // proste efekty MVP
            if (def.healthDelta != 0) { ps.AddHealth(def.healthDelta); consumed = true; }
            if (def.energyDelta != 0) { ps.AddEnergy(def.energyDelta); consumed = true; }
            if (def.sanityDelta != 0) { ps.AddSanity(def.sanityDelta); consumed = true; }

            // narzędzia (Tool) – nie zużywamy, tylko „aktywujemy” (tu jedynie komunikat)
            if (def.type == ItemType.Tool)
            {
                GameEvents.RaiseLink13($"[Narzędzie] {def.displayName} aktywne.");
                return true; // brak konsumpcji
            }

            // consumable: jeśli było jakieś delta, spróbuj zużyć 1 szt.
            if (def.type == ItemType.Consumable && consumed)
            {
                if (!Consume(itemId, 1))
                {
                    GameEvents.RaiseLink13("[Item] Brak na stanie.");
                    return false;
                }
                GameEvents.RaiseLink13($"[Item] Użyto: {def.displayName}.");
                return true;
            }

            // domyślnie: nic nie zrobiło
            GameEvents.RaiseLink13($"[Item] {def.displayName}: brak efektu (MVP).");
            return false;
        }

        // === SAVE ===
        private void Load()
        {
            var save = Services.ServiceLocator.Get<ISaveService>();
            var data = save.Load<SaveData>(_saveKey);
            _items.Clear();
            if (data != null && data.items != null) _items.AddRange(data.items);
        }

        private void Persist()
        {
            var save = Services.ServiceLocator.Get<ISaveService>();
            save.Store(_saveKey, new SaveData { items = new List<ItemStack>(_items) });
        }
    }
}