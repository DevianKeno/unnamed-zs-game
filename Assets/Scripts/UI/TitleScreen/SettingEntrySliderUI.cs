using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UZSG.UI
{
    public class SettingEntrySliderUI : SettingEntryUI
    {
        public new float Value => slider.value;
        public string ValueText
        {
            get => valueTextTmp.text;
            set => valueTextTmp.text = value;
        }

        [SerializeField] Slider slider;
        public Slider Slider => slider;
        [SerializeField] TextMeshProUGUI valueTextTmp;
    }
}