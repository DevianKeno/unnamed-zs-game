using System;
using UnityEngine;
using TMPro;
using UZSG.Systems;

namespace UZSG.UI
{
    public class ConsoleUI : MonoBehaviour
    {
        bool _isInitialized;
        bool _isVisible;
        public bool IsVisible => _isVisible;

        [SerializeField] TextMeshProUGUI messages;
        [SerializeField] TMP_InputField inputField;

        public event Action<bool> OnToggle;

        void OnEnable()
        {
            Initialize();
        }
        
        void OnDisable()
        {
            inputField.onEndEdit.RemoveListener(InputSubmit);
            Game.Console.OnLogMessage -= UpdateMessages;
        }

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            inputField.onEndEdit.AddListener(InputSubmit);
            Game.Main.OnLateInit += FinishInit;
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
            
            Game.Console.Run(input);
            inputField.text = "";
            inputField.Select();
        }

        public void ToggleWindow(bool isVisible)
        {
            OnToggle?.Invoke(isVisible);
            _isVisible = isVisible;
            gameObject.SetActive(isVisible);
        }

        public void ToggleWindow()
        {
            ToggleWindow(!_isVisible);
        }
    }
}
