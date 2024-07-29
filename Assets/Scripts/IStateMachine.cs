using System;

namespace UZSG.Systems
{
    public interface IStateMachine
    {
        public event Action OnStateChanged;
    }
}
