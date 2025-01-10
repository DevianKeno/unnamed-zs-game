using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Items;
using System;

namespace UZSG.UI
{
    public class RadialProgressUI : UIElement
    {
        public float TotalTime;
        public float TimeElapsed;

        [Range(0, 1)]
        [SerializeField] protected float progress;
        public float Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = Mathf.Clamp(value, 0, 1);
                Refresh();
            }
        }

        [SerializeField] protected Image fillInner;
        [SerializeField] protected Image fillOuter;
        [SerializeField] protected TextMeshProUGUI timeText;

        protected override void OnValidate()
        {
            base.OnValidate();
            Refresh();
        }

        protected virtual void Refresh()
        {
            UpdateProgressUI();
            UpdateTime();
        }

        void UpdateProgressUI()
        {
            TimeElapsed = progress * TotalTime;
            
            fillInner.fillAmount = 1 - progress;
            fillOuter.fillAmount = progress;
        }

        void UpdateTime()
        {
            var timeDisplayed = TotalTime - TimeElapsed;
            timeText.text = FormatTime(timeDisplayed);
        }
        
        protected string FormatTime(float time)
        {
            if (time >= 3600) /// 1 hour or more
                return $"{(int)(time / 3600)}H";

            if (time >= 600) /// Two-digit minutes (10 minutes or more)
                return $"{(int)(time / 60)}m";

            if (time >= 60) /// One-digit minutes (1 minute to 9 minutes 59 seconds)
                return $"{(int)(time / 60)}:{(int)(time % 60):D2}";

            if (time >= 1) /// Seconds (1 to 59 seconds)
                return $"{(int)time}";

            /// Milliseconds (less than 1 second)
            return $"{time:F1}";
        }
    }
}
