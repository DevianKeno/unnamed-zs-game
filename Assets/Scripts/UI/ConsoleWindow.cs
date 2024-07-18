using System;
using UnityEngine;
using TMPro;
using UZSG.Systems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace UZSG.UI
{
    public class ConsoleWindow : Window
    {
        string buffer;
        int currentIndex = 0;
        List<string> _previousInputs = new();

        [SerializeField] TextMeshProUGUI messages;
        [SerializeField] TMP_InputField inputField;

        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();

        public void Initialize()
        {
            actionMap = Game.Main.GetActionMap("Console Window");
            foreach (var action in actionMap.actions)
            {
                inputs[action.name] = action;
                action.Enable();            
            }

            inputs["Hide/Show"].performed += (ctx) =>
            {
                ToggleVisibility();
            };
            inputs["Navigate Entry Up"].performed += NavigatePreviousEntry;
            inputs["Navigate Entry Down"].performed += NavigateNextEntry;

            inputField.onEndEdit.AddListener(InputSubmit);
            
            Game.Console.OnLogMessage += UpdateMessages;
            messages.text = string.Join('\n', Game.Console.Messages);
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
            }
            else
            {
                currentIndex = 0;
                inputField.text = buffer;
            }            
        }

        public override void OnShow()
        {
            actionMap.Enable();
        }

        public override void OnHide()
        {
            actionMap.Disable();
            buffer = "";
            currentIndex = 0;
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
    }
}
