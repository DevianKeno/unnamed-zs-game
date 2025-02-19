using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;



namespace UZSG.UI
{
    public class ChoiceWindow : Panel
    {
        string _label = "Label";
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                labelTMP.enabled = true;
                labelTMP.text = value;
            }
        }

        [SerializeField] int _selectedIndex = -1;
        List<Choice> _choices;
        Choice _selected;

        InputActionMap choiceWindowActionMap;
        InputAction navigateInput;
        InputAction submitInput;

        [Space]
        [Header("Components")]
        [SerializeField] TextMeshProUGUI labelTMP;
        [SerializeField] Transform choicesHolder;

        [Space]
        [Header("Child UIs")]
        [SerializeField] GameObject choicePrefab;
        
        protected override void Awake()
        {
            base.Awake();
        }

        public Choice AddChoice(string text)
        {
            var choice = Instantiate(choicePrefab).GetComponent<Choice>();
            choice.transform.SetParent(choicesHolder);
            choice.Label = text;
            return choice;
        }

        public void ClearChoices()
        {
            foreach (Transform t in choicesHolder)
            {
                /// There are other children so destroy only the choices
                if (t.TryGetComponent(out Choice c))
                {
                    Destroy(t.gameObject);
                }
            }
        }
    }
}