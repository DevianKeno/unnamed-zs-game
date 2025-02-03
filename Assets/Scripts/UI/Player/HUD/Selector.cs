using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using UZSG.Systems;

namespace UZSG.UI
{
    public class Selector : UIElement
    {
        public bool EnableAnimations { get; set; } = true;
        public LeanTweenType TweenType;

        RectTransform _target;
        List<Image> images;
        
        protected override void Awake()
        {
            rect = GetComponent<RectTransform>();
            images = new(GetComponentsInChildren<Image>());
        }

        void Update()
        {
            if (IsVisible)
            {
                rect.position = _target.position;
                rect.sizeDelta = _target.sizeDelta;
            }
        }

        public void Select(RectTransform target, bool show = true)
        {
            if (target == null) return;
            
            this._target = target;
            // SetParent(target.transform);
            if (EnableAnimations && Game.UI.EnableScreenAnimations)
            {
                LeanTween.cancel(gameObject);
                LeanTween.value(gameObject, rect.position, target.position, Game.UI.GlobalAnimationFactor / 5f)
                .setOnUpdate((Vector3 i) =>
                {
                    rect.position = i;
                })
                .setEase(TweenType);
                LeanTween.size(rect, target.sizeDelta, Game.UI.GlobalAnimationFactor / 5f)
                .setEase(TweenType);
            }
            else
            {
                rect.position = target.position;
                rect.sizeDelta = target.sizeDelta;
            }

            if (show) Show();
        }

        protected override void OnShow()
        {
            // foreach (Image i in images)
            // {
            //     i.enabled = true;
            // }
        }

        protected override void OnHide()
        {
            // foreach (Image i in images)
            // {
            //     i.enabled = false;
            // }
        }
    }
}
