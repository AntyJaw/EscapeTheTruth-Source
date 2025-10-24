using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EtT.NPC.Witness.UI
{
    /// <summary>
    /// Minimalny panel przesłuchania:
    /// - Tryb Arcade: przyciski intencji,
    /// - Tryb Detektyw: pole tekstowe → mapowane do intencji,
    /// - Wyświetla odpowiedź + wskaźnik prawdy.
    /// </summary>
    public sealed class WitnessPanelController : MonoBehaviour
    {
        [Header("Refy UI")]
        [SerializeField] private TMP_Text npcName;
        [SerializeField] private TMP_Text answerText;
        [SerializeField] private Slider truthSlider;
        [SerializeField] private TMP_InputField detectiveInput;

        [Header("Przyciski Arcade")]
        [SerializeField] private Button btnAlibi;
        [SerializeField] private Button btnWhereNight;
        [SerializeField] private Button btnKnowVictim;
        [SerializeField] private Button btnSuspicious;
        [SerializeField] private Button btnPhoneData;

        [Header("Źródło logiki")]
        [SerializeField] private EtT.NPC.Witness.WitnessDialogueController controller;

        void Awake()
        {
            Wire(btnAlibi,       () => AskArcade(EtT.NPC.Witness.IntentLexicon.INTENT_ALIBI));
            Wire(btnWhereNight,  () => AskArcade(EtT.NPC.Witness.IntentLexicon.INTENT_WHERE_NIGHT));
            Wire(btnKnowVictim,  () => AskArcade(EtT.NPC.Witness.IntentLexicon.INTENT_KNOW_VICTIM));
            Wire(btnSuspicious,  () => AskArcade(EtT.NPC.Witness.IntentLexicon.INTENT_SUSPICIOUS));
            Wire(btnPhoneData,   () => AskArcade(EtT.NPC.Witness.IntentLexicon.INTENT_PHONE_DATA));
        }

        private void Wire(Button b, System.Action a)
        {
            if (b != null) b.onClick.AddListener(() => a());
        }

        public void AskDetectiveFromInput()
        {
            var text = detectiveInput != null ? detectiveInput.text : "";
            var turn = controller.AskDetective(text);
            RenderTurn(turn);
        }

        private void AskArcade(string intent)
        {
            var turn = controller.AskArcade(intent);
            RenderTurn(turn);
        }

        private void RenderTurn(EtT.NPC.Witness.DialogueTurn t)
        {
            if (answerText) answerText.text = t.response;
            if (truthSlider) truthSlider.value = t.truthScore;
        }

        // Podaj wyświetlaną nazwę/NPC info
        public void BindProfile(EtT.NPC.Witness.WitnessProfile p)
        {
            if (controller) controller.profile = p;
            if (npcName) npcName.text = p ? p.witnessId : "NPC";
        }
    }
}