using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

using UZSG.Saves;
using UZSG.Systems;
using UZSG.UI;
using UZSG.UI.TitleScreen;
using static UZSG.Systems.Status;
using UZSG.Worlds;

namespace UZSG.TitleScreen
{
    public class ContinueHandler : MonoBehaviour
    {
        WorldEntryUI selectedEntry = null;
        List<WorldEntryUI> entries = new();
        Selector selector = null;

        [SerializeField] FrameController parentFrameController;
        [SerializeField] Button playBtn;
        [SerializeField] Button deleteBtn;
        [SerializeField] Transform entryContainer;
        [SerializeField] TextMeshProUGUI loadingTmp;
        [SerializeField] GameObject worldEntryPrefab;

        void Awake()
        {
            InitializeEvents();
        }

        void InitializeEvents()
        {
            playBtn.onClick.AddListener(OnPlayBtnClick);
            deleteBtn.onClick.AddListener(OnDeleteBtnClick);
            parentFrameController.OnSwitchFrame += (context) =>
            {
                if (context.Frame.Id != "worlds")
                {
                    selectedEntry = null;
                    Destroy(selector.gameObject);
                }
            };
        }

        public void ReadWorlds()
        {
            loadingTmp.gameObject.SetActive(true);
            List<WorldSaveData> saveDataList = new();
            var savedWorldsPath = Path.Join(Application.persistentDataPath, "SavedWorlds");
            if (!Directory.Exists(savedWorldsPath)) Directory.CreateDirectory(savedWorldsPath);
            var worldPaths = Directory.GetDirectories(savedWorldsPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var path in worldPaths)
            {
                var datFile = Path.Join(path, "level.dat");
                if (!File.Exists(datFile)) continue;
                var json = File.ReadAllText(datFile);
                try
                {
                    var saveData = JsonConvert.DeserializeObject<WorldSaveData>(json);
                    saveDataList.Add(saveData);
                } catch (Exception e)
                {
                    Game.Console.Error($"Failed to deserialize level data for world '{Path.GetFileName(path)}'!");
                    continue;
                }
            }

            ClearEntries();
            foreach (var sd in saveDataList)
            {
                var entry = Game.UI.Create<WorldEntryUI>("World Entry UI");
                entry.SetParent(entryContainer);
                entry.SetData(sd);
                entry.OnClick += OnEntryClicked;
                entries.Add(entry);
            }
            loadingTmp.gameObject.SetActive(false);
        }

        void ClearEntries()
        {
            foreach (WorldEntryUI entry in entries)
            {
                Destroy(entry.gameObject);
            }
            entries.Clear();
        }

        void OnEntryClicked(WorldEntryUI entry)
        {
            if (selector == null) CreateSelector();

            selectedEntry = entry;
            selector.Select(entry.transform as RectTransform);
        }
        
        void OnPlayBtnClick()
        {
            if (selectedEntry == null) return;

            playBtn.interactable = false;
            Game.Main.LoadScene(
                new(){
                    SceneToLoad = "LoadingScreen",
                    Mode = LoadSceneMode.Additive,
                    ActivateOnLoad = true,
                },
                onLoadSceneCompleted: () =>
                {
                    var options = new WorldManager.LoadWorldOptions()
                    {
                        OwnerId = Game.World.GetLocalUserId(),
                        WorldSaveData = selectedEntry.SaveData,
                    };

                    Game.World.LoadWorld(options, OnLoadWorldCompleted);
                });
        }

        void OnLoadWorldCompleted(WorldManager.LoadWorldResult result)
        {
            if (result.Status == Success)
            {
                selector.Hide();
                
                Game.Main.UnloadScene("TitleScreen");
                Game.Main.UnloadScene("LoadingScreen");
            }
            else if (result.Status == Failed)
            {
                Game.Main.LoadScene(
                    new(){
                        SceneToLoad = "TitleScreen",
                        Mode = LoadSceneMode.Single
                    });
                playBtn.interactable = true;
            }
        }

        void OnDeleteBtnClick()
        {
            throw new NotImplementedException();
        }

        void CreateSelector()
        {
            selector = Game.UI.Create<Selector>("Selector");
        }
    }
}