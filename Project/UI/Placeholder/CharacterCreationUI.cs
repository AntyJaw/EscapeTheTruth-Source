using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EtT.Data;
using EtT.Services;
using EtT.PlayerStats;

namespace EtT.UI.Placeholder
{
    public sealed class CharacterCreationUI : MonoBehaviour
    {
        // Sygnał dla AppFlowController
        public static event System.Action OnCharacterCreated;

        [Header("Dropdowny")]
        [SerializeField] private TMP_Dropdown classDropdown;
        [SerializeField] private TMP_Dropdown genderDropdown;

        [Header("Opis")]
        [SerializeField] private TMP_Text descText;

        [Header("Atrybuty (wartości)")]
        [SerializeField] private TMP_Text perceptionVal;
        [SerializeField] private TMP_Text techniqueVal;
        [SerializeField] private TMP_Text composureVal;
        [SerializeField] private TMP_Text remainingVal;

        [Header("Przyciski +/-")]
        [SerializeField] private Button perPlus;  [SerializeField] private Button perMinus;
        [SerializeField] private Button tecPlus;  [SerializeField] private Button tecMinus;
        [SerializeField] private Button comPlus;  [SerializeField] private Button comMinus;

        [Header("Potwierdzenie")]
        [SerializeField] private Button confirmBtn;

        [Header("Definicje")]
        [SerializeField] private ClassDefinition[] classes;

        private ClassDefinition _selectedClass;
        private GenderType _selectedGender = GenderType.Male;

        private EtT.PlayerStats.Attributes _tempAttr; // chwilowa kopia do UI
        private int _tempRemaining;
        private int _maxAttr;

        private void Start()
        {
            // Balans
            var ps = ServiceLocator.Get<IPlayerService>() as Player.PlayerService;
            var balField = typeof(Player.PlayerService).GetField("_balance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var balance = balField?.GetValue(ps) as EtT.Data.PlayerBalanceConfig;
            _tempRemaining = balance != null ? balance.startingAttributePoints : 5;
            _maxAttr = balance != null ? balance.maxAttributeValue : 10;

            // Dropdown: Klasa
            classDropdown.ClearOptions();
            foreach (var c in classes) classDropdown.options.Add(new TMP_Dropdown.OptionData(c.displayName));
            classDropdown.onValueChanged.AddListener(i => SelectClass(i));

            // Dropdown: Płeć (M/K)
            genderDropdown.ClearOptions();
            genderDropdown.options.Add(new TMP_Dropdown.OptionData("Mężczyzna"));
            genderDropdown.options.Add(new TMP_Dropdown.OptionData("Kobieta"));
            genderDropdown.onValueChanged.AddListener(i => SelectGender(i));

            // Przyciski
            perPlus.onClick.AddListener(()=> Inc(EtT.PlayerStats.AttributeType.Perception));
            perMinus.onClick.AddListener(()=> Dec(EtT.PlayerStats.AttributeType.Perception));
            tecPlus.onClick.AddListener(()=> Inc(EtT.PlayerStats.AttributeType.Technique));
            tecMinus.onClick.AddListener(()=> Dec(EtT.PlayerStats.AttributeType.Technique));
            comPlus.onClick.AddListener(()=> Inc(EtT.PlayerStats.AttributeType.Composure));
            comMinus.onClick.AddListener(()=> Dec(EtT.PlayerStats.AttributeType.Composure));

            confirmBtn.onClick.AddListener(Confirm);

            SelectClass(0);
            SelectGender(0);
        }

        private void SelectClass(int idx)
        {
            _selectedClass = classes[idx];
            if (descText) descText.text = _selectedClass.description;

            // Bazowe atrybuty z klasy + bonus płci do podglądu
            _tempAttr = new EtT.PlayerStats.Attributes
            {
                Perception = _selectedClass.perception + (_selectedGender == GenderType.Female ? 1 : 0),
                Technique  = _selectedClass.technique,
                Composure  = _selectedClass.composure + (_selectedGender == GenderType.Male ? 1 : 0)
            };

            // Reset puli (5 pkt)
            var ps = ServiceLocator.Get<IPlayerService>() as Player.PlayerService;
            var balField = typeof(Player.PlayerService).GetField("_balance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var balance = balField?.GetValue(ps) as EtT.Data.PlayerBalanceConfig;
            _tempRemaining = balance != null ? balance.startingAttributePoints : 5;

            RefreshUI();
        }

        private void SelectGender(int idx)
        {
            _selectedGender = idx == 0 ? GenderType.Male : GenderType.Female;
            // przelicz atrybuty dla tej samej klasy z bonusem płci
            SelectClass(classDropdown.value);
        }

        private void Inc(EtT.PlayerStats.AttributeType t)
        {
            if (_tempRemaining <= 0) return;
            int cur = _tempAttr.Get(t);
            if (cur >= _maxAttr) return;
            _tempAttr.Set(t, cur+1);
            _tempRemaining--;
            RefreshUI();
        }

        private void Dec(EtT.PlayerStats.AttributeType t)
        {
            int cur = _tempAttr.Get(t);
            // Nie schodzimy poniżej baz (klasa + bonus płci)
            int basePer = _selectedClass.perception + (_selectedGender == GenderType.Female ? 1 : 0);
            int baseTec = _selectedClass.technique;
            int baseCom = _selectedClass.composure + (_selectedGender == GenderType.Male ? 1 : 0);
            int baseVal = t switch { EtT.PlayerStats.AttributeType.Perception => basePer, EtT.PlayerStats.AttributeType.Technique => baseTec, _ => baseCom };
            if (cur <= baseVal) return;

            _tempAttr.Set(t, cur-1);
            _tempRemaining++;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (perceptionVal) perceptionVal.text = _tempAttr.Perception.ToString();
            if (techniqueVal)  techniqueVal.text = _tempAttr.Technique.ToString();
            if (composureVal)  composureVal.text = _tempAttr.Composure.ToString();
            if (remainingVal)  remainingVal.text = _tempRemaining.ToString();

            confirmBtn.interactable = _tempRemaining == 0;
        }

        private void Confirm()
        {
            // 1) Nadaj klasę + płeć na profilu (ustawia bazę i punkty)
            var ps = ServiceLocator.Get<IPlayerService>() as Player.PlayerService;
            ps.SelectClass(_selectedClass, _selectedGender);

            // 2) Nadpisz końcowe atrybuty na profilu tym, co wyklikał gracz
            var attrField = typeof(Player.PlayerService).GetField("_attributes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attrField?.SetValue(ps, _tempAttr);

            // 3) Wyzeruj nieprzydzielone (wszystko rozdane)
            var remField = typeof(Player.PlayerService).GetField("_unspentPoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            remField?.SetValue(ps, 0);

            // 4) Flaga „profil gotowy” jest już ustawiona w SelectClass
            // 5) Powiadom flow + schowaj panel
            OnCharacterCreated?.Invoke();
            EtT.Core.GameEvents.RaiseAttributesChanged();
            gameObject.SetActive(false);
        }
    }
}