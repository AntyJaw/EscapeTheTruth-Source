using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EtT.Services;

namespace EtT.UI.Placeholder
{
    public sealed class InventoryPanel : MonoBehaviour
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private GameObject listItemPrefab; // prosty prefab z TMP_Text + Button "Użyj"
        [SerializeField] private TMP_Text emptyLabel;

        private void OnEnable() => Refresh();

        public void Refresh()
        {
            var inv = ServiceLocator.Get<EtT.IInventoryService>();
            foreach (Transform c in listRoot) Destroy(c.gameObject);

            var all = inv.All();
            if (all.Count == 0) { if (emptyLabel) emptyLabel.gameObject.SetActive(true); return; }
            if (emptyLabel) emptyLabel.gameObject.SetActive(false);

            foreach (var stack in all)
            {
                var go = Instantiate(listItemPrefab, listRoot);
                var label = go.GetComponentInChildren<TMP_Text>();
                var btn = go.GetComponentInChildren<Button>();
                label.text = $"{stack.itemId}  x{stack.quantity}";
                btn.onClick.AddListener(() =>
                {
                    bool ok = inv.Use(stack.itemId);
                    if (ok) Refresh();
                });
            }
        }
    }
}