using UnityEngine;
using TMPro;
using EtT.Data;
using EtT.Services;

namespace EtT.UI.Placeholder
{
    public sealed class ClassSelectPanel : MonoBehaviour
    {
        [SerializeField] private PlayerClassCatalog catalog;
        [SerializeField] private TMP_Dropdown dropdown;

        private void Start()
        {
            if (!catalog || catalog.classes==null) return;
            dropdown.ClearOptions();
            var opts = new System.Collections.Generic.List<string>();
            foreach (var c in catalog.classes)
            {
                if (c.hiddenUntilRite) continue;
                opts.Add(c.characterClass.ToString());
            }
            dropdown.AddOptions(opts);
        }

        public void ApplySelected()
        {
            if (!catalog || catalog.classes==null) return;
            var name = dropdown.options[dropdown.value].text;
            foreach (var c in catalog.classes)
            {
                if (c.characterClass.ToString()==name)
                {
                    ServiceLocator.Get<IPlayerService>().SelectClass(c);
                    break;
                }
            }
        }
    }
}