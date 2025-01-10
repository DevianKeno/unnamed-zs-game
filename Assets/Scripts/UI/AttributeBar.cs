using UnityEngine;
using UZSG.Attributes;

namespace UZSG.UI
{
    public class AttributeBar : ProgressBarUI
    {
        [SerializeField] protected Attribute attribute;
        public Attribute Attribute => attribute;
        
        public bool IsBuffered { get; set; } = true;
        public float BufferDuration = 0.2f;
        public LeanTweenType BufferEase = LeanTweenType.linear;

        bool _isAlreadyBuffering;

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

            if (IsBuffered && ctx.ValueChangedType == Attribute.ValueChangeType.Decreased)
            {
                AnimateBuffer(ctx);
            }
            else
            {
                RefreshBuffer();
            }
        }

        void AnimateBuffer(AttributeValueChangedContext ctx)
        {
            float start;
            if (_isAlreadyBuffering) /// refresh buffer end point
            {
                start = Mathf.Abs(bufferRect.offsetMax.x);
            }
            else /// new buffer
            {
                start = Mathf.Lerp(barRect.rect.width, BarMin, ctx.Previous / BarMax);
            }
            float end = Mathf.Lerp(barRect.rect.width, BarMin, ctx.New / BarMax);

            _isAlreadyBuffering = true;
            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject, start, end, BufferDuration)
            .setEase(BufferEase)
            .setOnUpdate((float x) =>
            {
                bufferRect.offsetMax = new Vector2(-x, bufferRect.offsetMax.y);
            })
            .setOnComplete(() =>
            {
                _isAlreadyBuffering = false;
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
