using System;

using UnityEngine;

using TMPro;

namespace UZSG.UI
{
    public class RuleEntryInputFieldUI : RuleEntryUI
    {
        public string Value
        {
            get => inputField.text;
            set
            {
                inputField.text = value;
            }
        }
        [SerializeField] TMP_InputField inputField;
        public TMP_InputField InputField => inputField;
    }
}