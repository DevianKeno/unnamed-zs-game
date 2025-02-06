using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

using UZSG.Data;


namespace UZSG.Worlds.Events
{
    public class WorldEvent
    {
        [FormerlySerializedAs("_eventData"), SerializeField] WorldEventData eventData;
        public WorldEventData EventData
        {
            get => eventData;
            set => eventData = value;
        }
        
        public event Action<WorldEvent> OnEventSpawned;
        public event Action<WorldEvent> OnEventEnded;

        List<object> _selectedEvents = new();
        public List<object> SelectedEvents => _selectedEvents;

        public void SpawnEvent()
        {
            OnEventSpawned?.Invoke(this);
        }
        
        public List<object> PrepareEvent()
        {
            switch (eventData.Type)
            {
                // case WorldEventType.Weather:
                //     SelectWeatherToOccur(_eventData);
                //     break;
                case WorldEventType.Raid:
                    SelectRaidToOccur(eventData);
                    break;
                default:
                    return null;
            };

            return _selectedEvents;
        }

        // void SelectWeatherToOccur(WorldEventData eventData)
        // {
        //     List<Wea> selectedEvents = new();
            
        //     if (selectedEvents.Count == 0)
        //     {
        //         Game.Console.LogInfo($"<color=#e8eb34>No weather event selected.</color>");
        //         return;
        //     }
        //     else if (selectedEvents.Count > 1 && !eventData.AllowMultipleEvents)
        //     {
        //         selectedEvents = KeepOnlyAtIndex(selectedEvents, UnityEngine.Random.Range(0, selectedEvents.Count));
        //     }
            
        //     Game.Console.LogInfo($"<color=#e8eb34>Event occured: {selectedEvents[0].Name}</color>");  

        //     _selectedEvents.Add(selectedEvents[0]);
        // }

        void SelectRaidToOccur(WorldEventData eventData)
        {
            List<RaidEventInstance> selectedEvents = new();

            int chance = UnityEngine.Random.Range(1, 100);
            foreach (RaidEventInstance raidInstance in eventData.RaidTypes)
            {
                if (raidInstance.ChanceToOccur >= chance) selectedEvents.Add(raidInstance);
            }
            if (selectedEvents.Count == 0)
            {
                Game.Console.LogInfo($"<color=#e8eb34>No raid event selected.</color>");
                return;
            }
            else if (selectedEvents.Count > 1 && !eventData.AllowMultipleEvents)
                selectedEvents = KeepOnlyAtIndex(selectedEvents, UnityEngine.Random.Range(0, selectedEvents.Count));

            foreach (RaidEventInstance raidInstance in selectedEvents)
                {
                    Game.Console.LogInfo($"<color=#e8eb34>Event occured: {raidInstance.Name}</color>");
                    _selectedEvents.Add(raidInstance);
                }
        }

        List<T> KeepOnlyAtIndex<T>(List<T> originalList, int index)
        {
            return new List<T> { originalList[index] };
        }
    }
}