using System;

namespace UZSG.Attributes
{
    [Serializable]
    public struct AttributeValueChangedContext
    {
        /// <summary>
        /// The value before the change.
        /// </summary>
        public float Previous { get; set; }
        /// <summary>
        /// The value after the change.
        /// </summary>
        public float New { get; set; }
        /// <summary>
        /// The change in value.
        /// </summary>
        public readonly float Change
        {
            get
            {
                return UnityEngine.Mathf.Abs(New - Previous);
            }
        }
        /// <summary>
        /// Whether if the value had increased or decreased.
        /// </summary>
        public readonly Attribute.ValueChangedType ValueChangedType
        {
            get
            {
                return New > Previous ? Attribute.ValueChangedType.Increased : Attribute.ValueChangedType.Decreased;
            }
        }
    }
}