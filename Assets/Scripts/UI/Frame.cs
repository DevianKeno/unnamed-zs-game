using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UZSG.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class Frame : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("Id")] string id = "frame";
        public string Id => id;
        /// <summary>
        /// The name of the frame.
        /// </summary>
        [SerializeField, FormerlySerializedAs("Name")] string displayName = "Frame";
        public string DisplayName => displayName;
        
        [SerializeField] RectTransform rect;
        public RectTransform Rect => rect;

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

