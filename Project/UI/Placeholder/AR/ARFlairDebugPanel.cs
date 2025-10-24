using UnityEngine;
using TMPro;
using UnityEngine.UI;
using EtT.Services;
using EtT.AR.Flairs;

namespace EtT.UI.Placeholder
{
    public sealed class ARFlairDebugPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown flairDropdown;
        [SerializeField] private Button triggerBtn;

        private void Start()
        {
            flairDropdown.ClearOptions();
            flairDropdown.options.Add(new TMP_Dropdown.OptionData("flair_001 • Światło Prawdy"));
            flairDropdown.options.Add(new TMP_Dropdown.OptionData("flair_002 • Rytuał Przejścia"));
            flairDropdown.options.Add(new TMP_Dropdown.OptionData("flair_003 • Lustro Widzenia"));
            flairDropdown.options.Add(new TMP_Dropdown.OptionData("flair_004 • Krąg Rytualny"));
            flairDropdown.options.Add(new TMP_Dropdown.OptionData("flair_005 • Dotyk Krwi"));
            flairDropdown.options.Add(new TMP_Dropdown.OptionData("flair_006 • Szept do Runy"));
            flairDropdown.options.Add(new TMP_Dropdown.OptionData("flair_007 • Woda Oczyszczenia"));
            flairDropdown.options.Add(new TMP_Dropdown.OptionData("flair_008 • Relikwia Gracza"));
            flairDropdown.options.Add(new TMP_Dropdown.OptionData("flair_009 • Sprzężenie Ezoteryczne"));

            triggerBtn.onClick.AddListener(() =>
            {
                string id = flairDropdown.value switch
                {
                    0 => FlairIds.LightOfTruth,
                    1 => FlairIds.RiteOfPassage,
                    2 => FlairIds.MirrorSight,
                    3 => FlairIds.RitualCircle,
                    4 => FlairIds.BloodTouch,
                    5 => FlairIds.WhisperToRune,
                    6 => FlairIds.WaterCleansing,
                    7 => FlairIds.PlayerRelic,
                    8 => FlairIds.EsotericCoupling,
                    _ => FlairIds.LightOfTruth
                };
                ServiceLocator.Get<IARFlairService>().Trigger(id);
            });
        }
    }
}