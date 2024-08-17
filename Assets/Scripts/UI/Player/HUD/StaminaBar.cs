using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UZSG.Attributes;

namespace UZSG.UI
{
    public class StaminaBar : AttributeBar
    {
        public float FadeSeconds = 0.5f;

        bool _isFading;
        bool _isFullyVisible;
        float _originalAlphaBar;
        float _originalAlphaFill;
        float _originalAlphaBuffer;

        [SerializeField] Attribute stamina;
        
        [SerializeField] Image barImage;
        [SerializeField] Image fillImage;
        [SerializeField] Image bufferImage;

        void Start()
        {
            _originalAlphaBar = barImage.color.a;
            _originalAlphaFill = fillImage.color.a;
            _originalAlphaBuffer = bufferImage.color.a;

            FadeAlpha(barImage, 0f, 0f);
            FadeAlpha(fillImage, 0f, 0f);
            FadeAlpha(bufferImage, 0f, 0f);
        }

        public override void BindAttribute(Attribute attr)
        {
            if (attr == null || !attr.IsValid) return;

            stamina = attr;
            stamina.OnValueChanged += OnValueChanged;
            stamina.OnReachMaximum += OnReachMaximum;
            Refresh();
        }

        protected override void OnValueChanged(object sender, AttributeValueChangedContext info)
        {
            if (!_isFullyVisible && !_isFading)
            {
                StartCoroutine(FadeIn(FadeSeconds));
            }

            Value = stamina.ValueMaxRatio * 100f;

            if (BufferOnDecreaseValue && info.ValueChangedType == Attribute.ValueChangeType.Decreased)
            {
                float start = Mathf.Lerp(barRect.rect.width, 0f, info.Previous / 100f);
                float end = Mathf.Lerp(barRect.rect.width, 0f, info.New / 100f);

                LeanTween.cancel(gameObject);
                LeanTween.value(gameObject, start, end, BufferDuration)
                .setOnUpdate((float x) =>
                {
                    bufferRect.offsetMax = new Vector2(-x, bufferRect.offsetMax.y);
                })
                .setEase(TweenType);
            }
        }

        void OnReachMaximum(object sender, AttributeValueChangedContext e)
        {
            StartCoroutine(FadeOut(FadeSeconds));
        }

        IEnumerator FadeIn(float duration)
        {
            _isFading = true;

            FadeAlpha(barImage, _originalAlphaBar, duration);
            FadeAlpha(fillImage, _originalAlphaFill, duration);
            FadeAlpha(bufferImage, _originalAlphaBuffer, duration);

            yield return new WaitForSeconds(duration);
            _isFullyVisible = true;
            _isFading = false;
        }

        IEnumerator FadeOut(float duration)
        {
            _isFading = true;
            _isFullyVisible = false;
            yield return new WaitForSeconds(1f);

            FadeAlpha(barImage, 0f, duration);
            FadeAlpha(fillImage, 0f, duration);
            FadeAlpha(bufferImage, 0f, duration);

            yield return new WaitForSeconds(duration);
            _isFading = false;
        }

        void FadeAlpha(Image image, float targetAlpha, float duration)
        {
            LeanTween.cancel(image.gameObject);
            LeanTween.alpha(image.rectTransform, targetAlpha, duration);
        }
    }
}
