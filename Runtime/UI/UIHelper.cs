using System;
using System.Collections.Generic;
using System.Linq;
using Evbishop.Runtime.UI;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace JEGStudios.UI
{
    [CreateAssetMenu(fileName = "UIHelper", menuName = "Scriptable Objects/Singletons/UI Helper")]
    public class UIHelper : SerializedScriptableObject
    {
        #region Singleton
        private static string s_assetName => nameof(UIHelper);
        private static string s_loadPath => $"Singletons/{s_assetName}";
        private static string s_savePath => $"Assets/Resources/{s_loadPath}.asset";

        private static UIHelper s_instance;

        public static UIHelper Instance
        {
            get
            {
                if (s_instance != null)
                    return s_instance;

                s_instance = Resources.Load<UIHelper>(s_loadPath);
                if (s_instance != null)
                    return s_instance;

                s_instance = CreateInstance<UIHelper>();
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(s_instance, s_savePath);
#endif
                return s_instance;
            }
        }
        #endregion

        [field: SerializeField, AssetsOnly] public InputActionReference InputBackButton { get; private set; }
        [SerializeField, AssetsOnly] private TextAsset _profanityAsset;
        [SerializeField] private LocalizedString _keyErrorProfanity;
        public string ProfanityErrorMessage => _keyErrorProfanity.GetLocalizedString();

        private HashSet<string> _profanityWords = new();

        public List<UIPopup> BackButtonBlockers { get; private set; }

        public event Action OnBackButtonPressed;

        public void Init()
        {
            BackButtonBlockers = new();
            OnBackButtonPressed = null;
            InputBackButton.action.performed -= HandleBackButtonPressed;
            InputBackButton.action.performed += HandleBackButtonPressed;
            InitializeProfanityList();
        }

        private void InitializeProfanityList()
        {
            if (_profanityAsset != null)
            {
                string[] words = _profanityAsset.text.Split('\n');
                foreach (string word in words)
                {
                    string trimmedWord = word.Trim().ToLower();
                    if (!string.IsNullOrEmpty(trimmedWord))
                    {
                        _profanityWords.Add(trimmedWord);
                    }
                }
            }
        }

        public bool HasProfanity(string input, out string message)
        {
            message = string.Empty;
            if (string.IsNullOrEmpty(input))
                return false;

            string normalizedInput = input.ToLower();

            if (_profanityWords.Any(word => normalizedInput.Contains(word)))
            {
                message = ProfanityErrorMessage;
                return true;
            }

            return false;
        }

        private void HandleBackButtonPressed(InputAction.CallbackContext context)
        {
            ButtonBack();
        }

        public void ButtonBack()
        {
            if (BackButtonBlockers.Count > 0)
            {
                var lastBlocker = BackButtonBlockers[^1];
                if (lastBlocker.HideByBackButton)
                {
                    lastBlocker.Hide();
                }
                return;
            }
            OnBackButtonPressed?.Invoke();
        }

        public void BlockBackButton(UIPopup popup)
        {
            if (BackButtonBlockers.Contains(popup)) // move the already present popup to the end of the blockers list
            {
                BackButtonBlockers.Remove(popup);
            }
            BackButtonBlockers.Add(popup);
        }

        public void UnblockBackButton(UIPopup popup)
        {
            if (BackButtonBlockers.Contains(popup))
            {
                BackButtonBlockers.Remove(popup);
            }
        }
    }
}