using System;
using UnityEngine;
using TMPro;
using UZSG.Systems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace UZSG.UI
{
    public class ConsoleWindow : Window
    {
        string _inputBuffer;
        int _navigatingIndex = 0;
        List<string> _previousInputs = new();

        [SerializeField] TextMeshProUGUI messages;
        [SerializeField] TMP_InputField inputField;

        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();

        void Start()
        {
            actionMap = Game.Main.GetActionMap("Console Window");
            inputs = Game.Main.GetActionsFromMap(actionMap);
            
            inputs["Navigate Entry Up"].performed += NavigatePreviousEntry;
            inputs["Navigate Entry Down"].performed += NavigateNextEntry;

            inputField.onSubmit.AddListener(InputSubmit);
            
            Game.Console.OnLogMessage += UpdateMessages;
            messages.text = string.Join(' ', Game.Console.Messages);
        }

        void NavigatePreviousEntry(InputAction.CallbackContext context)
        {
            if (_navigatingIndex == 0)
            {
                _navigatingIndex = _previousInputs.Count;

                if (inputField.text != "")
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
            actionMap.Enable();
            Game.UI.SetCursorVisible(true);
        }

        protected override void OnHide()
        {
            // actionMap.Disable();
            _inputBuffer = "";
            _navigatingIndex = 0;
            Game.UI.SetCursorVisible(false);
        }
        
        void UpdateMessages(string message)
        {
            messages.text += message;
        }

        void InputSubmit(string input)
        {
            if (input == "") return;
            
            _previousInputs.Add(inputField.text);
            _inputBuffer = "";
            _navigatingIndex = 0;
            inputField.text = "";
            inputField.Select();
            Game.Console.Run(input);
        }
    }
}
