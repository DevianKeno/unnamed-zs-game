using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UZSG.UI
{
    public class HealthRingUI : MonoBehaviour
    {
        public float MaxValue;
        public float CurrentValue;
        [Range(0, 1)]
        [SerializeField] float progress;
        public float Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                Refresh();
            }
        }
        [SerializeField] Image fill;
        [SerializeField] TextMeshProUGUI valueText;

        void OnValidate()
        {
            Refresh();
        }
    
        public void Refresh()
        {
            fill.fillAmount = progress;
            valueText.text = $"{Mathf.FloorToInt(MaxValue * progress)}";
        }
    }
}