using System.Collections.Generic;
using UnityEngine;

namespace EtT.Inventory
{
    public sealed class ItemDatabase : ScriptableObject
    {
        public ItemDefinition[] all;
        private readonly Dictionary<string, ItemDefinition> _map = new();

        public void BuildIndex()
        {
            _map.Clear();
            if (all == null) return;
            foreach (var d in all)
                if (d && !string.IsNullOrEmpty(d.id))
                    _map[d.id] = d;
        }

        public ItemDefinition Get(string id)
        {
            if (_map.Count == 0) BuildIndex();
            return _map.TryGetValue(id, out var def) ? def : null;
        }

        public static ItemDatabase LoadOrCreate()
        {
            var db = Resources.Load<ItemDatabase>("ItemDatabase");
            if (!db) db = ScriptableObject.CreateInstance<ItemDatabase>();
            return db;
        }
    }
}