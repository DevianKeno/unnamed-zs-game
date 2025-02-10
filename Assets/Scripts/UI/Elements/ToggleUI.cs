using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace UZSG.UI
{
    public class ToggleUI : UIElement
    {
        [SerializeField] bool isOn = true;
        public bool Value => isOn;
        public float SpeedInSeconds = 0.5f;
        public Color BackgroundColorOff = new(0.37f, 0.37f, 0.37f, 1f);
        public Color BackgroundColorOn = new(0.2f, 0.33f, 0.46f, 1f);
        public Color KnobColor = new(1f, 1f, 1f, 1f);

        [SerializeField] Toggle toggle;
        [SerializeField] RectTransform knobRect;
        [SerializeField] Image backgroundImage;

        protected override void Awake()
        {
            base.Awake();
            SetValue(isOn);
            toggle.onValueChanged.AddListener(SetValue);
        }

        public bool GetValue()
        {
            return isOn;
        }

        public void SetValue(bool isOn)
        {
            this.isOn = isOn;
            toggle.interactable = false;

            SetInteractableAfter(true, SpeedInSeconds);

            if (this.isOn)
            {
                LeanTween.value(knobRect.gameObject, 0f, 1f, SpeedInSeconds)
                .setOnUpdate((float i) =>
                {
                    knobRect.pivot = new Vector2(i, knobRect.pivot.y);
                })
                .setEaseOutExpo();

                LeanTween.value(backgroundImage.gameObject, 0.1f, 1f, SpeedInSeconds)
                .setEaseOutExpo()
                .setOnUpdate((float i) =>
                {
                    backgroundImage.color = Color.Lerp(BackgroundColorOff, BackgroundColorOn, i);
                });
            }
            else
            {
                LeanTween.value(knobRect.gameObject, 1f, 0f, SpeedInSeconds)
                .setOnUpdate((float i) =>
                {
                    knobRect.pivot = new Vector2(i, knobRect.pivot.y);
                })
                .setEaseOutExpo();

                LeanTween.value(backgroundImage.gameObject, 0.1f, 1f, SpeedInSeconds)
                .setEaseOutExpo()
                .setOnUpdate((float i) =>
                {
                    backgroundImage.color = Color.Lerp(BackgroundColorOn, BackgroundColorOff, i);
                });
            }
        }

        async void SetInteractableAfter(bool value, float delayInSeconds)
        {
            await Task.Delay((int)(delayInSeconds * 1000));
            toggle.interactable = value;
        }
    }
}
