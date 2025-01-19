using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

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
using UZSG.Data;

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

        [Header("Debugging")]
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

            currentWorldAttributes = new WorldAttributes
            {
                MaxPlayers = WorldAttributes.DEFAULT_MAX_NUM_PLAYERS
            };
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

            if (EOSSubManagers.Auth.IsLoggedIn)
            {
                var localUser = Game.EOS.GetLocalUserId();
                if (localUser != null && localUser.IsValid())
                {
                    Game.Console.LogInfo($"Creating lobby...");
                    EOSSubManagers.Lobbies.CreateLobby(CreateLobbyFromProperties(), OnCreateLobbyCompleted);
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
            // var localUser = EOSSubManagers.UserInfo.GetLocalUserInfo();
            // var country = localUser.Country.ToString();
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
            
            newLobby.AddAttribute(new()
            {
                Key = AttributeKeys.WORLD_NAME,
                AsString = selectedWorldEntry.SaveData.WorldName,
                ValueType = AttributeType.String,
                Visibility = LobbyAttributeVisibility.Public
            });

            var levelData = Resources.Load<LevelData>($"Data/Levels/{selectedWorldEntry.SaveData.LevelId}");
            newLobby.AddAttribute(new()
            {
                Key = AttributeKeys.LEVEL_DISPLAY_NAME,
                AsString = levelData.DisplayName,
                ValueType = AttributeType.String,
                Visibility = LobbyAttributeVisibility.Public
            });

            string levelId = selectedWorldEntry.SaveData.LevelId.ToString();
            newLobby.AddAttribute(new()
            {
                Key = AttributeKeys.LEVEL_ID,
                AsString = levelId,
                ValueType = AttributeType.String,
                Visibility = LobbyAttributeVisibility.Public
            });

            /// NOTE: There's a promote member method in the lobby manager I suppose this is supposed to be there instead
            var localUserInfo = EOSSubManagers.UserInfo.GetLocalUserInfo();
            newLobby.AddAttribute(new()
            {
                Key = AttributeKeys.LOBBY_OWNER_DISPLAY_NAME,
                AsString = localUserInfo.DisplayName,
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
                if (Game.Main.EnableDebugMode)
                {
                    string msg = $"Created lobby with id: {EOSSubManagers.Lobbies.CurrentLobby.Id}";
                    Game.Console.LogDebug(msg);
                    print(msg);
                }

                EOSSubManagers.Lobbies.PromoteMember(Game.EOS.GetProductUserId(), OnPromoteMemberCompleted);
                // EOSSubManagers.Lobbies.AddNotifyMemberUpdateReceived(OnMemberUpdate);
            }
            else
            {
                string msg = $"Error creating lobby. [" + result + "]";
                Game.Console.LogInfo(msg);
                Debug.LogError(msg);

                startBtn.interactable = true;
            }
        }

        void OnMemberUpdate(string lobbyId, ProductUserId memberId)
        {   
            Game.Console.LogDebug($"NotifyMemberUpdateReceived, Id: {memberId}");
            
        }

        void OnPromoteMemberCompleted(Result result)
        {
            if (result == Result.Success)
            {
                LoadWorld();
            }
            else
            {
                string msg = $"Error promoting owner: [" + result + "]";
                Game.Console.LogInfo(msg);
                Debug.LogError(msg);
                
                startBtn.interactable = true;
            }
        }

        void LoadWorld()
        {
            if (Game.Main.EnableDebugMode && !loadWorldUponHost) return;
            
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