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
            stamina = attr;
            stamina.OnReachMaximum += OnReachMaximum;
            
            base.BindAttribute(attr);
        }

        protected override void OnValueChanged(object sender, AttributeValueChangedContext ctx)
        {
            if (!_isFullyVisible && !_isFading)
            {
                StartCoroutine(FadeIn(FadeSeconds));
            }
            
            base.OnValueChanged(sender, ctx);
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
            image.CrossFadeAlpha(targetAlpha, duration, false);
        }
    }
}
