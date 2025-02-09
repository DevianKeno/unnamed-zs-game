using UnityEngine;

using TMPro;

namespace UZSG.UI
{
    public class MaterialCountUI : ItemSlotUI
    {
        [SerializeField] int present;
        /// <summary>
        /// The left hand side of the count.
        /// </summary>
        public int Present
        {
            get
            {
                return present;
            }
            set
            {
                present = value;
                if (present < needed) countText.text = $"<color=\"red\">{present}</color>/{needed}";
                else countText.text = $"{present}/{needed}";
            }
        }
        /// <summary>
        /// The right hand side of the count.
        /// </summary>
        [SerializeField] int needed;
        public int Needed
        {
            get
            {
                return needed;
            }
            set
            {
                needed = value;
                if (present < needed) countText.text = $"<color=\"red\">{present}</color>/{needed}";
                else countText.text = $"{present}/{needed}";
            }
        }

        [Space]
        [SerializeField] TextMeshProUGUI countText;

        void Start()
        {
            itemDisplayUI.SetCountDisplayed(false);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            countText.text = $"{present}/{needed}";
        }
    }
}