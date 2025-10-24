using System;
using System.Collections.Generic;

namespace EtT.Core.Profiles
{
    /// <summary>
    /// Prosty manager profili (slotów) gry.
    /// Implementuje IProfilesService używane przez PlayerService (do klucza save).
    /// </summary>
    public sealed class ProfilesService : IProfilesService
    {
        // aktywny profil (id = np. "default", "slot2"...)
        private string _activeId = "default";

        // dostępne profile (na MVP trzymamy tylko listę id)
        private readonly HashSet<string> _known = new(StringComparer.OrdinalIgnoreCase)
        { "default" };

        public string ActiveId => _activeId;

        public event Action<string> OnActiveProfileChanged;

        public void SetActive(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            _known.Add(id);
            if (!string.Equals(_activeId, id, StringComparison.OrdinalIgnoreCase))
            {
                _activeId = id;
                OnActiveProfileChanged?.Invoke(_activeId);
            }
        }

        public string[] GetAll()
        {
            var arr = new string[_known.Count];
            _known.CopyTo(arr);
            return arr;
        }

        // MVP: tworzenie prostego slotu
        public string CreateNew(string suggestedId = null)
        {
            string id = string.IsNullOrWhiteSpace(suggestedId)
                ? $"slot{_known.Count+1}"
                : suggestedId.Trim();

            if (_known.Contains(id))
                id = $"{id}_{Guid.NewGuid().ToString("N")[..4]}";

            _known.Add(id);
            return id;
        }

        // MVP: usuwanie „z pamięci”; odpowiedzialność za kasowanie plików save zostawiamy SaveService
        public bool Remove(string id)
        {
            if (string.Equals(id, "default", StringComparison.OrdinalIgnoreCase)) return false; // nie kasujemy domyślnego
            if (!_known.Remove(id)) return false;
            if (string.Equals(_activeId, id, StringComparison.OrdinalIgnoreCase))
            {
                _activeId = "default";
                OnActiveProfileChanged?.Invoke(_activeId);
            }
            return true;
        }
    }
}