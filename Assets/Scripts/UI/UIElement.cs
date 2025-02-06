using UnityEngine;
using UnityEngine.UI;

namespace UZSG.UI
{
    /// <summary>
    /// UI elements that can be shown or hidden.
    /// </summary>
    public class UIElement : MonoBehaviour
    {
        [SerializeField] protected RectTransform rect;
        public RectTransform Rect => rect;
        [field: Space]
        
        public bool IsVisible => gameObject.activeSelf;
        public Vector2 Position
        {
            get { return rect.anchoredPosition; }
            set { rect.anchoredPosition = value; }
        }
        public Vector2 Pivot
        {
            get { return rect.pivot; }
            set { rect.pivot = value; }
        }        
       
        protected virtual void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        protected virtual void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                rect ??= GetComponent<RectTransform>();
            }
        }
        
        /// <summary>
        /// Executes once ONLY IF the window is previously hidden, then made visible.
        /// </summary>
        protected virtual void OnShow() { }
        /// <summary>
        /// Executes once ONLY IF the window is previously visible, then made hidden.
        /// </summary>
        protected virtual void OnHide() { }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        /// <summary>
        /// Hides the window.
        /// Does not destroy nor disables the object.
        /// Use Destroy() if you need to delete, and SetActive() to disable.
        /// </summary>
        public virtual void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        public void SetActive(bool enabled)
        {
            if (enabled)
            {
                gameObject.SetActive(true);
                Show();
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            }
            else
            {
                Hide();
                // Game.UI.RemoveFromActiveWindows(this);
                gameObject.SetActive(false);
            }
        }

        public void SetParent(Transform p)
        {
            transform.SetParent(p);
        }

        // public void FadeIn()
        // {

        // }

        // public void FadeOut()
        // {

        // }

        /// <summary>
        /// <i>Destroys</i> this object in accordance with UZSG laws.
        /// <c>Destruct</c> because <c>Destroy</c> is reserved for UnityEngine's method.
        /// </summary>
        public void Destruct(float delay = 0f, bool invokeOnHideEvent = true)
        {
            if (invokeOnHideEvent) Hide();
            Game.UI.DestroyElement(gameObject, delay);
        }

        public void SetScale(float multiplier)
        {
            Vector2 dimensions = rect.localScale;
            dimensions.x *= multiplier; 
            dimensions.y *= multiplier; 
            rect.localScale = dimensions;
        }

        public void SetScale(float width, float height)
        {
            rect.localScale = new(width, height);
        }
        
        public void Move(Vector3 position)
        {
            rect.position = position;
        }

        public void Rebuild()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }
}
