using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace UZSG.UI
{
    public class SliderValueDisplayUI : UIElement
    {
        public bool IsHundred;

        [SerializeField] Slider slider;
        [SerializeField] TextMeshProUGUI textTmp;

        protected override void Awake()
        {
            base.Awake();
            textTmp = GetComponent<TextMeshProUGUI>();
        }

        internal void OnSliderValueChanged(float value)
        {
            if (IsHundred)
            {
                textTmp.text = Mathf.FloorToInt(value * 100).ToString();
            }
            else
            {
                textTmp.text = Mathf.FloorToInt(value).ToString();
            }
        }

        public void Refresh()
        {
            OnSliderValueChanged(slider.value);
        }
    }
}
