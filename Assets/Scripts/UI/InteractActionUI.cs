using System;

using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;

using UZSG.Interactions;

namespace UZSG.UI
{
    public class InteractActionUI : UIElement
    {
        /// <summary>
        /// How long before the input goes down again.
        /// </summary>
        const float BUFFER_TIME = 0.25f;

        public string ActionText
        {
            get => actionText.text;
            set
            {
                actionText.text = value;
            }
        }
        public string InteractableText
        {
            get => interactableText.text;
            set
            {
                interactableText.text = value;
            }
        }
        public string Key
        {
            get => keyText.text;
            set
            {
                keyText.text = value;
            }
        }

        bool _enableUpdate = false;
        float _progress;
        InteractAction interactAction = null;
        public InteractAction InteractAction
        {
            get => interactAction;
            set
            {
                if (interactAction != null) return; /// cannot be used twice lol

                SetInteractAction(value);
            }
        }
        public InputAction InputAction { get; set; }

        [Header("UI Elements")]
        [SerializeField] KeyButtonUI keyButton;
        [SerializeField] TextMeshProUGUI actionText;
        [SerializeField] TextMeshProUGUI interactableText;
        [SerializeField] TextMeshProUGUI keyText;

        void Start()
        {
            if (InputAction != null && InputAction.bindings.Count > 0)
            {
                Key = InputAction.bindings[0].ToDisplayString();
            }
        }

        void ListenToInputs()
        {
            if (interactAction.InputAction == null) return;

            interactAction.InputAction.started += OnInput;
            interactAction.InputAction.performed += OnInput;
            interactAction.InputAction.canceled += OnInput;
        }

        void OnDestroy()
        {
            Cleanup();
        }

        void Cleanup()
        {
            if (interactAction.InputAction == null) return;

            interactAction.InputAction.started -= OnInput;
            interactAction.InputAction.performed -= OnInput;
            interactAction.InputAction.canceled -= OnInput;
        }

        bool _isHoldingInput;
        bool _isActivated;
        float _cdTimer;

        void OnInput(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                _cdTimer = BUFFER_TIME;
                _isHoldingInput = true;
                _enableUpdate = true;
            }
            else if (context.phase == InputActionPhase.Performed)
            {
                if (!interactAction.IsHold)
                {
                    interactAction.Perform(new InteractionContext()
                    {
                        Type = interactAction.Type,
                        Interactable = interactAction.Interactable,
                    });
                }
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                _isHoldingInput = false;
            }
        }

        void Update()
        {
            if (!_enableUpdate) return;

            if (_isHoldingInput && interactAction.IsHold && !_isActivated)
            {
                _progress += Time.deltaTime;
                keyButton.Progress = _progress / interactAction.HoldDurationSeconds;

                if (_progress >= interactAction.HoldDurationSeconds)
                {
                    _progress = 1;
                    _isActivated = true;

                    interactAction.Perform(new InteractionContext()
                    {
                        Type = interactAction.Type,
                        Interactable = interactAction.Interactable,
                    });
                }
            }
            else if (keyButton.Progress > 0)
            {
                _isActivated = false;
                _cdTimer -= Time.deltaTime;
                if (_cdTimer < 0)
                {
                    _progress -= Time.deltaTime;
                    keyButton.Progress = Mathf.Max(0, _progress);

                    if (_progress < 0)
                    {
                        _enableUpdate = false;
                    }
                }
            }
        }

        public void SetInteractAction(InteractAction value)
        {
            interactAction = value;
            ActionText = InteractAction.Translatable(interactAction.Type);
            InteractableText = interactAction.Interactable.DisplayName;
            InputAction = interactAction.InputAction;
            _enableUpdate = true;
            ListenToInputs();
        }
    }
}