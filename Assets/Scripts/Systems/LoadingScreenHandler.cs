using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UZSG.SceneHandlers
{
    public class LoadingScreenHandler : MonoBehaviour
    {
        public static LoadingScreenHandler Instance { get; private set; }

        public string Message
        {
            get { return messageTmp.text; }
            set { messageTmp.text = value; }
        }
        public string Tip
        {
            get { return tipTmp.text; }
            set { tipTmp.text = value; }
        }
        [SerializeField, Range(0, 1)] float progress;
        public float Progress
        {
            get => progress;
            set
            {
                progress = Mathf.Clamp(value, 0f, 1f);
                logoLoadingFill.fillAmount = progress;
            }
        }

        [SerializeField] TextMeshProUGUI messageTmp;
        [SerializeField] TextMeshProUGUI tipTmp;
        [SerializeField] Image logoLoadingFill;
        [SerializeField] RectTransform progressRect;

        void Awake()
        {
            DontDestroyOnLoad(this);

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                Progress = progress;
            }
        }
    }
}