using UnityEngine;
using UZSG.Entities;
using UZSG.Players;

namespace UZSG.Items.Weapons
{
    /// <summary>
    /// Controls the item held by something.
    /// </summary>
    public abstract class HeldItemController : MonoBehaviour
    {
        public ItemData ItemData;
        Entity owner;
        public Entity Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public abstract void Initialize();
        public abstract void SetStateFromAction(ActionStates state);
    }

    public abstract class WeaponController : HeldItemController
    {
        
    }
}
