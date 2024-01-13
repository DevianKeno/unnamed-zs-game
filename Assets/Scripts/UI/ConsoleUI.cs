using System;
using UnityEngine;
using TMPro;
using UZSG.Systems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace UZSG.UI
{
    public class ConsoleUI : MonoBehaviour
    {
        bool _isInitialized;
        bool _isVisible;
        public bool IsVisible => _isVisible;
        string buffer;
        int currentIndex = 0;
        List<string> _previousInputs = new();
        [SerializeField] TextMeshProUGUI messages;
        [SerializeField] TMP_InputField inputField;
        PlayerInput input;
        InputActionMap consoleWindowInput;
        InputAction up;
        InputAction down;

        public event Action<bool> OnToggle;

        void Awake()
        {
            input = GetComponent<PlayerInput>();
        }

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            consoleWindowInput = input.actions.FindActionMap("Console Window");
            up = consoleWindowInput.FindAction("Up");
            down = consoleWindowInput.FindAction("Down");
            up.performed += NavigatePreviousEntry;
            down.performed += NavigateNextEntry;

            inputField.onEndEdit.AddListener(InputSubmit);
            Game.Main.OnLateInit += FinishInit;
        }

        void NavigatePreviousEntry(InputAction.CallbackContext context)
        {
            if (currentIndex == 0)
            {
                currentIndex = _previousInputs.Count;

                if (inputField.text != "")
                {
                    buffer = inputField.text;
                }                
            }

            if (currentIndex - 1 >= 0)
            {
                currentIndex--;
                inputField.text = _previousInputs[currentIndex];
            }
        }

        void NavigateNextEntry(InputAction.CallbackContext context)
        {
            if (currentIndex + 1 < _previousInputs.Count)
            {
                currentIndex++;
                inputField.text = _previousInputs[currentIndex];
            } else
            {
                currentIndex = 0;
                inputField.text = buffer;
            }            
        }

        void OnEnable()
        {
            Initialize();
            consoleWindowInput.Enable();
        }

        void OnDisable()
        {
            consoleWindowInput.Disable();
            buffer = "";
            currentIndex = 0;
        }
        
        void OnDestroy()
        {
            inputField.onEndEdit.RemoveListener(InputSubmit);
            up.performed -= NavigatePreviousEntry;
            Game.Console.OnLogMessage -= UpdateMessages;
        }

        void FinishInit()
        {
            Game.Console.OnLogMessage += UpdateMessages;
            messages.text = string.Join('\n', Game.Console.Messages);
        }

        void UpdateMessages(string message)
        {
            messages.text += message;
        }

        void InputSubmit(string input)
        {
            if (input == "") return;
            
            _previousInputs.Add(inputField.text);
            buffer = "";
            currentIndex = 0;
            inputField.text = "";
            inputField.Select();
            Game.Console.Run(input);
        }

        public void ToggleWindow(bool isVisible)
        {
            _isVisible = isVisible;
            OnToggle?.Invoke(isVisible);
            Game.UI.ToggleCursor(isVisible);
            gameObject.SetActive(isVisible);
        }

        public void ToggleWindow()
        {
            ToggleWindow(!_isVisible);
        }
    }
}
