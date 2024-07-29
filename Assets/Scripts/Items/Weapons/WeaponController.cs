using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Systems;

namespace UZSG.Items.Weapons
{
    /// <summary>
    /// Controls the item held by something.
    /// </summary>
    public abstract class HeldItemController : MonoBehaviour
    {
        public ItemData ItemData;
        protected Entity owner;
        public Entity Owner
        {
            get { return owner; }
            set { owner = value; }
        }
        protected InputActionMap actionMap;

        public abstract void Initialize();
        public abstract void SetStateFromAction(ActionStates state);
    }

    public abstract class WeaponController : HeldItemController
    {
        
    }
}
