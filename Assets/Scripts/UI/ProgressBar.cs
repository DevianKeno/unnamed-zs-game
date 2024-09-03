using UnityEngine;

namespace UZSG.UI
{
    public class ProgressBar : Window
    {
        public const int BarMin = 0;
        public const int BarMax = 100;

        [SerializeField, Range(BarMin, BarMax)]
        protected float _value;
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    _value = Mathf.Clamp(value, BarMin, BarMax);
                    RefreshBar();
                }
            }
        }
        
        [Space]
        [SerializeField] protected RectTransform barRect;
        [SerializeField] protected RectTransform fillRect;

        void OnValidate()
        {
            RefreshBar();
        }

        public void RefreshBar()
        {
            float x = Mathf.Lerp(barRect.rect.width, BarMin, Value / BarMax);
            fillRect.offsetMax = new Vector2(-x, fillRect.offsetMax.y);
        }
    }
}
