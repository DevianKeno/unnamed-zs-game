
using UnityEngine;

namespace ZS
{
    public abstract class Consumable : MonoBehaviour, IConsumable, IPickupable, IDroppable
    {
        public void Consume()
        {
            throw new System.NotImplementedException();
        }

        public void Drop()
        {
            throw new System.NotImplementedException();
        }

        public void Pickup()
        {
            throw new System.NotImplementedException();
        }
    }
}