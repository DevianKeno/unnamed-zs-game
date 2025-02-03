using System;

using UnityEngine;

using UZSG.Data;

namespace UZSG.UI
{
    public class SettingEntryDropdownUI : SettingEntryUI
    {
        [SerializeField] DropdownUI dropdown;
        public DropdownUI Dropdown => dropdown;
        public override object Value => dropdown.value;

        protected override void Awake()
        {
            base.Awake();
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        void OnDropdownValueChanged(int index)
        {
            MarkDirty(this);
        }

        public void SetSelected(int index)
        {
            dropdown.SetValueWithoutNotify(index);
        }
    }
}