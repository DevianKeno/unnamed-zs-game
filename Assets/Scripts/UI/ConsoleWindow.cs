using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;


using UnityEngine.EventSystems;

namespace UZSG.UI
{
    public class ConsoleWindow : Window
    {
        int _navigatingIndex = 0;
        string _inputBuffer;
        List<string> _previousInputs = new();

        [SerializeField] TextMeshProUGUI messages;
        [SerializeField] TMP_InputField inputField;

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

            inputField.onSubmit.AddListener(OnInputFieldSubmit);
            
            Game.Console.OnLogMessage += UpdateMessages;
            messages.text = string.Join(' ', Game.Console.Messages);
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

        protected override void OnShow()
        {
            Game.UI.SetCursorVisible(true);
            inputField.ActivateInputField();
        }

        protected override void OnHide()
        {
            _inputBuffer = string.Empty;
            _navigatingIndex = 0;
            Game.UI.SetCursorVisible(false);
        }
        
        void UpdateMessages(string message)
        {
            messages.text += message;
        }

        void OnInputFieldSubmit(string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            
            _previousInputs.Add(inputField.text);
            _inputBuffer = string.Empty;
            _navigatingIndex = 0;
            inputField.text = string.Empty;
            inputField.ActivateInputField();
            
            Game.Console.RunCommand(input);
        }
    }
}
