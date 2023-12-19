using UnityEngine;

namespace URMG
{
    public abstract class Consumable : MonoBehaviour, IConsumable, IDroppable
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