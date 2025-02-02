using System;

using UnityEngine;
using UnityEngine.EventSystems;

using UZSG.Data;

namespace UZSG.UI
{
    public class SettingEntryUI : UIElement, IPointerDownHandler
    {
        [SerializeField] SettingsEntryData settingsEntryData;
        public SettingsEntryData Data => settingsEntryData;
        
        public event EventHandler<PointerEventData> OnClicked;
        public virtual event EventHandler<PointerEventData> OnValueChanged;
        [SerializeField] float spacerWidth;

        [SerializeField] RectTransform spacer;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            spacer.sizeDelta = new(spacerWidth, spacer.sizeDelta.y);
        }
#endif

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClicked?.Invoke(this, eventData);
        }

        public virtual void Refresh()
        {
        }
    }
}