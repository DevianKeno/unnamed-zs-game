// AttributeBar.cs
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
            Value = attribute.ValueMaxRatio; // Remove *100
            RefreshBuffer();
            Rebuild();
        }

        protected virtual void OnValueChanged(object sender, AttributeValueChangedContext ctx)
        {
            RefreshFromAttributeContext(ctx);
        }

        public void RefreshFromAttributeContext(AttributeValueChangedContext ctx)
        {
            Value = attribute.ValueMaxRatio; // Remove *100

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
            float end;
            
            if (_isAlreadyBuffering)
            {
                start = Mathf.Abs(bufferRect.offsetMax.x);
                end = barRect.rect.width * (1 - attribute.ValueMaxRatio);
            }
            else
            {
                start = barRect.rect.width * (1 - (ctx.Previous / attribute.CurrentMaximum));
                end = barRect.rect.width * (1 - (ctx.New / attribute.CurrentMaximum));
            }

            _isAlreadyBuffering = true;
            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject, start, end, BufferDuration)
                .setEase(BufferEase)
                .setOnUpdate(x =>
                {
                    bufferRect.offsetMax = new Vector2(-x, bufferRect.offsetMax.y);
                })
                .setOnComplete(() => _isAlreadyBuffering = false);
        }

        public void RefreshBuffer()
        {
            float x = barRect.rect.width * (1 - Value);
            bufferRect.offsetMax = new Vector2(-x, bufferRect.offsetMax.y);
        }

        public void Flash()
        {
            // Flash implementation remains the same
        }
    }
}