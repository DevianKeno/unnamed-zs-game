using System;
using UZSG.Systems;

namespace UZSG
{
    public interface IStateMachine<E> where E : Enum
    {
        public StateMachine<E> StateMachine { get; }
    }
}
