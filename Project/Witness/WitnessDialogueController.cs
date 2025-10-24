using UnityEngine;

namespace EtT.Witness
{
    public sealed class WitnessDialogueController : MonoBehaviour
    {
        [Header("Dane świadka")]
        public WitnessProfile profile;

        [Header("Mimika/Anim (opcjonalnie)")]
        public Animator animator;                 // podłącz, jeśli masz
        public SkinnedMeshRenderer faceRenderer;  // blendshape'y (opcjonalnie)

        // Indeksy blendshape'ów (opcjonalne)
        int bsSmile = -1, bsFrown = -1, bsJaw = -1, bsBrowDown = -1;

        void Start()
        {
            if (faceRenderer && faceRenderer.sharedMesh)
            {
                var m = faceRenderer.sharedMesh;
                bsSmile    = m.GetBlendShapeIndex("Smile");
                bsFrown    = m.GetBlendShapeIndex("Frown");
                bsJaw      = m.GetBlendShapeIndex("JawOpen");
                bsBrowDown = m.GetBlendShapeIndex("BrowDown");
            }
        }

        public DialogueTurn Ask(string playerTextOrIntent, float playerReputation01 = 0.5f)
        {
            string intent = IntentLexicon.Map(playerTextOrIntent);

            float honesty = Mathf.Clamp01(profile.honestyBase + 0.25f*(playerReputation01*2f-1f)
                                          - 0.3f*profile.fearOfPerp - 0.2f*profile.hostility);
            bool truth = Random.value < honesty;

            string text = truth ? KnowledgeBase.TrueAnswer(intent, profile)
                                : KnowledgeBase.MisleadingAnswer(intent, profile);

            DriveFaceAndBody(truth);

            return new DialogueTurn { playerUtterance = playerTextOrIntent, intent = intent, systemResponse = text, truthScore = truth ? 1f : 0f };
        }

        private void DriveFaceAndBody(bool truth)
        {
            float nervous = profile.nervousness + (truth ? 0f : 0.5f);

            SetBS(bsBrowDown, Mathf.Lerp(0, 60, nervous));
            SetBS(bsJaw, Mathf.Lerp(5, 15, truth ? 1f : 0f));
            SetBS(bsSmile, Mathf.Lerp(0, 20, Mathf.Max(0, profile.attitudeToPlayer)));
            SetBS(bsFrown, Mathf.Lerp(0, 25, Mathf.Max(0, profile.hostility)));

            if (animator)
                animator.CrossFade(truth ? "Open" : "Defensive", 0.1f);
        }

        private void SetBS(int idx, float val)
        {
            if (idx >= 0 && faceRenderer) faceRenderer.SetBlendShapeWeight(idx, val);
        }
    }

    public struct DialogueTurn
    {
        public string playerUtterance;
        public string intent;
        public string systemResponse;
        public float truthScore; // 0..1
    }
}