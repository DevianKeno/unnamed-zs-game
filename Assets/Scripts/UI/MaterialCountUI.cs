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
                countText.text = $"{value}/{needed}";
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
                countText.text = $"{present}/{value}";
            }
        }

        [Space]
        [SerializeField] TextMeshProUGUI countText;

        void Start()
        {
            itemDisplayUI.DisplayCount(false);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            countText.text = $"{present}/{needed}";
        }
    }
}