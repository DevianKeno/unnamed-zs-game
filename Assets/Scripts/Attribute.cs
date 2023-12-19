using System;
using UnityEngine;

namespace URMG
{
    /// <summary>
    /// An attribute represents a value without any particular bounds.
    /// </summary>
    [Serializable]
    public class Attribute
    {
        public class ValueChangedArgs : EventArgs
        {
            public float Prev;
            public float Change;
            public float New;
        }

        protected float _prev;
        [SerializeField] protected float _value;
        public float Value { get => _value; }

        /// <summary>
        /// Fired everytime ONLY IF this attribute's value is changed.
        /// </summary>
        public event EventHandler<ValueChangedArgs> OnValueChanged;

        public Attribute(float value)
        {
            _value = value;
        }

        protected virtual void OnValueChange()
        {
            OnValueChanged?.Invoke(this, new ()
            {
                Prev = _prev,
                Change = Mathf.Abs(_prev -= _value),
                New = _value
            });
        }

        public virtual void Add(float value)
        {
            _prev = _value;
            _value += value;
            if (_prev != _value) OnValueChange();
        }
        

        public virtual void Remove(float value)
        {
            _prev = _value;
            _value -= value;
            if (_prev != _value) OnValueChange();
        }
    }
}