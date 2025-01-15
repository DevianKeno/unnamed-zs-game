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

                    Game.Console.Log($"Unhandled login status." + loginStatus);
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
            /// The top-level, game-specific filtering information for session searches.
            /// This criteria should be set with mostly static, coarse settings,
            /// often formatted like "GameMode:Region:MapName".
            var rulesetType = "SAMPLE";
            var region = "ASIA";
            var bucketId = $"{rulesetType}:{region}";

            var newLobby = new Lobby()
            {
                BucketId = bucketId,
                AllowInvites = true,
                PresenceEnabled = true,
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
                Key = AttributeKeys.LEVEL_ID,
                AsString = currentWorldAttributes.LevelId,
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
                EOSSubManagers.Lobbies.PromoteMember(localProductUserId, OnPromoteMemberCompleted);
            }
            else
            {
                startBtn.interactable = true;
                Game.Console.Log($"Error creating lobby. [" + result + "]");
                Debug.LogError("Error creating lobby. [" + result + "]");
            }
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

                Game.Console.Log($"Error promoting owner. [" + result + "]");
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