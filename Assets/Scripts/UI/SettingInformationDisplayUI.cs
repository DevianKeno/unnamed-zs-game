using UnityEngine;
using UnityEngine.UI;

using TMPro;

using UZSG.Data;
using UZSG.UI;

namespace UZSG.Systems
{
    public class SettingInformationDisplayUI : UIElement
    {
        [SerializeField] TextMeshProUGUI settingNameTmp;
        [SerializeField] Image settingImage;
        [SerializeField] TextMeshProUGUI impactTmp;
        [SerializeField] TextMeshProUGUI descriptionTmp;

        public void SetSettingData(SettingsEntryData setting)
        {
            if (setting == null)
            {
                Game.Console.LogWarn($"No setting set for SettingEntryUI '{setting.name}'!");
                return;
            }

            settingNameTmp.text = setting.DisplayNameTranslatable;
            impactTmp.text = $"Performance Impact: " + setting.PerformanceImpact.ToString(); /// TODO: make translatable
            descriptionTmp.text = setting.DescriptionTranslatable;
        }
    }
}