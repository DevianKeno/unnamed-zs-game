using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using TMPro;

using UZSG.Worlds;

namespace UZSG.UI
{
    public class ChatManagerUI : UIElement
    {
        public bool IsActive { get; private set; }
        public bool HasInput
        {
            get => !string.IsNullOrWhiteSpace(inputField.text);
        }

        int _navigatingIndex = 0;
        float _originalSizeDeltaY;
        string _inputBuffer;
        List<string> _previousInputs = new();

        ChatManager chatManager;

        [Header("Elements")]
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] TextMeshProUGUI messages;
        [SerializeField] Mask messageMask;
        [SerializeField] TMP_InputField inputField;
        [SerializeField] Image messagesBackground;
        [SerializeField] Image inputFieldBackground;
        
        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();

        void Start()
        {
            actionMap = Game.Main.GetActionMap("Console Window");
            inputs = Game.Main.GetActionsFromMap(actionMap);

            var prevInput = inputs["Navigate Entry Up"];
            prevInput.performed += NavigatePreviousEntry;
            prevInput.Enable();

            var nextInput = inputs["Navigate Entry Down"];
            nextInput.performed += NavigateNextEntry;
            nextInput.Enable();

            var expandInput = inputs["Expand"];
            expandInput.performed += OnInputExpand;
            expandInput.started += OnInputExpand;
            expandInput.canceled += OnInputExpand;

            inputField.onSubmit.AddListener(OnInputFieldSubmit);
        }
        
        internal void Initialize(ChatManager chatManager)
        {
            this.chatManager = chatManager;
        }

        void NavigatePreviousEntry(InputAction.CallbackContext context)
        {
            if (_navigatingIndex == 0)
            {
                _navigatingIndex = _previousInputs.Count;

                if (!string.IsNullOrEmpty(inputField.text))
                {
                    _inputBuffer = inputField.text;
                }                
            }

            if (_navigatingIndex - 1 >= 0)
            {
                _navigatingIndex--;
                inputField.text = _previousInputs[_navigatingIndex];
            }
        }

        void NavigateNextEntry(InputAction.CallbackContext context)
        {
            if (_navigatingIndex + 1 < _previousInputs.Count)
            {
                _navigatingIndex++;
                inputField.text = _previousInputs[_navigatingIndex];
            }
            else
            {
                _navigatingIndex = 0;
                inputField.text = _inputBuffer;
            }            
        }

        void OnInputExpand(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _originalSizeDeltaY = Rect.sizeDelta.y;
                Rect.sizeDelta = new(Rect.sizeDelta.x, 0f);
            }
            else if (context.canceled)
            {
                Rect.sizeDelta = new(Rect.sizeDelta.x, _originalSizeDeltaY);
            }
        }

        void OnInputFieldSubmit(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                SetInactive();
                return;
            }
            
            _previousInputs.Add(inputField.text);
            _inputBuffer = string.Empty;
            _navigatingIndex = 0;
            inputField.text = string.Empty;
            inputField.ActivateInputField();
            
            chatManager.SendMessage(input);
        }
        

        #region Public

        [ContextMenu("Set Active")]
        public void SetActive()
        {
            IsActive = true;
            scrollRect.enabled = true;
            messageMask.showMaskGraphic = true;
            inputField.gameObject.SetActive(true);
            inputField.ActivateInputField();
        }

        [ContextMenu("Set Inactive")]
        public void SetInactive()
        {
            IsActive = false;
            scrollRect.enabled = false;
            messageMask.showMaskGraphic = false;
            inputField.gameObject.SetActive(false);
        }

        public void AddMessage(string message)
        {
            messages.text += message;
        }

        #endregion
    }
}