using UnityEngine;

using UZSG.Attributes;

namespace UZSG.UI
{
    public class StaminaBar : MonoBehaviour
    {
        public Attribute Attribute;

        float _fullSizeDelta;
        float _emptySizeDelta;

        public float BarValue;
        public float BufferValue;

        public RectTransform Container;
        public RectTransform Bar;
        public RectTransform Buffer;

        void Start()
        {
            _emptySizeDelta = Container.sizeDelta.x;
            _fullSizeDelta = 0f;
        }

        void ValueChangedCallback(object sender, Attribute.ValueChangedInfo e)
        {
            Attribute attr = (Attribute) sender;
            BarValue = attr.Value;
            var barOffset = Mathf.InverseLerp(attr.Minimum, attr.Maximum, attr.Value);

            Bar.offsetMax = new(
                -Mathf.Lerp(_emptySizeDelta, _fullSizeDelta, barOffset),
                Bar.offsetMax.y
            );

            // if (BarValue != BufferValue)
            // {
            //     LeanTween.cancel(Buffer.gameObject);
                
            //     LeanTween.value(Buffer.gameObject, BufferValue, BarValue, 0.5f)
            //     .setOnUpdate((i) =>
            //     {

            //         Buffer.offsetMax = new(
            //             -Mathf.Lerp(_emptySizeDelta, _fullSizeDelta, bufferOffset),
            //             Buffer.offsetMax.y
            //         );
            //     });
            //     BufferValue = BarValue;
            // }

            // if (BarValue != BufferValue)
            // {
            //     LeanTween.cancel(Buffer.gameObject);
                
            //     LeanTween.value(Buffer.gameObject, BufferValue, BarValue, 0.5f)
            //     .setOnUpdate((i) =>
            //     {
            //         var bufferOffset = Mathf.InverseLerp(Attribute.Minimum, attr.Maximum, i);

            //         Buffer.offsetMax = new(
            //             -Mathf.Lerp(_emptySizeDelta, _fullSizeDelta, bufferOffset),
            //             Buffer.offsetMax.y
            //         );
            //     });
            // }
        }

        void Update()
        {
            // if (BarValue < BufferValue)
            // {
            //     var barCurrentX = Bar.offsetMax.x;
            //     var bufferCurrentX = Buffer.offsetMax.x;

            //     if (lerpTimer < Game.Tick.SecondsPerTick)
            //     {
            //         lerpTimer += Time.time;

            //         Buffer.offsetMax = new(
            //             -Mathf.Lerp(barCurrentX, bufferCurrentX, lerpTimer / Game.Tick.SecondsPerTick),
            //             Buffer.offsetMax.y 
            //         );

            //     } else
            //     {
            //         lerpTimer = 0f;
            //         BufferValue = BarValue;
            //     }
            // }
        }

        /// TESTING ONLY
        public void SetAttribute(Attribute attribute)
        {
            Attribute = attribute;
            BarValue = BufferValue = attribute.Value;
            attribute.OnValueChanged += ValueChangedCallback;
        }
    }
}
