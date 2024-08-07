using UnityEngine;
using UZSG.Attributes;

namespace UZSG.UI
{
    public class AttributeBar : ProgressBar
    {
        [SerializeField] protected Attribute attribute;
        public Attribute Attribute => attribute;
        public bool BufferOnDecreaseValue;
        public float BufferDuration = 0.2f;
        public LeanTweenType TweenType = LeanTweenType.linear;

        [SerializeField] protected RectTransform bufferRect;

        public void BindAttribute(Attribute attr)
        {
            if (attr == null || !attr.IsValid) return;

            attribute = attr;
            attribute.OnValueChanged += OnValueChanged;
            Refresh();
        }

        void OnValueChanged(object sender, AttributeValueChangedContext info)
        {
            Value = attribute.ValueMaxRatio * 100f;

            if (BufferOnDecreaseValue && info.ValueChangedType == Attribute.ValueChangedType.Decreased)
            {
                float start = Mathf.Lerp(barRect.rect.width, 0f, info.Previous / 100f);
                float end = Mathf.Lerp(barRect.rect.width, 0f, info.New / 100f);

                LeanTween.cancel(gameObject);
                LeanTween.value(gameObject, start, end, BufferDuration)
                .setEase(TweenType)
                .setOnUpdate((float x) =>
                {
                    bufferRect.offsetMax = new Vector2(-x, bufferRect.offsetMax.y);
                });
            }
        }

        public override void Refresh()
        {
            base.Refresh();

            float x = Mathf.Lerp(barRect.rect.width, 0f, Value / 100f);
            bufferRect.offsetMax = new Vector2(-x, bufferRect.offsetMax.y);
        }

        public void Flash()
        {
            /// flash bar indicating something?
        }
    }
}
