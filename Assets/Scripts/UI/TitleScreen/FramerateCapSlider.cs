using UnityEngine;
using UnityEngine.UI;

namespace UZSG.UI
{
    public class FramerateCapSlider : MonoBehaviour
    {
        [SerializeField] SettingEntrySliderUI settingEntrySlider;

        void Awake()
        {
            settingEntrySlider = GetComponent<SettingEntrySliderUI>();
            settingEntrySlider.Slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
        void OnSliderValueChanged(float value)
        {
            if (value > settingEntrySlider.Slider.maxValue - 1)
            {
                settingEntrySlider.ValueText = "Unlimited";
                //TODO: settingEntrySlider.ValueText = Game.Locale.Translatable("setting.framerate_cap.unlimited");
            }
        }
    }
}