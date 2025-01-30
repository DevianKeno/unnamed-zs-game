using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using Epic.OnlineServices.Lobby;

using UZSG.Systems;
using UZSG.EOS;
using UZSG.EOS.Lobbies;

namespace UZSG.UI.Lobbies
{
    public class JoinWorldHandler : MonoBehaviour
    {
        [SerializeField] EOSTransport EOSTransport;
        Lobby selectedLobby = null;
        LobbyDetails selectedLobbyDetails = null;
        List<LobbyEntryUI> lobbyEntriesUI = new();
        
        string serverWorldFilepath = string.Empty;
        string serverLevelId = string.Empty;
        
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
            EOSSubManagers.Lobbies.SearchByAttribute(AttributeKeys.GAME_VERSION, Game.Main.GetVersionString(), OnSearchCompleted);
        }
        
        public void JoinSelectedLobby()
        {
            if (selectedLobby == null) return;

            joinBtn.gameObject.SetActive(false);
            EOSSubManagers.Lobbies.JoinLobby(selectedLobby.Id, selectedLobbyDetails, false, OnJoinLobbyCompleted);
        }

        #endregion


        #region EOS callbacks

        void OnSearchCompleted(Epic.OnlineServices.Result result)
        {
            loadingIcon.gameObject.SetActive(false);

            if (result == Epic.OnlineServices.Result.Success)
            {
                ClearLobbyEntries();
                
                foreach (var kv in EOSSubManagers.Lobbies.SearchResults)
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

        void OnJoinLobbyCompleted(Epic.OnlineServices.Result result)
        {
            if (result == Epic.OnlineServices.Result.Success)
            {
                EOSTransport.SetConnection(selectedLobby.OwnerProductUserId);
                EOSSubManagers.Transport.StartClient();

                if (selectedLobby.TryGetAttribute(AttributeKeys.LEVEL_ID, out var attr))
                {
                    this.serverWorldFilepath = string.Empty;
                    this.serverLevelId = attr.AsString;
                    
                    EOSSubManagers.P2P.RequestWorldSaveData(selectedLobby.OwnerProductUserId, OnRequestWorldSaveDataCompleted);
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

        /// <summary>
        /// TODO: add result, leave lobby upon fail
        /// </summary>
        /// <param name="filepath">The location of the downloaded world save data from the server</param>
        void OnRequestWorldSaveDataCompleted(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) return;

            this.serverWorldFilepath = filepath;
            var loadOptions = new Game.LoadSceneOptions()
            {
                SceneToLoad = "LoadingScreen",
                Mode = LoadSceneMode.Single,
                ActivateOnLoad = true,
            };
            Game.Main.LoadSceneAsync(loadOptions, OnLoadingScreenLoaded);
        }

        void OnLoadingScreenLoaded()
        {
            Game.World.LoadWorldFromFilepathAsync(this.serverWorldFilepath, OnLoadWorldCompleted);
        }

        void OnLoadWorldCompleted(WorldManager.LoadWorldResult result)
        {
            if (result.Result == Result.Success)
            {
                Game.World.InitializeWorld();

                Game.Main.UnloadScene("TitleScreen");
                Game.Main.UnloadScene("LoadingScreen");
            }
            else if (result.Result == Result.Failed)
            {
                BackToTitleScreen();
            }
        }

        void BackToTitleScreen(bool leaveCurrentLobby = true)
        {
            if (leaveCurrentLobby)
            {
                EOSSubManagers.Lobbies.LeaveCurrentLobby();
            }

            Game.Main.LoadSceneAsync(
                new(){
                    SceneToLoad = "TitleScreen",
                    Mode = LoadSceneMode.Single
                });
            joinBtn.interactable = true;
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
                var le = selector.gameObject.AddComponent<LayoutElement>();
                le.ignoreLayout = true;
            }
            return selector;
        }
    }
}