using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UZSG.UI
{
    public class SettingEntrySliderUI : SettingEntryUI
    {
        [SerializeField] Slider slider;
        public Slider Slider => slider;
        [SerializeField] TextMeshProUGUI valueTextTmp;
        public string ValueText
        {
            get => valueTextTmp.text;
            set => valueTextTmp.text = value;
        }
        public override object Value => slider.value;

        protected override void Awake()
        {
            base.Awake();
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        void OnSliderValueChanged(float value)
        {
            MarkDirty(this);
        }

        public void SetSelected(int index)
        {
            
        }
    }
}