using UZSG.Attributes;
using UZSG.Interactions;
using UZSG.Saves;
using UZSG.StatusEffects;

namespace UZSG.Entities
{
    /// <summary>
    /// Interface collection for Player Entity.
    /// </summary>
    public interface IPlayer :
        IAttributable,
        IStatusEffectAfflictable,
        IDamageable,
        IDamageSource,
        IInteractActor,
        IMeleeWeaponActor,
        ISaveDataReadWrite<PlayerSaveData>
    {
    }
}