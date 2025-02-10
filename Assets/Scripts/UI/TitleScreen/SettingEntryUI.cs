using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

using UZSG.Data;

namespace UZSG.UI
{
    public class SettingEntryUI : UIElement, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        readonly Color HoveredColor = new(1f, 1f, 1f, 0.025f);
        readonly Color NormalColor = Colors.Transparent;

        [SerializeField] SettingsEntryData settingsEntryData;
        public SettingsEntryData Data => settingsEntryData;
        public SettingEntry Setting { get; set; }
        public bool IsDirty { get; set; }
        public object Value { get; }

        [SerializeField] float spacerWidth;

        public event EventHandler<PointerEventData> OnMouseDown;
        public event Action OnValueChanged;

        [SerializeField] Image background;
        [SerializeField] TextMeshProUGUI labelTmp;
        [SerializeField] RectTransform spacer;

        protected override void Awake()
        {
            base.Awake();
            labelTmp = transform.Find("Label (TMP)").GetComponent<TextMeshProUGUI>();
            OnValueChanged += MarkDirty;
        }

        protected virtual void OnValueChangedEvent()
        {
            OnValueChanged?.Invoke();
        }

        void Start()
        {
            labelTmp.text = settingsEntryData.DisplayNameTranslatable;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            spacer.sizeDelta = new(spacerWidth, spacer.sizeDelta.y);
        }
#endif

        protected override void OnShow()
        {
            labelTmp.text = settingsEntryData.DisplayNameTranslatable;
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

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
            OnMouseDown?.Invoke(this, eventData);
        }
    }
}