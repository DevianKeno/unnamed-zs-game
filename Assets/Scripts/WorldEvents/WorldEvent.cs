using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using UZSG.Data;

namespace UZSG.WorldEvents
{
    public class WorldEvent
    {
        WorldEventProperties _eventInfo;
        public WorldEventProperties EventInfo 
        {
            get => _eventInfo;
            set => _eventInfo = value;
        }

        List<EventPrefab> _selectedEvent;
        public List<EventPrefab> SelectedEvent
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