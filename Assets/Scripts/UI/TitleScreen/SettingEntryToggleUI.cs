using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UZSG.UI
{
    public class SettingEntryToggleUI : SettingEntryUI
    {
        public new bool Value => toggle.isOn;

        [SerializeField] Toggle toggle;
        public Toggle Toggle => toggle;
    }
}