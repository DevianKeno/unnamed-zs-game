using UZSG.Attributes;
using UZSG.Interactions;
using UZSG.Saves;

namespace UZSG.Entities
{
    /// <summary>
    /// Interface collection for Player Entity.
    /// </summary>
    public interface IPlayerBeingDamage 
    {
        public void DamagePlayer(float damageDealt);
    }
}