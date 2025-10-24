using System.Collections.Generic;
using EtT.Services;

namespace EtT.Missions
{
    public sealed class MissionRepository : IMissionRepository
    {
        private State _state;
        private string Key => "missions_" + Services.ServiceLocator.Get<IProfilesService>().ActiveId;

        public List<Mission> Active => _state.active;
        public List<Mission> Completed => _state.completed;
        public List<Mission> Archived => _state.archived;

        public void Init()
        {
            Load();
            EtT.Core.GameEvents.OnActiveProfileChanged += _ => Load();
        }

        private void Load()
        {
            _state = Services.ServiceLocator.Get<ISaveService>().Load<State>(Key) ?? new State();
            _state.active   ??= new List<Mission>();
            _state.completed??= new List<Mission>();
            _state.archived ??= new List<Mission>();
        }

        public void SaveAll()
        {
            Services.ServiceLocator.Get<ISaveService>().Store(Key, _state);
        }

        private class State
        {
            public List<Mission> active = new();
            public List<Mission> completed = new();
            public List<Mission> archived = new();
        }
    }
}