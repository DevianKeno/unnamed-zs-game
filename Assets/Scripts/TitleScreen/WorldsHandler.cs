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
using Unity.VisualScripting;

namespace UZSG.TitleScreen
{
    public class WorldsHandler : MonoBehaviour
    {
        WorldEntryUI selectedEntry = null;
        List<WorldEntryUI> entries = new();
        Selector selector = null;

        [Header("UI Elements")]
        [SerializeField] FrameController parentFrameController;
        [SerializeField] Frame frame;
        [SerializeField] Button playBtn;
        [SerializeField] Button deleteBtn;
        [SerializeField] Transform entryContainer;
        [SerializeField] TextMeshProUGUI loadingTmp;
        [SerializeField] GameObject worldEntryPrefab;

        [Header("Handlers")]
        [SerializeField] HostWorldHandler hostWorldHandler;

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
                if (context.Frame.Id != this.frame.Id)
                {
                    this.selector?.Hide();
                }
            };
        }

        bool isHosting; 
        public void SetHosting(bool value)
        {
            isHosting = value;
        }

        public void ReadWorlds()
        {
            /// TODO: make a manifest file, read that instead of the entire save data
            loadingTmp.gameObject.SetActive(true);
            List<WorldSaveData> loadedSaveDatas = new();
            var savedWorldsPath = Path.Join(Application.persistentDataPath, "SavedWorlds");
            if (!Directory.Exists(savedWorldsPath)) Directory.CreateDirectory(savedWorldsPath);
            var worldPaths = Directory.GetDirectories(savedWorldsPath, "*", SearchOption.TopDirectoryOnly);
            
            ClearEntries();

            foreach (var path in worldPaths)
            {
                var datFile = Path.Join(path, "level.dat");
                if (!File.Exists(datFile)) continue;
                var json = File.ReadAllText(datFile);
                var saveData = Game.World.DeserializeWorldData(datFile);

                if (saveData == null) continue;
                        
                var entry = Game.UI.Create<WorldEntryUI>("World Entry UI");
                entry.SetParent(entryContainer);
                entry.SetData(saveData);
                entry.Filepath = datFile;
                entry.OnClick += OnEntryClicked;
                entries.Add(entry);
            }

            loadingTmp.gameObject.SetActive(false);
        }

        void ClearEntries()
        {
            if (selector != null)
            {
                selector.Hide();
            }
            foreach (WorldEntryUI entry in entries)
            {
                Destroy(entry.gameObject);
            }
            entries.Clear();
        }

        void OnEntryClicked(WorldEntryUI entry)
        {
            selectedEntry = entry;
            if (selector == null)
            {
                selector ??= Game.UI.Create<Selector>("Selector", parent: entryContainer);
                var le = selector.AddComponent<LayoutElement>();
                le.ignoreLayout = true;
            }
            selector.Select(entry.transform as RectTransform);
        }
        
        void OnPlayBtnClick()
        {
            if (selectedEntry == null) return;

            if (isHosting)
            {
                parentFrameController.SwitchToFrame("host_world");
                hostWorldHandler.SetWorld(selectedEntry.SaveData);
            }
            else
            {
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
                            Filepath = selectedEntry.Filepath,
                            WorldSaveData = selectedEntry.SaveData,
                        };

                        Game.World.LoadWorld(options, OnLoadWorldCompleted);
                    });
            }
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
    }
}