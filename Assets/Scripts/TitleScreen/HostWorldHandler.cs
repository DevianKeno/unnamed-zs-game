using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using UZSG.Saves;
using UZSG.Systems;
using UZSG.UI;
using UZSG.UI.TitleScreen;
using UZSG.Worlds;
using UZSG.EOS;
using UZSG.EOS.Lobbies;

using static UZSG.Systems.Status;
using System.Text;
using Unity.VisualScripting;

namespace UZSG.TitleScreen
{
    public class HostWorldHandler : MonoBehaviour
    {
        WorldAttributes currentWorldAttributes;
        /// <summary>
        /// <c>string</c> is rule Id.
        /// </summary>
        Dictionary<string, RuleEntryUI> ruleEntriesDict;
        Dictionary<RuleTypeEnum, object> ruleLinks;
        
        [Header("UI Elements")]
        [SerializeField] FrameController frameController;
        [SerializeField] WorldEntryUI selectedWorldEntry;
        [SerializeField] Button startBtn;

        ProductUserId localProductUserId => Game.EOS.GetProductUserId();

        [Header("Debugging")]
        [SerializeField] bool enableDebugging = false;
        [SerializeField] bool loadWorldUponHost = false;

        void Awake()
        {
            startBtn.onClick.AddListener(OnStartBtnClick);
        }

        void Start()
        {
            ruleEntriesDict = new();
            ruleLinks = new();
            
            foreach (RuleEntryUI entry in GetComponentsInChildren<RuleEntryUI>())
            {
                ruleEntriesDict[entry.Id] = entry;
                entry.OnValueChanged += OnRuleValueChanged;
            }

            selectedWorldEntry.OnClick += (entry) =>
            {
                frameController.SwitchToFrame("worlds");
            };

            currentWorldAttributes = new WorldAttributes();
            ruleLinks[RuleTypeEnum.DaytimeLength] = currentWorldAttributes.DayLengthSeconds;
            ruleLinks[RuleTypeEnum.NighttimeLength] = currentWorldAttributes.NightLengthSeconds;
            ruleLinks[RuleTypeEnum.MaxPlayers] = currentWorldAttributes.MaxPlayers;
            /// TODO: few for testing, soon add the rest
        }


        #region Event callbacks

        void OnStartBtnClick()
        {
            startBtn.interactable = false;
            CreateLobby();
        }

        void OnRuleValueChanged(RuleEntryUI sender, object value)
        {
            if (ruleLinks.ContainsKey(sender.Rule))
            {
                ruleLinks[sender.Rule] = value;
                Debug.Log($"Updated world attribute '{sender.Rule}' to a value of: {value.ToString()}");
            }
        }
        
        #endregion
        
        
        public void SetWorld(WorldSaveData worldSaveData)
        {
            selectedWorldEntry.SetData(worldSaveData);
        }
        
        public void CreateLobby()
        {
            WorldAttributes.Validate(ref currentWorldAttributes);

            if (Game.Main.IsOnline)
            {
                var loginStatus = Game.EOS.GetEOSAuthInterface().GetLoginStatus(Game.EOS.GetLocalUserId());
                if (loginStatus == LoginStatus.LoggedIn)
                {
                    if (!localProductUserId.IsValid())
                    {
                        Debug.LogError("Lobbies (CreateLobby): Current player is invalid!");
                        return;
                    }

                    EOSSubManagers.Lobbies.CreateLobby(CreateLobbyFromProperties(), OnCreateLobbyCompleted);
                    return;
                }
                else if (loginStatus == LoginStatus.UsingLocalProfile)
                {
                    startBtn.interactable = true;

                    Game.Console.LogInfo($"Unhandled login status." + loginStatus);
                    Debug.LogError($"Unhandled login status." + loginStatus);
                    return;
                }
            }

            /// fallback to local host
            LoadWorld();
        }
        
        /// <summary>
        /// Creates a lobby.
        /// </summary>
        public Lobby CreateLobbyFromProperties()
        {
            var region = "ASIA";
            var world = selectedWorldEntry.SaveData;
            var bucketId = $"{region}:{world.LevelId}";

            var newLobby = new Lobby()
            {
                BucketId = bucketId,
                AllowInvites = true,
                PresenceEnabled = false,
                RTCRoomEnabled = false,
                LobbyPermissionLevel = LobbyPermissionLevel.Publicadvertised,
            };

            /// -- Lobby attributes --
            newLobby.SetWorldAttributes(currentWorldAttributes);
            newLobby.AddAttribute(new()
            {
                Key = AttributeKeys.GAME_VERSION,
                AsString = Game.Main.GetVersionString(),
                ValueType = AttributeType.String,
                Visibility = LobbyAttributeVisibility.Public
            });

            string levelId = currentWorldAttributes.LevelId.ToSafeString();
            newLobby.AddAttribute(new()
            {
                Key = AttributeKeys.LEVEL_ID,
                AsString = levelId,
                ValueType = AttributeType.String,
                Visibility = LobbyAttributeVisibility.Public
            });
            
            newLobby.AddAttribute(new()
            {
                Key = AttributeKeys.ANTICHEAT,
                AsBool = false, /// False for now :(
                ValueType = AttributeType.Boolean,
                Visibility = LobbyAttributeVisibility.Public
            });
            
            return newLobby;
        }

        void OnCreateLobbyCompleted(Result result)
        {
            if (result == Result.Success)
            {
                if (enableDebugging)
                {
                    Debug.Log($"Created lobby with id: {EOSSubManagers.Lobbies.CurrentLobby.Id}");
                }
                
                EOSSubManagers.Lobbies.PromoteMember(localProductUserId, OnPromoteMemberCompleted);
                EOSSubManagers.Lobbies.AddNotifyMemberUpdateReceived(OnMemberUpdate);
            }
            else
            {
                startBtn.interactable = true;
                Game.Console.LogInfo($"Error creating lobby. [" + result + "]");
                Debug.LogError("Error creating lobby. [" + result + "]");
            }
        }

        void OnMemberUpdate(string LobbyId, ProductUserId MemberId)
        {   
            Debug.Log($"NotifyMemberUpdateReceived, Id: {MemberId}");
        }

        void OnPromoteMemberCompleted(Result result)
        {
            if (result == Result.Success)
            {
                LoadWorld();
            }
            else
            {
                startBtn.interactable = true;

                Game.Console.LogInfo($"Error promoting owner. [" + result + "]");
                Debug.LogError("Error promoting owner. [" + result + "]");
            }
        }

        void LoadWorld()
        {
            if (enableDebugging && !loadWorldUponHost) return;
            
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
                        Filepath = selectedWorldEntry.Filepath,
                        WorldSaveData = selectedWorldEntry.SaveData,
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
                startBtn.interactable = true;
            }
        }
    }
}