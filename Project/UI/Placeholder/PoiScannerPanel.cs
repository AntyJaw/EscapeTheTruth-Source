using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using EtT.Services;

namespace EtT.World.Poi
{
    public sealed class PoiScannerPanel : MonoBehaviour
    {
        [Header("UI refs")]
        [SerializeField] private Button scanBtn;
        [SerializeField] private TMP_Dropdown typeDropdown;
        [SerializeField] private Transform listRoot;
        [SerializeField] private TMP_Text listItemPrefab;

        private void Start()
        {
            typeDropdown.ClearOptions();
            typeDropdown.AddOptions(System.Enum.GetNames(typeof(PoiType)).ToList());
            scanBtn.onClick.AddListener(Scan);
        }

        private void Scan()
        {
            var poiService = ServiceLocator.Get<PoiService>();
            var gps = ServiceLocator.Get<IGpsWorldService>();

            var pos = new EtT.World.Position(52.23, 21.01); // TODO: gps.GetCurrentPosition()
            var type = (PoiType)typeDropdown.value;

            var results = poiService.FindByType(type, pos.Lat, pos.Lng, 5);

            foreach (Transform child in listRoot)
                Destroy(child.gameObject);

            foreach (var poi in results)
            {
                var txt = Instantiate(listItemPrefab, listRoot);
                txt.text = $"{poi.Name} ({poi.DistanceMeters:F0} m)";
                var captured = poi;
                txt.GetComponent<Button>()?.onClick.AddListener(() => PoiInteractions.Interact(captured));
            }
        }
    }
}