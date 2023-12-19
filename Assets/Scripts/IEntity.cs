
namespace URMG
{
    public interface ISpawnable
    {
        public void Spawn();
    }
    
    /// <summary>
    /// Entities are damageable and killable.
    /// </summary>
    public interface IEntity : ISpawnable, IDamageable, IKillable
    {

    }
}