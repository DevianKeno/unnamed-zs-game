using UnityEngine;

namespace UZSG
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