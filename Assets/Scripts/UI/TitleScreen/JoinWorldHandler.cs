using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using UZSG.Systems;
using UZSG.EOS;
using UZSG.EOS.Lobbies;
using UZSG.UI.TitleScreen;

using static UZSG.Systems.Status;
using UZSG.Saves;
using UZSG.EOS.P2P;
using Unity.VisualScripting;

namespace UZSG.UI.Lobbies
{
    public class JoinWorldHandler : MonoBehaviour
    {
        EOSLobbyManager lobbyManager => EOSSubManagers.Lobbies;

        Lobby selectedLobby = null;
        LobbyDetails selectedLobbyDetails = null;
        List<LobbyEntryUI> lobbyEntriesUI = new();
        
        [Header("Scripts")]
        // [SerializeField] LobbyInfoContainer lobbyInfo;
        Selector selector = null;

        [Header("UI Elements")]
        [SerializeField] FrameController parentFrameController;
        [SerializeField] Frame frame;
        [SerializeField] GameObject lobbyContainer;
        [SerializeField] TMP_InputField searchField;
        [SerializeField] Button refreshBtn;
        [SerializeField] Button joinBtn;
        [SerializeField] LoadingIconAnimated loadingIcon;

        [Header("Prefabs")]
        [SerializeField] GameObject lobbyEntryPrefab;

        void Awake()
        {
            parentFrameController.OnSwitchFrame += (context) =>
            {
                if (context.Frame.Id != this.frame.Id)
                {
                    this.selector?.Hide();
                }
            };
        }

        void OnEnable()
        {
            refreshBtn.onClick.AddListener(SearchLobbies);
            joinBtn.onClick.AddListener(JoinSelectedLobby);
        }

        void OnDisable()
        {
            refreshBtn.onClick.RemoveListener(SearchLobbies);
            joinBtn.onClick.RemoveListener(JoinSelectedLobby);
        }


        #region Public methods

        public void SearchLobbies()
        {
            selectedLobby = null;
            selectedLobbyDetails = null;
            ClearLobbyEntries();
            joinBtn.gameObject.SetActive(false);
            loadingIcon.gameObject.SetActive(true);
            lobbyManager.SearchByAttribute(AttributeKeys.GAME_VERSION, Game.Main.GetVersionString(), OnSearchCompleted);
        }
        
        public void JoinSelectedLobby()
        {
            if (selectedLobby == null) return;

            joinBtn.gameObject.SetActive(false);
            lobbyManager.JoinLobby(selectedLobby.Id, selectedLobbyDetails, false, OnJoinLobbyCompleted);
        }

        #endregion


        #region EOS callbacks

        void OnSearchCompleted(Result result)
        {
            loadingIcon.gameObject.SetActive(false);

            if (result == Result.Success)
            {
                ClearLobbyEntries();
                
                foreach (var kv in lobbyManager.SearchResults)
                {
                    var lobby = kv.Key;
                    var lobbyDetails = kv.Value;
                    var go = Instantiate(lobbyEntryPrefab, lobbyContainer.transform);
                    LobbyEntryUI entry = go.GetComponent<LobbyEntryUI>();
                    entry.SetLobbyInfo(lobby, lobbyDetails);
                    entry.OnClick += (object sender, EventArgs e) =>
                    {
                        OnClickLobbyEntry(sender as LobbyEntryUI);
                    };

                    if (lobby.TryGetAttribute(AttributeKeys.GAME_VERSION, out var attr))
                    {
                        entry.SetVersionMismatch(!Utils.IsSameVersion(attr.AsString));
                    }

                    lobbyEntriesUI.Add(entry);
                }
            }
            else
            {
                Game.Console.LogInfo($"An error occured when searching lobbies. [" + result + "]");
                Debug.LogError("Error searching lobbies. [" + result + "]");
            }
        }

        void OnJoinLobbyCompleted(Result result)
        {
            if (result == Result.Success)
            {
                if (selectedLobby.TryGetAttribute(AttributeKeys.LEVEL_ID, out var attr))
                {
                    EOSSubManagers.P2P.RequestWorldData(selectedLobby.LobbyOwner, OnRequestWorldDataCompleted);
                }
            }
            else
            {
                joinBtn.gameObject.SetActive(true);

                Game.Console.LogInfo($"Error joining lobby. [" + result + "]");
                Debug.LogError("Error joining lobby. [" + result + "]");
            }
        }

        #endregion
        
        void OnClickLobbyEntry(LobbyEntryUI entry)
        {
            selectedLobby = entry.Lobby;
            selectedLobbyDetails = entry.LobbyDetails;
            selector = GetOrCreateSelectorUI();
            selector.Select(entry.transform as RectTransform);
            joinBtn.gameObject.SetActive(true);
        }

        void OnRequestWorldDataCompleted(string filepath)
        {
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
                        OwnerId = EOSSubManagers.Lobbies.CurrentLobby.LobbyOwnerAccountId.ToString(),
                        Filepath = filepath,
                        WorldSaveData = Game.World.DeserializeWorldData(filepath),
                    };
                    Game.World.LoadWorld(options, OnLoadWorldCompleted);
                });
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
                joinBtn.interactable = true;
            }
        }

        void ClearLobbyEntries()
        {
            selector?.Hide();
            foreach (LobbyEntryUI entry in lobbyEntriesUI)
            {
                Destroy(entry.gameObject);
            }
            lobbyEntriesUI.Clear();
        }

        Selector GetOrCreateSelectorUI()
        {
            if (selector == null)
            {
                selector = Game.UI.Create<Selector>("Selector", parent: lobbyContainer.transform);
                var le = selector.AddComponent<LayoutElement>();
                le.ignoreLayout = true;
            }
            return selector;
        }
    }
}