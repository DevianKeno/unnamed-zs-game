using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;

namespace UZSG.Items
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
        [SerializeField] protected AudioSourceController audioSourceController;
        public AudioSourceController AudioSource => audioSourceController;
        protected InputActionMap actionMap;

        public abstract void Initialize();
        public abstract void SetStateFromAction(ActionStates state);
    }
}
