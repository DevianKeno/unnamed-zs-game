using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UZSG.Data;

namespace UZSG.UI
{
    public class SettingEntryUI : UIElement, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        readonly Color HoveredColor = new(1f, 1f, 1f, 0.025f);
        readonly Color NormalColor = Colors.Transparent;

        internal bool _isDirty;

        [SerializeField] SettingsEntryData settingsEntryData;
        public SettingsEntryData Data => settingsEntryData;
        public SettingEntry Setting;
        public virtual object Value { get; }
        public event EventHandler<PointerEventData> OnClicked;
        public virtual event Action<SettingEntryUI> OnValueChanged;
        [SerializeField] float spacerWidth;

        [SerializeField] Image background;
        [SerializeField] RectTransform spacer;

        protected override void Awake()
        {
            base.Awake();
            OnValueChanged += MarkDirty;
        }

        protected void MarkDirty(SettingEntryUI sender)
        {
            _isDirty = true;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            spacer.sizeDelta = new(spacerWidth, spacer.sizeDelta.y);
        }
#endif

        public void OnPointerEnter(PointerEventData eventData)
        {
            background.color = HoveredColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            background.color = NormalColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClicked?.Invoke(this, eventData);
        }

        public virtual void Refresh()
        {
        }

    }
}