using UnityEngine;
using UnityEngine.UI;
using UZSG.Systems;

namespace UZSG.UI
{
    [RequireComponent(typeof(Graphic))]
    public class FadeableElement : MonoBehaviour
    {
        float _originalAlpha;
        Graphic graphic;

        void Awake()
        {
            graphic = GetComponent<Graphic>();
        }

        void Start()
        {
            _originalAlpha = graphic.color.a;
        }

        public void SetAlpha(float alpha)
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        public void FadeIn(float duration)
        {
            if (Game.UI.EnableScreenAnimations)
            {
                SetAlpha(0f);
                graphic.CrossFadeAlpha(_originalAlpha, duration, false);
            }
            else
            {
                SetAlpha(_originalAlpha);
            }
        }

        public void FadeOut(float duration)
        {
            if (Game.UI.EnableScreenAnimations)
            {
                graphic.CrossFadeAlpha(0f, duration, false);
            }
            else
            {
                SetAlpha(0f);
            }
        }
    }
}
