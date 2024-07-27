using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UZSG.UI
{
    public class Choice : MonoBehaviour
    {
        public string Label
        {
            get
            {
                return labelText.text;
            }
            set
            {
                labelText.text = value;
            }
        }
        public bool HasIcon;
        public Sprite Icon
        {
            get
            {
                return iconImage.sprite;
            }
            set
            {
                iconImage.sprite = value;
            }
        }

        bool _hideOnSubmit = true;
        List<Action> _callbacks = new();
        ChoiceWindow parent;

        [SerializeField] TextMeshProUGUI labelText;
        [SerializeField] Image iconImage;
        [SerializeField] Button button;
        
        void OnValidate()
        {
            if (HasIcon)
            {
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        void Start()
        {
            parent = GetComponentInParent<ChoiceWindow>();
            button.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            button.interactable = false;
            Invoke();

            if (_hideOnSubmit)
            {
                parent.Hide();
            }
        }

        public Choice AddCallback(Action callback)
        {
            _callbacks.Add(callback);
            return this;
        }

        public Choice Invoke()
        {
            if (_callbacks.Count == 0) return this;
            
            foreach (var c in _callbacks)
            {
                c.Invoke();
            }
            return this;
        }

        public Choice SetEnabled(bool value)
        {
            button.interactable = value;
            return this;
        }

        public Choice HideOnSubmit(bool value = true)
        {
            _hideOnSubmit = value;
            return this;
        }
    }
}