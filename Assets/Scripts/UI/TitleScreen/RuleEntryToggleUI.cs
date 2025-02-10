using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace UZSG.UI
{
    public class RuleEntryToggleUI : RuleEntryUI
    {
        public bool Value
        {
            get => toggle.isOn;
            set
            {
                toggle.isOn = value;
            }
        }
        [SerializeField] Toggle toggle;
        public Toggle Toggle => toggle;
    }
}