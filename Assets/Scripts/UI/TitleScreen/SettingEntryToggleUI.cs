using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UZSG.UI
{
    public class SettingEntryToggleUI : SettingEntryUI
    {
        [SerializeField] Toggle toggle;
        public Toggle Toggle => toggle;
        public override object Value => toggle.isOn;

        protected override void Awake()
        {
            base.Awake();
            toggle.onValueChanged.AddListener(OnSliderValueChanged);
        }

        void OnSliderValueChanged(bool value)
        {
            MarkDirty(this);
        }

        public void SetSelected(int index)
        {
            
        }
    }
}