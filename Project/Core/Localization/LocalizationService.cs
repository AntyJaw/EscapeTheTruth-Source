using System.Collections.Generic;
using EtT.Core;

namespace EtT.Localization
{
    public sealed class LocalizationService : ILocalizationService
    {
        private readonly Dictionary<string, Dictionary<string,string>> _db = new();
        private string _code = "pl";
        public string CurrentCode => _code;

        public void Init()
        {
            _db["pl"] = new Dictionary<string,string> {
                {"ui.start","Start"}, {"ui.language","Język"}, {"ui.generate","Wygeneruj misję"}, {"ui.daily","Misja dzienna"},
                {"mission.default.title","Zniekształcenie"}, {"mission.default.desc","Zbadaj punkt przeznaczenia"}
            };
            _db["en"] = new Dictionary<string,string> {
                {"ui.start","Start"}, {"ui.language","Language"}, {"ui.generate","Generate mission"}, {"ui.daily","Daily Mission"},
                {"mission.default.title","Distortion"}, {"mission.default.desc","Investigate the destiny point"}
            };
        }

        public void SetLanguage(string code)
        {
            if (_db.ContainsKey(code))
            {
                _code = code;
                GameEvents.RaiseLanguageChanged(_code);
            }
        }

        public string T(string key)
        {
            if (_db.TryGetValue(_code, out var dict) && dict.TryGetValue(key, out var val)) return val;
            return key;
        }
    }
}