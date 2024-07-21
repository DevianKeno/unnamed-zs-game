using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UZSG.UI
{
    public class Window : MonoBehaviour, IUIElement
    {
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

        public event Action OnOpen;
        public event Action OnClose;

        RectTransform rect;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }

        public void Show()
        {
            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            OnShow();
            IsVisible = true;
            OnOpen?.Invoke();
        }

        public void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
            IsVisible = false;
            OnClose?.Invoke();
        }

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
                Show();
            } else
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
            rect.localScale = new Vector2(width, height);
        }
        
        public void Move(Vector3 position)
        {
            rect.position = position;
        }
    }
}
