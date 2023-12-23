using UnityEngine;

namespace UZSG.UI
{
    public class Frame : MonoBehaviour
    {
        public string Name = "Frame";
        public Vector2 size = new (1920f, 1080f); // Default size
        public RectTransform Rect;
        
        void Awake()
        {
            Rect = GetComponent<RectTransform>();
        }

        void Start()
        {
            Rect.sizeDelta = size;
        }
    }
}

