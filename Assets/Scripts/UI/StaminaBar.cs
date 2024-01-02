using UnityEngine;
using UZSG.Attributes;

namespace UZSG.UI
{
    public class StaminaBar : MonoBehaviour
    {
        public Attribute Attribute;
        public RectTransform Container;
        public RectTransform Bar;
        public RectTransform Buffer;

        float _fullSizeDelta;
        float _emptySizeDelta;

        public float BarValue;
        public float BufferValue;

        void Start()
        {
            _emptySizeDelta = Container.sizeDelta.x;
            _fullSizeDelta = 0f;
        }

        void ValueChangedCallback(object sender, Attribute.ValueChangedArgs e)
        {
            Attribute attr = (Attribute) sender;
            BarValue = attr.Value;
            var barOffset = Mathf.InverseLerp(attr.Minimum, attr.CurrentMaximum, attr.Value);

            Bar.offsetMax = new(
                -Mathf.Lerp(_emptySizeDelta, _fullSizeDelta, barOffset),
                Bar.offsetMax.y
            );

            if (BarValue != BufferValue)
            {
                LeanTween.cancel(Buffer.gameObject);
                
                LeanTween.value(Buffer.gameObject, BufferValue, BarValue, 0.5f)
                .setOnUpdate((i) =>
                {
                    var bufferOffset = Mathf.InverseLerp(Attribute.Minimum, attr.CurrentMaximum, i);

                    Buffer.offsetMax = new(
                        -Mathf.Lerp(_emptySizeDelta, _fullSizeDelta, bufferOffset),
                        Buffer.offsetMax.y
                    );
                });
                BufferValue = BarValue;
            }
        }

        public void SetAttribute(Attribute attribute)
        {
            Attribute = attribute;
            BarValue = BufferValue = attribute.Value;
            attribute.OnValueChanged += ValueChangedCallback;
        }
    }
}
