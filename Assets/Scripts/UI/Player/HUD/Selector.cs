using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UZSG.Systems;

namespace UZSG.UI
{
    public class Selector : Window
    {
        public float AnimationFactor = 0.1f;
        public LeanTweenType TweenType;

        List<Image> images;
        
        void Awake()
        {
            rect = GetComponent<RectTransform>();
            images = new(GetComponentsInChildren<Image>());
        }

        public void Select(RectTransform target)
        {
            if (target == null) return;
            
            SetParent(target.transform);
            if (Game.UI.EnableScreenAnimations)
            {
                LeanTween.cancel(gameObject);
                LeanTween.value(gameObject, rect.position, target.position, AnimationFactor)
                .setOnUpdate((Vector3 i) =>
                {
                    rect.position = i;
                })
                .setEase(TweenType);
                LeanTween.size(rect, target.sizeDelta, AnimationFactor)
                .setEase(TweenType);
            }
            else
            {
                rect.position = target.position;
                rect.sizeDelta = target.sizeDelta;
            }

            foreach (Image i in images)
            {
                i.enabled = true;
            }
        }

        public override void OnShow()
        {
            foreach (Image i in images)
            {
                i.enabled = true;
            }
        }

        public override void OnHide()
        {
            foreach (Image i in images)
            {
                i.enabled = false;
            }
        }
    }
}
