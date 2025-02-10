using System;

using UnityEngine;

using TMPro;

namespace UZSG.UI
{
    public enum RuleValueType {
        Input, Checkbox
    }

    public class RuleEntryUI : UIElement
    {
        public RuleTypeEnum RuleType;
        
        [SerializeField] float spacerWidth;
        [SerializeField] RectTransform spacer;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (spacer != null) spacer.sizeDelta = new(spacerWidth, spacer.sizeDelta.y);
        }
    }
}