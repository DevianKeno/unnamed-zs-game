using System;
using UnityEngine;

using UZSG.Data;

namespace UZSG.UI
{
    public class SettingEntryDropdownUI : SettingEntryUI
    {
        [SerializeField] Dropdown dropdown;
        public Dropdown Dropdown => dropdown;

        public void SetSelected(int index)
        {
            dropdown.SetValueWithoutNotify(index);
        }
    }
}