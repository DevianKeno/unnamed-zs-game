using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace UZSG.UI
{
    public class KeyButtonUI : UIElement
    {
        [SerializeField, Range(0, 1)] float progress;
        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                UpdateFill();
            }
        }
        
        [Header("UI Elements")]
        [SerializeField] TextMeshProUGUI labelTmp;
        [SerializeField] Image fillImage;

        protected override void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                UpdateFill();
            }
        }

        void UpdateFill()
        {
            fillImage.fillAmount = this.progress;
        }
    }
}