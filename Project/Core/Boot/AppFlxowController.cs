using UnityEngine;
using EtT.Services;

namespace EtT.Core
{
    /// <summary>
    /// Włącza kreator lub HUD na starcie. Reaguje na zakończenie kreatora.
    /// </summary>
    public sealed class AppFlowController : MonoBehaviour
    {
        [Header("UI Roots")]
        [SerializeField] private GameObject characterCreationRoot;
        [SerializeField] private GameObject hudRoot;

        private void OnEnable()
        {
            UI.Placeholder.CharacterCreationUI.OnCharacterCreated += HandleCharacterCreated;
        }

        private void OnDisable()
        {
            UI.Placeholder.CharacterCreationUI.OnCharacterCreated -= HandleCharacterCreated;
        }

        private void Start()
        {
            var ps = ServiceLocator.Get<IPlayerService>() as Player.PlayerService;
            bool has = ps != null && ps.HasProfile;

            if (characterCreationRoot) characterCreationRoot.SetActive(!has);
            if (hudRoot)               hudRoot.SetActive(has);
        }

        private void HandleCharacterCreated()
        {
            if (characterCreationRoot) characterCreationRoot.SetActive(false);
            if (hudRoot)               hudRoot.SetActive(true);
        }
    }
}