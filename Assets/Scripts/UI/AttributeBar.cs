using UnityEngine;
using UZSG.Attributes;

namespace UZSG.UI
{
    public class AttributeBar : ProgressBar
    {
        [SerializeField] protected Attribute attribute;
        public Attribute Attribute => attribute;
        public bool AnimateBufferOnValueDecrease;
        public float BufferDuration = 0.2f;
        public LeanTweenType BufferEase = LeanTweenType.linear;

        [SerializeField] protected RectTransform bufferRect;

        public virtual void BindAttribute(Attribute attr)
        {
            if (attr == null || !attr.IsValid) return;

            attribute = attr;
            attribute.OnValueChanged += OnValueChanged;
            Value = attribute.ValueMaxRatio * BarMax;
            RefreshBuffer();
            Rebuild();
        }

        protected virtual void OnValueChanged(object sender, AttributeValueChangedContext ctx)
        {
            RefreshFromAttributeContext(ctx);
        }

        public void RefreshFromAttributeContext(AttributeValueChangedContext ctx)
        {
            Value = attribute.ValueMaxRatio * BarMax;

            if (AnimateBufferOnValueDecrease && ctx.ValueChangedType == Attribute.ValueChangeType.Decreased)
            {
                AnimateBuffer(ctx);
            }
        }

        void AnimateBuffer(AttributeValueChangedContext ctx)
        {
            float start = Mathf.Lerp(barRect.rect.width, BarMin, ctx.Previous / BarMax);
            float end = Mathf.Lerp(barRect.rect.width, BarMin, ctx.New / BarMax);

            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject, start, end, BufferDuration)
            .setEase(BufferEase)
            .setOnUpdate((float x) =>
            {
                bufferRect.offsetMax = new Vector2(-x, bufferRect.offsetMax.y);
            });
        }

        public void RefreshBuffer()
        {
            float x = Mathf.Lerp(barRect.rect.width, BarMin, Value / BarMax);
            bufferRect.offsetMax = new Vector2(-x, bufferRect.offsetMax.y);
        }

        public void Flash()
        {
            /// flash bar indicating something?
        }
    }
}
