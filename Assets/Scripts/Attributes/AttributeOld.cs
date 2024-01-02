using System;
using UnityEngine;

namespace UZSG.Attributes
{
    /// <summary>
    /// Represents a value without any particular bounds.
    /// </summary>
    [Serializable]
    public class AttributeOld
    {
        public struct ValueChangedArgs
        {
            public float Previous;
            public float Change;
            public float New;
        }

        AttributeData _data;
        public AttributeData Data => _data;

        protected float _prev;
        [SerializeField] protected float _value;
        public float Value => _value; 

        /// <summary>
        /// Fired everytime ONLY IF the value of this attribute is changed.
        /// </summary>
        public event EventHandler<ValueChangedArgs> OnValueChanged;

        public AttributeOld(AttributeData data)
        {
            _data = data;
        }

        protected virtual void ValueChanged()
        {
            OnValueChanged?.Invoke(this, new()
            {
                Previous = _prev,
                Change = Mathf.Abs(_prev -= _value),
                New = _value
            });
        }

        /// <summary>
        /// Add amount to the attribute's value.
        /// </summary>
        public virtual void Add(float value)
        {
            _prev = _value;
            _value += value;
            if (_prev != _value) ValueChanged();
        }
        
        /// <summary>
        /// Remove amount from the attribute's value.
        /// </summary>
        public virtual void Remove(float value)
        {
            _prev = _value;
            _value -= value;
            if (_prev != _value) ValueChanged();
        }

        public virtual void SetData()
        {
            
        }
    }
}