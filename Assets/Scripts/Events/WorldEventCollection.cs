using System;
using UZSG.Events;
using UZSG.Entities;

namespace UZSG.Worlds
{
    public class WorldEventCollection : EventCollection
    {
        public event Action OnPlayerJoined;
        public event Action<Player> OnPlayerLeft;
    }
}