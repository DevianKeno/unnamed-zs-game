using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UZSG.UI.HUD
{
    public class MinimapUI : Window
    {
        public float Duration;
        public float Delay;
        public LeanTweenType Ease;

        [SerializeField] Image bgImage;

        void Start()
        {
        }

        protected override void OnShow()
        {
            AnimateEntry();
        }

        void AnimateEntry()
        {
            LeanTween.scale(bgImage.rectTransform, Vector3.zero, 0f);
            LeanTween.scaleX(bgImage.gameObject, 1f, Duration)
            .setEase(Ease)
            .setOnComplete(AnimateY);

            void AnimateY()
            {
                LeanTween.scaleY(bgImage.gameObject, 1f, Duration)
                .setEase(Ease);
            }
        }

        protected override void OnHide()
        {
            AnimateExit();
        }

        void AnimateExit()
        {
            LeanTween.scale(bgImage.rectTransform, Vector3.one, 0f);

            LeanTween.scaleX(bgImage.gameObject, 0f, Duration)
            .setEase(Ease)
            .setOnComplete(AnimateY);

            void AnimateY()
            {
                LeanTween.scaleY(bgImage.gameObject, 1f, Duration)
                .setEase(Ease);
            }
        }
    }
}