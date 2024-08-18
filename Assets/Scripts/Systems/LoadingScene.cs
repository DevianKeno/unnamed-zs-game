using System;
using UnityEngine;

namespace UZSG.Scenes
{
    public class LoadingScene : MonoBehaviour
    {
        [Range(0, 1)]
        [SerializeField] float progress;
        public float Progress
        {
            get => progress;
            set
            {
                progress = Mathf.Clamp(value, 0, 1);
                UpdateProgress();
            }
        }

        [SerializeField] RectTransform logo;
        [SerializeField] RectTransform progressRect;

        void OnValidate()
        {

        }

        void Update()
        {

        }

        void UpdateProgress()
        {
            var x = Mathf.Lerp(logo.rect.width, 0f, Progress);
            progressRect.offsetMax = new(-x, progressRect.offsetMax.y);
        }
    }
}