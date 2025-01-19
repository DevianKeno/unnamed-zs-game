using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Data;
using UZSG.Systems;
using UZSG.UI;

namespace UZSG.TitleScreen
{
    public class MapSelectorHandler : MonoBehaviour
    {
        [SerializeField] List<MapEntryUI> entries = new();
        public List<MapEntryUI> Entries => entries;

        public event Action<MapEntryUI> OnEntryClicked;

        [SerializeField] FrameController parentFrameController;
        [SerializeField] Transform entriesContainer;
        [SerializeField] GameObject mapEntryPrefab;

        public void ReadMaps()
        {
            ClearEntries();
            
            foreach (LevelData data in Resources.LoadAll<LevelData>("Data/Levels"))
            {
                if (!data.Scene.IsSet())
                {
                    Game.Console.LogWarn($"There is no Scene set for LevelData '{data.Id}'");
                    continue;
                }
                if (!data.Enable)
                {
                    Debug.Log($"Map '{data.Id}' is available and disabled ");
                    continue;
                }

                var entry = Instantiate(mapEntryPrefab, entriesContainer).GetComponent<MapEntryUI>();
                entry.SetLevelData(data);
                entry.OnClicked += OnClickedEntry;
                entry.OnClicked += (e) =>
                {
                    parentFrameController.SwitchToFrame("world");
                };
                entries.Add(entry);
            }

            /// Sort alphabetically
            entries.Sort((entry1, entry2) => string.Compare(entry1.LevelData.DisplayName, entry2.LevelData.DisplayName, StringComparison.OrdinalIgnoreCase));
            // Rearrange game objects
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].transform.SetSiblingIndex(i);
            }
        }

        public void OnClickedEntry(MapEntryUI entry)
        {
            OnEntryClicked?.Invoke(entry);
        }

        public void ClearEntries()
        {
            foreach (MapEntryUI t in entries)
            {
                Destroy(t.gameObject);
            }
            entries.Clear();
        }
    }
}