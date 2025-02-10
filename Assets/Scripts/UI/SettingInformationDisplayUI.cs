using UnityEngine;
using UnityEngine.UI;

using TMPro;

using UZSG.Data;

namespace UZSG.UI
{
    public class SettingInformationDisplayUI : UIElement
    {
        [SerializeField] TextMeshProUGUI settingNameTmp;
        [SerializeField] Transform imageContainer;
        [SerializeField] Image settingImage;
        [SerializeField] TextMeshProUGUI impactTmp;
        [SerializeField] TextMeshProUGUI descriptionTmp;

        public void SetSettingData(SettingsEntryData setting)
        {
            if (setting == null)
            {
                Game.Console.LogWarn($"No setting set for SettingEntryUI '{setting.name}'!", true);
                return;
            }

            if (setting.Images.Count == 0)
            {
                imageContainer.gameObject.SetActive(false);
            }
            else
            {
                imageContainer.gameObject.SetActive(true);
            }

            settingNameTmp.text = setting.DisplayNameTranslatable;
            impactTmp.text = $"Performance Impact: " + setting.PerformanceImpact.ToString(); /// TODO: make translatable
            descriptionTmp.text = setting.DescriptionTranslatable;
        }
    }
}