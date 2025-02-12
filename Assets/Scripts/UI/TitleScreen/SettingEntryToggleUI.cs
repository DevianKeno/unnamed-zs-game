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

        void Start()
        {
            toggle.onValueChanged.AddListener((isOn) => OnValueChangedEvent<bool>(isOn));
        }

        public override void SetValue<T>(T value)
        {
            if (value is bool valueBool)
            {
                toggle.isOn = valueBool;
            }
        }
    }
}