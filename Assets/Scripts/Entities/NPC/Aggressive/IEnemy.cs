using System;

namespace UZSG.Entities
{
    public interface IEnemy
    {
        public event Action<IEnemy> OnDeath;    
    }
}