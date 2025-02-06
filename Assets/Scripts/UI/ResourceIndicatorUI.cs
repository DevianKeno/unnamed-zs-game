using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Interactions;
using UZSG.Entities;
using UZSG.Objects;
using UZSG.Data;


namespace UZSG.UI
{
    public class ResourceIndicatorUI : UIElement
    {
        public string Label
        {
            get => resourceText.text;
            set => resourceText.text = value;
        }
        public Sprite Icon
        {
            get => iconImage.sprite;
            set => iconImage.sprite = value;
        }

        [SerializeField] Image iconImage;
        [SerializeField] TextMeshProUGUI resourceText;
        
        Sprite GetToolTypeIcon(ToolType type)
        {
            var name = type switch
            {
                ToolType.Any => "tool_any",
                ToolType.Axe => "tool_axe",
                ToolType.Pickaxe => "tool_pickaxe",
                ToolType.Shovel => "tool_shovel",
                _ => "hand"
            };

            return Game.UI.GetIcon(name);
        }

        public void DisplayResource(Resource resource)
        {
            Label = resource.ResourceData.DisplayName;
            Icon = GetToolTypeIcon(resource.ResourceData.ToolType);
            Rebuild();
        }
    }
}
