using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using UZSG.Data;

namespace UZSG.UI
{
    public class CraftableItemUI : Window, IPointerEnterHandler, IPointerExitHandler
    {
        static Color CraftableTextColor = Color.white;
        static Color UncraftableTextColor = Color.gray;

        public RecipeData RecipeData;
        CraftableItemStatus status;
        public CraftableItemStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                
                if (status == CraftableItemStatus.Uncraftable)
                {
                    text.color = UncraftableTextColor;
                }
                else if (status == CraftableItemStatus.Craftable)
                {
                    text.color = CraftableTextColor;
                }
            }
        }

        Color _originalBgColor;

        [Space]
        [SerializeField] Button button;
        [SerializeField] Image background;
        [SerializeField] Image image;
        [SerializeField] TextMeshProUGUI text;

        public event Action<CraftableItemUI> OnClick;

        void Start()
        {
            _originalBgColor = background.color;
            button.onClick.AddListener(() =>
            {
                OnClick?.Invoke(this);
            });
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            background.CrossFadeColor(Color.white, 0.25f, false, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            background.CrossFadeColor(_originalBgColor, 0.25f, false, true);
        }

        public void SetRecipe(RecipeData recipeData)
        {
            RecipeData = recipeData;
            image.sprite = recipeData.Output.Data.Sprite;
            text.text = recipeData.Name;
        }

        public void Enable()
        {
            button.interactable = true;
        }

        public void Disable()
        {
            button.interactable = false;
        }
    }
}