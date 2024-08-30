using UnityEngine;
using UnityEngine.UI;
using UZSG.Systems;

namespace UZSG.UI
{
    public class Selector : Window
    {
        public float AnimationFactor = 0.1f;
        public LeanTweenType TweenType;
        
        [SerializeField] Image image;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            image = GetComponent<Image>();
        }

        public void Select(RectTransform target)
        {            
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
            image.enabled = true;
        }

        public override void OnShow()
        {
            image.enabled = true;
        }

        public override void OnHide()
        {
            image.enabled = false;
        }
    }
}
