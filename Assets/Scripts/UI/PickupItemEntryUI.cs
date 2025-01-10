using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Items;
using System;

namespace UZSG.UI
{
    public class PickupItemEntryUI : Window
    {
        public Item Item;
        public float AnimationFactor = 0.5f;
        public float LifetimeDuration;

        [Header("Animation Settings")]
        [Header("Item Sprite")]
        public LeanTweenType IconEase;

        [Header("Item Name")]
        public float xDistance = 10f;
        public LeanTweenType NameEase;

        float _lifetime;

        [SerializeField] Image bgImage;
        [SerializeField] Image itemImage;
        [SerializeField] TextMeshProUGUI itemNameTMP;
        [SerializeField] TextMeshProUGUI countTMP;

        public event Action<PickupItemEntryUI> OnExpire;

        void Update()
        {
            _lifetime += Time.deltaTime;
            if (_lifetime > LifetimeDuration)
            {
                OnExpire?.Invoke(this);
                Destroy();
            }
        }

        void Start()
        {
            _lifetime = 0f;
        }

        /// <summary>
        /// Set the UIs alpha to 0 and and offsets and stuff
        /// </summary>
        void SetUIForEntry()
        {
            bgImage.color = Color.white;
            bgImage.CrossFadeAlpha(0f, 0f, true);

            itemImage.CrossFadeAlpha(0f, 0f, true);
            itemImage.rectTransform.localScale = Vector3Ext.FromValue(0.95f);

            itemNameTMP.CrossFadeAlpha(0f, 0f, true);
            var nameRect = itemNameTMP.rectTransform.anchoredPosition;
            nameRect.x += xDistance;
            itemNameTMP.rectTransform.anchoredPosition = nameRect;

            countTMP.CrossFadeAlpha(0f, 0f, true);
        }

        void AnimateEntry()
        {
            bgImage.CrossFadeColor(Color.black, AnimationFactor, false, true);
            bgImage.CrossFadeAlpha(1f, AnimationFactor, false);

            itemImage.CrossFadeAlpha(1f, AnimationFactor, true);
            LeanTween.scale(itemImage.rectTransform, Vector3.one, AnimationFactor)
            .setEase(IconEase);

            itemNameTMP.CrossFadeAlpha(1f, 0f, false);
            LeanTween.moveX(itemNameTMP.rectTransform, itemNameTMP.rectTransform.anchoredPosition.x - xDistance, AnimationFactor)
            .setEase(NameEase);

            countTMP.CrossFadeAlpha(1f, AnimationFactor, false);
        }

        void AnimateExit()
        {
            
        }

        protected override void OnShow()
        {
            SetUIForEntry();
            AnimateEntry();
        }

        protected override void OnHide()
        {
            AnimateExit();
        }

        public void SetDisplayedItem(Item item)
        {
            if (item.IsNone) return;
            
            this.Item = new(item);
            itemImage.sprite = item.Data.Sprite;
            itemNameTMP.text = item.Data.DisplayName;
            countTMP.text = item.Count.ToString();
            Rebuild();
        }

        public void IncrementCount(Item item)
        {
            if (!Item.CompareTo(item)) return;

            Item.Combine(item);
            countTMP.text = Item.Count.ToString();
            _lifetime = 0f;
        }
    }
}