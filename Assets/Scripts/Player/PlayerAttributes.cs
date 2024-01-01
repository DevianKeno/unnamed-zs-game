using UnityEngine;

namespace UZSG.Player
{
    /// <summary>
    /// Represents the default attributes a player has.
    /// </summary>
    public class PlayerAttributes
    {
        [SerializeField] PlayerVitalAttributes _vital;
        public PlayerVitalAttributes Vital => _vital;
        
        [SerializeField] PlayerGenericAttributes _generic;
        public PlayerGenericAttributes Generic => _generic;
        
        internal void Initialize()
        {
            _vital.Initialize();
            _generic.Initialize();
        }

        public void ReadData()
        {
            
        }
    }
}