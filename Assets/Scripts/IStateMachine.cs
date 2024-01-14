using System;
using UZSG.Systems;

namespace UZSG
{
    public interface IStateMachine<T> where T : Enum
    {
        public StateMachine<T> sm { get; }
    }
}
