using System.Collections.Generic;
using EtT.Core;
using EtT.Services;
using UnityEngine;

namespace EtT.Evidence
{
    /// <summary>
    /// Główny serwis dowodów: generuje, degraduje, zapisuje, obsługuje interakcje.
    /// </summary>
    public sealed class EvidenceService
    {
        private readonly List<EvidenceItem> _active = new();
        private const string SAVE_KEY = "evidence_state";

        public void Init()
        {
            Load();
        }

        public IReadOnlyList<EvidenceItem> Active => _active;

        public void AddEvidence(EvidenceItem item)
        {
            _active.Add(item);
            GameEvents.RaiseEvidenceFound(item);
            Persist();
        }

        public void CollectEvidence(string id)
        {
            var ev = _active.Find(e => e.Id == id);
            if (ev != null && !ev.Collected)
            {
                ev.Collected = true;
                GameEvents.RaiseLink13($"Dowód zebrany: {ev.DisplayName}");
                Persist();
            }
        }

        public void DegradeAll(float weatherFactor, float deltaTime)
        {
            foreach (var ev in _active)
            {
                if (ev.Collected) continue;
                ev.Integrity = Mathf.Clamp01(ev.Integrity - weatherFactor * deltaTime);
            }
            Persist();
        }

        public void ClearAll()
        {
            _active.Clear();
            Persist();
        }

        private void Load()
        {
            var save = ServiceLocator.Get<ISaveService>();
            var data = save.Load<List<EvidenceItem>>(SAVE_KEY);
            if (data != null) _active.AddRange(data);
        }

        private void Persist()
        {
            ServiceLocator.Get<ISaveService>().Store(SAVE_KEY, _active);
        }
    }
}