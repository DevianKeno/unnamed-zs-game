using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UZSG.UI
{
    public class SettingEntrySliderUI : SettingEntryUI
    {
        public string ValueText
        {
            get => valueTextTmp.text;
            set => valueTextTmp.text = value;
        }

        [SerializeField] Slider slider;
        public Slider Slider => slider;
        [SerializeField] TextMeshProUGUI valueTextTmp;
        
        void Start()
        {
            slider.onValueChanged.AddListener((value) => OnValueChangedEvent<float>(value));
        }

        public override void SetValue<T>(T value)
        {
            if (value is float valueFloat)
            {
                slider.value = valueFloat;
            }
        }
    }
}