using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UZSG.Data;
using UZSG.EOS;
using UZSG.Systems;
using UZSG.UI;
using static UZSG.Systems.Status;

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
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };

            var localUser = EOSSubManagers.UserInfo.GetLocalUserInfo();
            options.OwnerId = localUser.UserId.ToString();

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
            if (result.Status == Success)
            {
                Game.Main.LoadScene(
                    new(){
                        SceneToLoad = "LoadingScreen",
                        Mode = LoadSceneMode.Additive,
                        ActivateOnLoad = true,
                    },
                    onLoadSceneCompleted: () =>
                    {
                        Game.World.LoadWorld(result.Savepath, OnLoadWorldCompleted);
                    });
            }
            else if (result.Status == Failed)
            {
                Debug.LogError("Unexpected error occured when creating world");
                createBtn.interactable = true;
            }
        }

        void OnLoadWorldCompleted(WorldManager.LoadWorldResult result)
        {
            if (result.Status == Success)
            {
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