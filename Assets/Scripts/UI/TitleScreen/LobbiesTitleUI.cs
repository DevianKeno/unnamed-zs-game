using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using UZSG.Systems;
using UZSG.EOS;
using UZSG.EOS.Lobbies;

namespace UZSG.UI.Lobbies
{
    public class LobbiesHandlerUI : MonoBehaviour
    {
        EOSLobbyManager lobbyManager => EOSSubManagers.Lobbies;

        Lobby _selectedLobby = null;
        LobbyDetails _selectedLobbyDetails = null;
        List<LobbyEntryUI> _lobbyEntriesUI = new();
        
        [Header("Scripts")]
        // [SerializeField] LobbyInfoContainer lobbyInfo;

        [Header("UI Elements")]
        [SerializeField] GameObject lobbyContainer;
        [SerializeField] TMP_InputField searchField;
        [SerializeField] Button refreshBtn;
        [SerializeField] Button joinBtn;
        [SerializeField] LoadingIconAnimated loadingIcon;

        [Header("Prefabs")]
        [SerializeField] GameObject lobbyEntryPrefab;

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
            _selectedLobby = null;
            _selectedLobbyDetails = null;
            ClearLobbyEntries();
            // lobbyInfo.Clear();
            joinBtn.gameObject.SetActive(false);
            loadingIcon.gameObject.SetActive(true);
            /// HMMMMMMMMMMMMMMMMMMMMMMMM
            string searchString = searchField.text; 
            lobbyManager.SearchByAttribute("RULESET", searchString, OnSearchCompleted);
        }
        
        public void JoinSelectedLobby()
        {
            if (_selectedLobby == null)
            {
                return;
            }
            lobbyManager.JoinLobby(_selectedLobby.Id, _selectedLobbyDetails, false, OnJoinLobbyCompleted);
        }

        #endregion


        #region EOS callbacks

        void OnSearchCompleted(Result result)
        {
            loadingIcon.gameObject.SetActive(false);

            if (result == Result.Success)
            {
                Dictionary<Lobby, LobbyDetails> searchResults = lobbyManager.GetSearchResults();
                ClearLobbyEntries();
                
                foreach (var kv in searchResults)
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
                    _lobbyEntriesUI.Add(entry);
                }
            } else
            {
                Game.Console.Log($"Error searching lobbies. [" + result + "]");
                Debug.LogError("Error searching lobbies. [" + result + "]");
            }
        }

        void OnJoinLobbyCompleted(Result result)
        {
            if (result == Result.Success)
            {
                if (_selectedLobby.TryGetAttribute("RULESET", out var attr))
                {
                    // Ruleset ruleset = attr.AsString switch
                    // {
                    //     "Standard" => Ruleset.CreateFromPreset(RulesetPreset.Classic),
                    //     "Speed" => Ruleset.CreateFromPreset(RulesetPreset.Speed),
                    //     "Custom" => Ruleset.CreateFromPreset(RulesetPreset.Custom),
                    // };
                    // Game.Main.CreateMatch(ruleset);
                    // Game.Main.LoadScene("Match", playTransition: true);
                }
            }
            else
            {
                Game.Console.Log($"Error joining lobby. [" + result + "]");
                Debug.LogError("Error joining lobby. [" + result + "]");
            }
        }

        #endregion


        void ClearLobbyEntries()
        {
            foreach (LobbyEntryUI entry in _lobbyEntriesUI)
            {
                Destroy(entry.gameObject);
            }
            _lobbyEntriesUI.Clear();
        }

        void OnClickLobbyEntry(LobbyEntryUI lobbyEntry)
        {
            _selectedLobby = lobbyEntry.Lobby;
            _selectedLobbyDetails = lobbyEntry.LobbyDetails;
            // lobbyInfo.SetLobbyInfo(lobbyEntry.Lobby);
            joinBtn.gameObject.SetActive(true);
        }
    }
}