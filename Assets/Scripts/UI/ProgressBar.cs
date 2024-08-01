using UnityEngine;

namespace UZSG.UI
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField, Range(0, 100)]
        protected float _value;
        public float Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = Mathf.Clamp(value, 0, 100);
                    Refresh();
                }
            }
        }
        
        [Space]
        [SerializeField] protected RectTransform barRect;
        [SerializeField] protected RectTransform fillRect;

        void OnValidate()
        {
            Refresh();
        }

        public virtual void Refresh()
        {
            float x = Mathf.Lerp(barRect.rect.width, 0f, Value / 100f);
            fillRect.offsetMax = new Vector2(-x, fillRect.offsetMax.y);
        }
    }
}
