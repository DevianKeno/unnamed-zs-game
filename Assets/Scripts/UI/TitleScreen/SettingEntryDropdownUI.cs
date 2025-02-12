using System;

using UnityEngine;

using UZSG.Data;

namespace UZSG.UI
{
    public class SettingEntryDropdownUI : SettingEntryUI
    {
        [SerializeField] DropdownUI dropdown;
        public DropdownUI Dropdown => dropdown;

        void Start()
        {
            dropdown.onValueChanged.AddListener((value) => OnValueChangedEvent<int>(value));
        }
        
        public override void SetValue<T>(T value)
        {
            if (value is int valueInt)
            {
                dropdown.SetValueWithoutNotify(valueInt);
            }
        }
    }
}