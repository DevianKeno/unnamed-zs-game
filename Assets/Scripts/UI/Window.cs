using System;

using UnityEngine;
using UnityEngine.UI;

using UZSG.Systems;

namespace UZSG.UI
{
    /// <summary>
    /// Windows are UI elements that can be shown or hidden.
    /// </summary>
    public class Window : MonoBehaviour, IUIElement
    {
        [SerializeField] protected RectTransform rect;
        public RectTransform Rect => rect;
        [field: Space]

        public bool IsVisible { get; set; }
        public Vector3 Position
        {
            get
            {
                return rect.transform.position;
            }
            set
            {
                rect.transform.position = value;
            }
        }
        public float AnimationFactor = 0.3f;
        /// <summary>
        /// Removes all other windows when this is displayed.
        /// </summary>
        public bool AlwaysSolo { get; set; }

        protected GameObject blocker;

        public event Action OnOpen;
        public event Action OnClose;

        void Start()
        {
            rect = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Executes once ONLY IF the window is previously hidden, then made visible.
        /// </summary>
        public virtual void OnShow() { }
        /// <summary>
        /// Executes once ONLY IF the window is previously visible, then made hidden.
        /// </summary>
        public virtual void OnHide() { }

        /// <summary>
        /// Shows the window.
        /// </summary>
        /// <param name="solo">Removes all other windows if true.</param>
        public void Show(bool solo = false)
        {
            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

            if (solo)
            {
                Game.UI.SetCurrentWindow(this);
            }
            
            if (!IsVisible)
            {
                OnShow();
                OnOpen?.Invoke();
            }
            IsVisible = true;
            Game.UI.AddToActiveWindows(this);
        }

        bool _destroyOnBlockerClick;

        public void CreateBlocker(bool destroyOnClick = true)
        {
            _destroyOnBlockerClick = destroyOnClick;
            blocker = Game.UI.CreateBlocker(forElement: this, onClick: () =>
            {
                if (_destroyOnBlockerClick) Destroy();
            });
        }

        /// <summary>
        /// Hides the window. Does not destroy. Use Destroy() if you need to,
        /// </summary>
        public void Hide()
        {
            if (IsVisible)
            {
                OnHide();
                OnClose?.Invoke();
            };
            IsVisible = false;
            
            Game.UI.RemoveFromActiveWindows(this);
            gameObject.SetActive(false);
        }

        // public void FadeIn()
        // {

        // }

        // public void FadeOut()
        // {

        // }

        public void Destroy(float delay = 0f)
        {
            Hide();
            Destroy(gameObject, delay);
        }

        public void ToggleVisibility()
        {
            SetVisible(!IsVisible);
        }

        public void SetVisible(bool visible)
        {
            if (visible)
            {
                Show(AlwaysSolo);
            }
            else
            {
                Hide();
            }
            IsVisible = visible;
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
    }
}
