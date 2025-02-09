// ProgressBarUI.cs
using UnityEngine;

namespace UZSG.UI
{
    public class ProgressBarUI : UIElement
    {
        public const float BarMin = 0f;
        public const float BarMax = 1f;

        [SerializeField, Range(BarMin, BarMax)] protected float value;
        
        /// <summary>
        /// Set to a value between 0 and 1.
        /// </summary>
        public float Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    this.value = Mathf.Clamp(value, BarMin, BarMax);
                    RefreshBar();
                }
            }
        }

        [Space]
        [SerializeField] protected RectTransform barRect;
        [SerializeField] protected RectTransform fillRect;

        protected override void OnValidate()
        {
            base.OnValidate();
            RefreshBar();
        }

        public void RefreshBar()
        {
            // Simplified calculation using direct 0-1 range
            float x = barRect.rect.width * (1 - Value);
            fillRect.offsetMax = new Vector2(-x, fillRect.offsetMax.y);
        }
    }
}