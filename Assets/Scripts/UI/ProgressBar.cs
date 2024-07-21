using UnityEngine;
using TMPro;

namespace UZSG.UI
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 100)]
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
        
        [Space(10)]
        [SerializeField] RectTransform barRect;
        [SerializeField] RectTransform fillRect;

        void OnValidate()
        {
            Refresh();
        }

        public void Refresh()
        {
            fillRect.offsetMax = new(
                -Mathf.Lerp(barRect.rect.width, 0f, Value),
                fillRect.offsetMax.y
            );
        }
    }
}
