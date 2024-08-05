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
        public float Change { get; set; }
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

        public AttributeValueChangedContext(float previous, float @new, float change)
        {
            Previous = previous;
            New = @new;
            Change = change;
        }
    }
}