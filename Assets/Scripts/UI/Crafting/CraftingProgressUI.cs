using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Items;

namespace UZSG.UI
{
    public class CraftingProgressUI : RadialProgressUI
    {
        [Space]
        public float TimeSingle;
        public float TimeElapsedSingle;

        [Range(0, 1)]
        public float ProgressSingle;

        [SerializeField] protected ItemDisplayUI itemDisplayUI;
        [SerializeField] protected Image fillSingle;
        
        public void SetDisplayedItem(Item item)
        {
            itemDisplayUI.SetDisplayedItem(item);
        }
    }
}
