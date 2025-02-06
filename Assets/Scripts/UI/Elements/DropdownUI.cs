using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;


using UnityEngine.UI;

namespace UZSG.UI
{
    public class DropdownUI : TMP_Dropdown
    {
        public float HeightMin = 300f;
        public float HeightMax = 900f;
        public event Action OnClick;

        Vector2 _itemSizeDelta = default;
        Vector2 _contentSizeDelta;
        ScrollRect _currScrollRect;
        /// <summary>
        /// The instantiated dropdown list rect.
        /// </summary>
        RectTransform dropdownListRect;
        
        /// <summary>
        /// This dropdown rect.
        /// </summary>
        [SerializeField] RectTransform dropdownRect;
        [SerializeField] TMP_Dropdown tmpDropdown;

        protected override void Awake()
        {
            base.Awake();

            dropdownRect = this.transform as RectTransform;
            OnClick += AnimateDropdown;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            OnClick?.Invoke();
        }

        public override void OnCancel(BaseEventData eventData)
        {
            LeanTween.cancel(dropdownListRect.gameObject);
            base.OnCancel(eventData);
        }

        protected override GameObject CreateDropdownList(GameObject template)
        {
            GameObject dropdownList = base.CreateDropdownList(template);

            dropdownListRect = dropdownList.GetComponent<RectTransform>();
            _currScrollRect = dropdownList.GetComponent<ScrollRect>();
            _contentSizeDelta = _currScrollRect.content.sizeDelta;
            _itemSizeDelta = new(dropdownRect.sizeDelta.x, dropdownRect.sizeDelta.y);
            
            return dropdownList;
        }

        // protected override DropdownItem CreateItem(DropdownItem itemTemplate)
        // {
        //     DropdownItem dropdownItem = base.CreateItem(itemTemplate);

        //     /// Override the item's height to match the dropdown's actual height
        //     var itemRect = dropdownItem.transform as RectTransform;
        //     itemRect.sizeDelta = _itemSizeDelta;

        //     return dropdownItem;
        // }

        public void AnimateDropdown()
        {
            UpdateItemSizeDelta();
            /// min height or max height
            float targetHeight = Mathf.Min(_itemSizeDelta.y * options.Count, HeightMax);
            /// scroll to current selected option
            float contentHeight = _itemSizeDelta.y * options.Count;
            float selectedItemPosition = _itemSizeDelta.y * this.value;
            float normalizedPosition = 1 - Mathf.Clamp01(selectedItemPosition / contentHeight);
            _currScrollRect.verticalNormalizedPosition = normalizedPosition;

            if (Game.UI.EnableScreenAnimations)
            {
                LeanTween.value(dropdownListRect.gameObject, 0f, targetHeight, Game.UI.GlobalAnimationFactor)
                .setEaseOutExpo()
                .setOnUpdate((i) =>
                {
                    dropdownListRect.sizeDelta = new Vector2(dropdownListRect.sizeDelta.x, i);
                });
            }
            else
            {
                dropdownListRect.sizeDelta = new Vector2(dropdownListRect.sizeDelta.x, targetHeight);
            }
        }

        void UpdateItemSizeDelta()
        {
            (itemText.transform.parent as RectTransform).sizeDelta = _itemSizeDelta;
        }
    }
}