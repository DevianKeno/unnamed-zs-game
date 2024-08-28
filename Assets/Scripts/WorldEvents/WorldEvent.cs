using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.WorldEvents
{
    public class WorldEvent
    {
        WorldEventData _eventData;
        public WorldEventData EventData { set => _eventData = value; }
        
        public event Action<WorldEvent> OnSpawnEvent;
        public event Action<WorldEvent> OnEndEvent;

        List<object> _selectedEvents = new();
        public List<object> SelectedEvents => _selectedEvents;

        public void SpawnEvent()
        {
            OnSpawnEvent?.Invoke(this);
        }
        public object PrepareEvent()
        {
            switch (_eventData.Type)
            {
                case WorldEventType.Weather:
                    SelectWeatherToOccur(_eventData);
                    break;
                case WorldEventType.Raid:
                    SelectRaidToOccur(_eventData);
                    break;
                default:
                    return null;
            };

            return _selectedEvents;
        }

        WeatherEventInstance? SelectWeatherToOccur(WorldEventData eventData)
        {
            List<WeatherEventInstance> selectedEvents = new();
            
            int chance = UnityEngine.Random.Range(1, 100);
            foreach (WeatherEventInstance weatherInstance in eventData.WeatherTypes)
            {
                if (weatherInstance.ChanceToOccur >= chance) selectedEvents.Add(weatherInstance);
            }
            if (selectedEvents.Count == 0)
            {
                Game.Console.Log($"<color=#e8eb34>No weather event selected.</color>");
                return null;
            }
            else if (selectedEvents.Count > 1 && !eventData.AllowMultipleEvents)
                selectedEvents = KeepOnlyAtIndex(selectedEvents, UnityEngine.Random.Range(0, selectedEvents.Count));
            
            Game.Console.Log($"<color=#e8eb34>Event occured: {selectedEvents[0].Name}</color>");  

            _selectedEvents.Add(selectedEvents[0]);
            return selectedEvents[0];
        }

        List<RaidEventInstance> SelectRaidToOccur(WorldEventData eventData)
        {
            List<RaidEventInstance> selectedEvents = new();

            int chance = UnityEngine.Random.Range(1, 100);
            foreach (RaidEventInstance raidInstance in eventData.RaidTypes)
            {
                if (raidInstance.ChanceToOccur >= chance) selectedEvents.Add(raidInstance);
            }
            if (selectedEvents.Count == 0)
            {
                Game.Console.Log($"<color=#e8eb34>No raid event selected.</color>");
                return null;
            }
            else if (selectedEvents.Count > 1 && !eventData.AllowMultipleEvents)
                selectedEvents = KeepOnlyAtIndex(selectedEvents, UnityEngine.Random.Range(0, selectedEvents.Count));

            foreach (RaidEventInstance raidInstance in selectedEvents)
                {
                    Game.Console.Log($"<color=#e8eb34>Event occured: {raidInstance.Name}</color>");
                    _selectedEvents.Add(raidInstance);
                }
            
            return selectedEvents;
        }

        // void SelectEvent(WorldEventData eventData)
        // {            
        //     List<object> selectedEvents;

        //     // int chance = UnityEngine.Random.Range(1, 100);
        //     // foreach (EventPrefab eventPrefab in eventData.EventPrefab)
        //     // {
        //     //     if (eventPrefab.ChanceToOccur >= chance) selectedEvents.Add(eventPrefab);
        //     // }
            
        //     if (selectedEvents.Count == 0)
        //     {
        //         Game.Console.Log($"<color=#e8eb34>No event prefab selected.</color>");
        //         return null;
        //     }
        //     else if (selectedEvents.Count > 1 && !eventData.AllowMultipleEvents)
        //         selectedEvents = KeepOnlyAtIndex(selectedEvents, UnityEngine.Random.Range(0, selectedEvents.Count));
            
        //     foreach (EventPrefab eventPrefab in selectedEvents)
        //         Game.Console.Log($"<color=#e8eb34>Event occured: {eventPrefab.Name}</color>");
            
        //     _selectedEvents = selectedEvents;
        // }

        List<T> KeepOnlyAtIndex<T>(List<T> originalList, int index)
        {
            return new List<T> { originalList[index] };
        }
    }
}