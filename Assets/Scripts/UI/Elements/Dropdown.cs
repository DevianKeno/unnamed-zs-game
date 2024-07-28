using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;
using UZSG.Systems;

namespace UZSG.UI
{
    public class Dropdown : TMP_Dropdown
    {
        public float HeightMin = 0f;
        public float HeightMax = 300f;
        public float AnimationFactor = 0.5f;
        public event Action OnClick;
        
        [SerializeField] RectTransform dropdownRect;

        protected override void Awake()
        {
            base.Awake();
            OnClick += Open;
        }

        public override void OnPointerClick(PointerEventData pointerEventData)
        {
            Show();
            OnClick?.Invoke();
        }

        public override void OnCancel(BaseEventData eventData)
        {
            LeanTween.cancel(dropdownRect.gameObject);
            base.OnCancel(eventData);
        }

        protected override GameObject CreateDropdownList(GameObject template)
        {
            GameObject dropdown = base.CreateDropdownList(template);
            dropdownRect = dropdown.GetComponent<RectTransform>();
            return dropdown;
        }

        public void Open()
        {
            if (Game.UI.EnableScreenAnimations)
            {
                LeanTween.value(dropdownRect.gameObject, HeightMin, HeightMax, AnimationFactor)
                .setEaseOutExpo()
                .setOnUpdate( (i) =>
                {
                    dropdownRect.sizeDelta = new(dropdownRect.sizeDelta.x , i);
                });
            }
            else
            {
                dropdownRect.sizeDelta = new(dropdownRect.sizeDelta.x, HeightMax);
            }
        }
    }
}