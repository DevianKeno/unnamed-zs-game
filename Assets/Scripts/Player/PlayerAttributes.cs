using UnityEngine;

namespace UZSG.Player
{
    /// <summary>
    /// Represents the default attributes a player has.
    /// </summary>
    public class PlayerAttributes : MonoBehaviour
    {
        [SerializeField] PlayerVitalAttributes _vitals;
        public PlayerVitalAttributes Vitals => _vitals;
        
        [SerializeField] PlayerGenericAttributes _generic;
        public PlayerGenericAttributes Generics => _generic;

        void Start()
        {
            _vitals = new();
            _generic = new();
        }
        
        internal void Initialize()
        {
            
        }
    }
}