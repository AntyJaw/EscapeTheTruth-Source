using System.IO;
using UnityEngine;

namespace EtT.Save
{
    public sealed class SaveServiceJson : ISaveService
    {
        private string _path;
        public void Init()
        {
            _path = Path.Combine(Application.persistentDataPath, "save");
            if (!Directory.Exists(_path)) Directory.CreateDirectory(_path);
        }

        public void Save() { }

        public T Load<T>(string key) where T : class, new()
        {
            var f = Path.Combine(_path, key + ".json");
            if (!File.Exists(f)) return null;
            return JsonUtility.FromJson<T>(File.ReadAllText(f));
        }

        public void Store<T>(string key, T data) where T : class
        {
            var f = Path.Combine(_path, key + ".json");
            File.WriteAllText(f, JsonUtility.ToJson(data, false));
        }
    }
}