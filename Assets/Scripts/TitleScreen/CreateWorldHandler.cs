using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using TMPro;

using UZSG.Worlds;
using UZSG.UI;

namespace UZSG.TitleScreen
{
    public class CreateWorldHandler : MonoBehaviour
    {
        [SerializeField] MapSelectorHandler mapSelector;

        [Header("Entries")]
        [SerializeField] MapEntryUI mapEntry;
        Dictionary<RuleTypeEnum, RuleEntryInputFieldUI> ruleEntriesInputField = new();
        Dictionary<RuleTypeEnum, RuleEntryToggleUI> ruleEntriesToggle = new();

        [Header("Components")]
        [SerializeField] FrameController parentFrameController;
        [SerializeField] Frame frame;
        [SerializeField] TextMeshProUGUI messageTmp;
        [SerializeField] TMP_InputField worldnameInput;
        [SerializeField] Button createBtn;

        void Awake()
        {
            createBtn.onClick.AddListener(CreateWorld);

            parentFrameController.OnSwitchFrame += (ctx) =>
            {
                if (ctx.NextId.Equals(this.frame.Id, StringComparison.OrdinalIgnoreCase))
                {
                    SetRandomSeed();
                }
            };
        }

        void Start()
        {
            HideMessage();
            createBtn.interactable = true;
            mapSelector.OnEntryClicked += OnMapSelect;

            foreach (var rule in GetComponentsInChildren<RuleEntryInputFieldUI>())
            {
                ruleEntriesInputField[rule.RuleType] = rule;
            }
            foreach (var rule in GetComponentsInChildren<RuleEntryToggleUI>())
            {
                ruleEntriesToggle[rule.RuleType] = rule;
            }
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

            int seed;
            if (int.TryParse(ruleEntriesInputField[RuleTypeEnum.Seed].Value, out var value))
            {
                seed = value;
            }
            else
            {
                seed = ruleEntriesInputField[RuleTypeEnum.Seed].Value.GetHashCode();
            }

            var options = new CreateWorldOptions()
            {
                WorldName = worldnameInput.text,
                MapId = mapEntry.LevelData.Id,
                Seed = seed,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };

            Game.World.CreateWorld(ref options, OnCreateWorldCompleted);
        }

        void SetRandomSeed()
        {
            ruleEntriesInputField[RuleTypeEnum.Seed].Value = UnityEngine.Random.Range(WorldAttributes.MIN_SEED, WorldAttributes.MAX_SEED).ToString();
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
                Game.Main.LoadScene(options, () =>
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
                Game.World.CurrentWorld.OnInitializeDone += OnWorldInitializeDone;
                Game.World.InitializeWorld();
                
                Game.Main.UnloadScene(SceneNames.TitleScreen);
            }
            else if (result.Result == Result_u.Failed)
            {
                Game.Main.LoadScene(
                    new(){
                        SceneToLoad = SceneNames.TitleScreen,
                        Mode = LoadSceneMode.Single
                    });
            }
        }

        void OnWorldInitializeDone()
        {
            Game.World.CurrentWorld.OnInitializeDone -= OnWorldInitializeDone;
            Game.Main.UnloadScene(SceneNames.LoadingScreen);
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