using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UZSG.World
{
    [Serializable]
    public class WorldEvent : MonoBehaviour
    {
        [SerializeField] protected WorldEventData data;
        public WorldEventData Data => data;
        bool _isActive;
        public bool Ongoing => _isActive;

        public event EventHandler<string> OnEventStart;
        public event EventHandler<string> OnEventOccur;
        public event EventHandler<string> OnEventEnd;
        
        
        public void StartEvent()
        {
            _isActive = true;
            OnEventStart?.Invoke(this, Data.Name);
        }

        public void OnEverySecond()
        {
            OnEventOccur?.Invoke(this, Data.Name);
        }

        public void EndEvent()
        {
            _isActive = false;
            OnEventEnd?.Invoke(this, Data.Name);
        }
    }
}
