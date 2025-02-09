using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Newtonsoft.Json;
using TMPro;

using UZSG.Saves;

using UZSG.UI;
using UZSG.UI.TitleScreen;
using UZSG.Worlds;

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
                if (!context.Frame.Id.Equals(this.frame.Id, StringComparison.OrdinalIgnoreCase))
                {
                    this.selector?.Hide();
                }
                if (context.Frame.Id.Equals(this.frame.Id, StringComparison.OrdinalIgnoreCase))
                {
                    var tmp = playBtn.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        if (isHosting) tmp.text = "Host selected world";
                        else tmp.text = "Play selected world";
                    }
                }
            };
        }

        bool isHosting; 
        public void SetHosting(bool value)
        {
            isHosting = value;
        }

        public async void ReadWorlds()
        {
            await ReadWorldsAsync();
        }

        public async Task ReadWorldsAsync()
        {
            await Task.Yield();

            loadingTmp.gameObject.SetActive(true);
            ClearEntries();

            var savedWorldsPath = Path.Join(Application.persistentDataPath, WorldManager.WORLDS_FOLDER);
            if (!Directory.Exists(savedWorldsPath)) Directory.CreateDirectory(savedWorldsPath);
            
            var worldPaths = Directory.GetDirectories(savedWorldsPath, "*", SearchOption.TopDirectoryOnly);
            List<WorldManifest> manifests = await LoadWorldManifestsAsync(worldPaths);

            foreach (var manifest in manifests)
            {
                var entry = Game.UI.Create<WorldEntryUI>("World Entry UI", parent: entryContainer);
                entry.SetManifest(manifest);
                entry.OnClick += OnEntryClicked;
                entries.Add(entry);
            }

            loadingTmp.gameObject.SetActive(false);
        }

        async Task<List<WorldManifest>> LoadWorldManifestsAsync(string[] worldPaths)
        {
            return await Task.Run(async () =>
            {
                List<WorldManifest> manifests = new();

                foreach (var worldPath in worldPaths)
                {
                    var manifestPath = Path.Join(worldPath, "world.manifest");
                    if (!File.Exists(manifestPath)) continue;

                    string json = await File.ReadAllTextAsync(manifestPath); // Read file asynchronously
                    var manifest = JsonConvert.DeserializeObject<WorldManifest>(json);
                    manifest.WorldRootDirectory = worldPath;
                    manifests.Add(manifest);
                }

                return manifests;
            });
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
                hostWorldHandler.SetWorld(selectedEntry);
            }
            else
            {
                playBtn.interactable = false;

                var options = new Game.LoadSceneOptions()
                {
                    SceneToLoad = "LoadingScreen",
                    Mode = LoadSceneMode.Additive,
                    ActivateOnLoad = true,
                };
                Game.Main.LoadSceneAsync(options, OnLoadingScreenLoaded);
            }
        }

        void OnLoadingScreenLoaded()
        {
            Game.World.LoadWorldFromFilepathAsync(selectedEntry.LevelDataPath, OnLoadWorldCompleted);
        }

        void OnLoadWorldCompleted(WorldManager.LoadWorldResult result)
        {
            if (result.Result == Result_u.Success)
            {
                selector.Hide();
                Game.World.CurrentWorld.OnInitializeDone += OnWorldInitializeDone;
                Game.World.InitializeWorld();
                
                Game.Main.UnloadScene("TitleScreen");
                Game.Main.UnloadScene("LoadingScreen");
            }
            else if (result.Result == Result_u.Failed)
            {
                Game.Main.LoadSceneAsync(
                    new(){
                        SceneToLoad = "TitleScreen",
                        Mode = LoadSceneMode.Single
                    });
                playBtn.interactable = true;
            }
        }

        void OnWorldInitializeDone()
        {
            Game.World.CurrentWorld.OnInitializeDone -= OnWorldInitializeDone;
            Game.Main.UnloadScene("LoadingScreen");
        }

        void OnDeleteBtnClick()
        {
            throw new NotImplementedException();
        }
    }
}