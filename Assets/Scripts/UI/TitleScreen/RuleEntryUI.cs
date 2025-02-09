using System;

using UnityEngine;

using TMPro;

namespace UZSG.UI
{
    public enum RuleTypeEnum {
        Seed, DaytimeLength, NighttimeLength, MaxPlayers, LootRespawns, DropItemsOnDeath,
    }

    public enum RuleValueType {
        Input, Checkbox
    }

    public class RuleEntryUI : UIElement
    {
        public string Id;
        public RuleTypeEnum Rule;
        public RuleValueType ValueType;
        [SerializeField] float spacerWidth;
        public event Action<RuleEntryUI, object> OnValueChanged;
        [SerializeField] RectTransform spacer;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (spacer != null) spacer.sizeDelta = new(spacerWidth, spacer.sizeDelta.y);
        }

        protected virtual void Start()
        {
            if (ValueType == RuleValueType.Input)
            {
                var c = GetComponentInChildren<TMP_InputField>();
                c?.onEndEdit.AddListener((text) =>
                {
                    OnValueChanged?.Invoke(this, text);
                });
            }
            else if (ValueType == RuleValueType.Checkbox)
            {

            }
        }
    }
}