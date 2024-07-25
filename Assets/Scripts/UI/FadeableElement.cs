using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UZSG.Systems;

namespace UZSG.UI
{
    [RequireComponent(typeof(Graphic))]
    public class FadeableElement : MonoBehaviour
    {
        Graphic graphic;
        float originalAlpha;

        void Awake()
        {
            graphic = GetComponent<Graphic>();
        }

        internal void Initialize()
        {
            graphic = GetComponent<Graphic>();
            originalAlpha = graphic.color.a;
            SetAlpha(0f);
        }

        public void SetAlpha(float alpha)
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        public void FadeIn(float duration)
        {
            StopAllCoroutines();

            if (Game.UI.EnableScreenAnimations)
            {
                StartCoroutine(FadeInCoroutine(duration));
            }
            else
            {
                SetAlpha(originalAlpha);
            }
        }

        IEnumerator FadeInCoroutine(float duration)
        {
            float elapsedTime = 0f;
            SetAlpha(0f);

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / duration) * originalAlpha;
                SetAlpha(alpha);
                yield return null;
            }
            SetAlpha(originalAlpha);
        }
    }
}