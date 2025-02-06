using System;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using UZSG.Worlds;

namespace UZSG.TitleScreen
{
    public class NewWorldHandler : MonoBehaviour
    {
        [SerializeField] MapSelectorHandler mapSelector;

        [Header("Entries")]
        [SerializeField] MapEntryUI mapEntry;

        [Header("Components")]
        [SerializeField] TextMeshProUGUI messageTmp;
        [SerializeField] TMP_InputField worldnameInput;
        [SerializeField] Button createBtn;

        void Awake()
        {
            createBtn.onClick.AddListener(CreateWorld);
        }

        void Start()
        {
            HideMessage();
            createBtn.interactable = true;
            mapSelector.OnEntryClicked += OnMapSelect;
        }

        void OnMapSelect(MapEntryUI entry)
        {
            if (entry == null) return;

            mapEntry.SetLevelData(entry.LevelData);
        }

        public void CreateWorld()
        {
            createBtn.interactable = false;

            if (!ValidateCreatingWorld())
            {
                createBtn.interactable = true;
                return;
            }

            var options = new CreateWorldOptions()
            {
                WorldName = worldnameInput.text,
                MapId = mapEntry.LevelData.Id,
                Seed = 12345,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };

            Game.World.CreateWorld(ref options, OnCreateWorldCompleted);
        }

        bool ValidateCreatingWorld()
        {
            if (string.IsNullOrEmpty(worldnameInput.text))
            {
                SetMessage("World name cannot be empty");
                return false;
            }

            if (mapEntry.LevelData == null)
            {
                SetMessage("Select a map");
                return false;
            }

            return true;
        }

        void OnCreateWorldCompleted(WorldManager.CreateWorldResult result)
        {
            if (result.Result == Result_u.Success)
            {
                var options = new Game.LoadSceneOptions()
                {
                    SceneToLoad = "LoadingScreen",
                    Mode = LoadSceneMode.Single,
                    ActivateOnLoad = true,
                };
                Game.Main.LoadSceneAsync(options, () =>
                {
                    OnLoadingScreenLoaded(result.FilePath);
                });
            }
            else if (result.Result == Result_u.Failed)
            {
                Debug.LogError("Unexpected error occured when creating world");
                createBtn.interactable = true;
            }
        }

        void OnLoadingScreenLoaded(string filepath)
        {
            Game.World.LoadWorldFromFilepathAsync(filepath, OnLoadWorldCompleted);
        }

        void OnLoadWorldCompleted(WorldManager.LoadWorldResult result)
        {
            if (result.Result == Result_u.Success)
            {
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
            }
        }

        public void LoadWorld(string path)
        {
            // Game.World.LoadWorld(path);
        }

        public void SetMessage(string msg)
        {
            messageTmp.gameObject.SetActive(true);
            messageTmp.text = msg;
        }
        
        public void HideMessage()
        {
            messageTmp.gameObject.SetActive(false);
        }
    }
}