using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace UZSG.World
{
    public class WorldEvent
    {
        WorldEventProperties _eventInfo;
        public WorldEventProperties EventInfo 
        {
            get => _eventInfo;
            set => _eventInfo = value;
        }

        EventPrefab _selectedEvent;
        public EventPrefab SelectedEvent
        {
            get => _selectedEvent;
            set => _selectedEvent = value;
        }

        public event EventHandler<WorldEventProperties> OnSpawnEvent;
        public event EventHandler<WorldEventProperties> OnEndEvent;

        void SpawnEvent()
        {
            OnSpawnEvent?.Invoke(this, _eventInfo);
        }
        void EndEvent()
        {
            OnEndEvent?.Invoke(this, _eventInfo);
        }

        public void Initialize()
        {
            SpawnEvent();
        }
    }
}