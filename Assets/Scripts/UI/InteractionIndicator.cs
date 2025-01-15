using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Interactions;

namespace UZSG.UI
{
    public class InteractionIndicator : UIElement
    {
        List<InteractActionUI> actions = new();

        [Header("Prefabs")]
        [SerializeField] GameObject actionPrefab;

        protected virtual void Start()
        {
            ClearActions();
        }

        protected override void OnHide()
        {
            base.OnHide();
            ClearActions();
        }

        public void Indicate(IInteractable interactable, List<InteractAction> actions)
        {
            if (interactable == null)
            {
                Hide();
                return;
            }

            ClearActions();
            if (actions != null)
            foreach (InteractAction option in actions)
            {
                AddAction(option);
                option.InputAction.performed += OnInputPerformed;
                
                /// safety net
                void OnInputPerformed(InputAction.CallbackContext context)
                {
                    option.InputAction.performed -= OnInputPerformed;
                }
            }
            Show();
            Rebuild();
        }

        public void AddAction(InteractAction options)
        {
            var go = Instantiate(actionPrefab, parent: transform);
            var action = go.GetComponent<InteractActionUI>();
            action.InteractAction = options;
            actions.Add(action);
            action.Show();
        }

        public void ClearActions()
        {
            foreach (var element in actions)
            {
                element.Destroy();
            }
            actions.Clear();
        }
    }
}
