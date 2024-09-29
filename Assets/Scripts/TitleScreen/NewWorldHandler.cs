using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UZSG.Systems;

using static UZSG.Systems.Status;

namespace UZSG.TitleScreen
{
    public class NewWorldHandler : MonoBehaviour
    {
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
        }

        public void CreateWorld()
        {
            createBtn.interactable = false;

            if (string.IsNullOrEmpty(worldnameInput.text))
            {
                SetMessage("World name cannot be empty");
                createBtn.interactable = true;
                return;
            }

            var options = new CreateWorldOptions()
            {
                WorldName = worldnameInput.text,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };

            Game.World.CreateWorld(ref options, OnCreateWorldCompleted);
        }

        void OnCreateWorldCompleted(WorldManager.CreateWorldResult result)
        {
            if (result.Status == Success)
            {
                // Game.Main.LoadScene(
                //     new(){
                //         Name = "World",
                //         Mode = LoadSceneMode.Additive,
                //         ActivateOnLoad = true
                //     });
                Game.Main.LoadScene(
                    new(){
                        Name = "LoadingScreen",
                        Mode = LoadSceneMode.Additive,
                        ActivateOnLoad = true,
                    },
                    onLoadSceneCompleted: () =>
                    {
                        Game.World.LoadWorldAsync(result.Savepath, OnLoadWorldCompleted);
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
                Game.Main.LoadScene(
                    new(){
                        Name = "World",
                        Mode = LoadSceneMode.Single
                    });
                /// unload only on success lol
                Game.Main.UnloadScene("TitleScreen");
                Game.Main.UnloadScene("LoadingScreen");
            }
            else if (result.Status == Failed)
            {
                Game.Main.UnloadScene("LoadingScreen");
                Game.Main.LoadScene(
                    new(){
                        Name = "TitleScreen",
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