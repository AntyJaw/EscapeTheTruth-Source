using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace EtT.UI.Placeholder
{
    public sealed class SmartNeedPrompt : MonoBehaviour
    {
        [SerializeField] private CanvasGroup root;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button acceptBtn;
        [SerializeField] private Button declineBtn;
        [SerializeField] private Button adBtn;

        private Action _onAccept;
        private Action _onDecline;
        private Action _onAd;

        private void Awake()
        {
            HideInstant();
            if (acceptBtn) acceptBtn.onClick.AddListener(() => { _onAccept?.Invoke(); Hide(); });
            if (declineBtn) declineBtn.onClick.AddListener(() => { _onDecline?.Invoke(); Hide(); });
            if (adBtn) adBtn.onClick.AddListener(() => { _onAd?.Invoke(); Hide(); });
        }

        public void Show(string header, string body, Action onAccept, Action onDecline, Action onAdBoost)
        {
            _onAccept = onAccept; _onDecline = onDecline; _onAd = onAdBoost;
            if (headerText) headerText.text = header;
            if (bodyText) bodyText.text = body;
            if (root) { root.alpha = 1f; root.blocksRaycasts = true; root.interactable = true; }
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (root) { root.alpha = 0f; root.blocksRaycasts = false; root.interactable = false; }
            gameObject.SetActive(false);
        }

        private void HideInstant() => Hide();
    }
}