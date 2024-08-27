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
        public float LifetimeDuration;

        float _lifetime;

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

        public override void OnShow()
        {
            /// Animate show
        }

        public override void OnHide()
        {
            /// Animate hide
        }

        public void SetDisplayedItem(Item item)
        {
            if (item.IsNone) return;
            
            this.Item = new(item);
            itemImage.sprite = item.Data.Sprite;
            itemNameTMP.text = item.Data.Name;
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