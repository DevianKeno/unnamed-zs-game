using UnityEngine;
using UnityEngine.UI;

namespace UZSG.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class Frame : MonoBehaviour
    {
        public string Name = "Frame";
        [SerializeField] RectTransform rect;
        public RectTransform Rect => rect;

        public Frame(string name)
        {
            Name = name;
        }

        void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        void Start()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
        }
    }
}

