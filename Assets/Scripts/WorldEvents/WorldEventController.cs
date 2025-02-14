using System.Collections.Generic;

using UnityEngine;


using UZSG.Data;

namespace UZSG.Worlds.Events
{
    public class WorldEventController : MonoBehaviour//, ISaveDataReadWrite<WorldEventSaveData>
    {
        public World World { get; private set; }

        List<WorldEventBase> ongoingEvents = new();

        internal NaturalEnemySpawnEvent naturalEnemySpawnEvent;
        
        void Awake()
        {
            World = GetComponentInParent<World>();
        }

        public void Initialize()
        {
        }

        public void Deinitialize()
        {
            foreach (var e in ongoingEvents)
            {
                /// TODO: some events may require saving upon world exit
                e.EndEvent();
            }
            ongoingEvents.Clear();
            naturalEnemySpawnEvent.EndEvent();
        }
        
        internal void BeginDefaultEvents()
        {
            /// This event is indefinite
            naturalEnemySpawnEvent = new NaturalEnemySpawnEvent();
            // naturalEnemySpawnEvent.StartEvent();
        }
    }
}