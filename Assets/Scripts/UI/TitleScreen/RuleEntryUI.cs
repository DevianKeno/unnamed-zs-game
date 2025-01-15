using System;
using TMPro;
using UnityEngine;

namespace UZSG.UI
{
    public enum RuleTypeEnum {
        DaytimeLength, NighttimeLength, MaxPlayers, LootRespawns, DropItemsOnDeath,
    }

    public enum RuleValueType {
        Input, Checkbox
    }

    public class RuleEntryUI : UIElement
    {
        public string Id;
        public RuleTypeEnum Rule;
        public RuleValueType ValueType;
        public event Action<RuleEntryUI, object> OnValueChanged;
    
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