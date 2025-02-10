using System;

using UnityEngine;

using UZSG.Data;

namespace UZSG.UI
{
    public class SettingEntryDropdownUI : SettingEntryUI
    {
        public new int Value => dropdown.value;

        [SerializeField] DropdownUI dropdown;
        public DropdownUI Dropdown => dropdown;
    }
}