using System;
using TMPro;
using UnityEngine;

namespace UZSG.Scenes
{
    public class LoadingScreenHandler : MonoBehaviour
    {
        public string Message
        {
            get
            {
                return messageTmp.text;
            }
            set
            {
                messageTmp.text = value;
            }
        }
        
        public string Tip
        {
            get
            {
                return tipTmp.text;
            }
            set
            {
                tipTmp.text = value;
            }
        }
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

        [SerializeField] TextMeshProUGUI messageTmp;
        [SerializeField] TextMeshProUGUI tipTmp;
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