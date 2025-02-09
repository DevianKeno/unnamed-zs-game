using System;
using System.Collections.Generic;

using Unity.Netcode;
using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

using UZSG.EOS;
using UZSG.Entities;
using UZSG.Saves;

using UZSG.Parties;

namespace UZSG.Network
{
    /// <summary>
    /// Player entity network functionalities.
    /// </summary>
    [RequireComponent(typeof(Player))]
    public partial class PlayerNetworkEntity : NetworkBehaviour
    {
        public Player Player { get; protected set; }
        
        public string DisplayName;
        ProductUserId productUserId;
        public ProductUserId ProductUserId => productUserId;
        ExternalAccountInfo accountInfo;
        public ExternalAccountInfo AccountInfo => accountInfo;
        /// <summary>
        /// The party this player is currently on. 
        /// </summary>
        public Party CurrentParty;
        /// <summary>
        /// List of parties created in the server. Includes those created by the host and by other players.
        /// </summary>
        List<Party> parties;

        [SerializeField] NetworkObject networkObject;

        #region Network variables
        NetworkVariable<Vector3> nPosition = new(Vector3.zero);
        NetworkVariable<Vector3> nRotationEuler = new(Vector3.zero);

        #endregion


        void Awake()
        {
            this.Player = GetComponent<Player>();
            this.networkObject = GetComponent<NetworkObject>();
        }

        bool _enableTracking = false;
        void FixedUpdate()
        {
            if (!_enableTracking) return;

            if (IsServer)
            {
                nPosition.Value = Player.Position;
                nRotationEuler.Value = Player.Rotation.eulerAngles;
            }
        }

        public override void OnNetworkSpawn()
        {
            RequestUserInfoByClientIdRpc(this.OwnerClientId);
                        
            if (IsServer)
            {
                var psd = Game.World.CurrentWorld.GetPlayerSaveData(Game.EOS.GetProductUserId());
                this.Player.InitializeAsPlayer(psd, IsLocalPlayer);
                /// Server spawned this player, which is already stored in cache
                parties = new();
            }
            else
            {
                EOSSubManagers.P2P.RequestPlayerSaveData(EOSSubManagers.Lobbies.CurrentLobby.OwnerProductUserId, onCompleted: OnRequestPlayerSaveDataCompleted);
                Game.World.CurrentWorld.CachePlayer(this.Player, this.OwnerClientId);
            }
            
            InitializeNetworkVariableTrackings();
            Game.World.CurrentWorld.OnNetworkPlayerJoined(this.Player);

            Game.Console.LogInfo($"[World]: {this.accountInfo.DisplayName} has entered the world");
        }

        public override void OnNetworkDespawn()
        {
            Game.World.CurrentWorld.OnNetworkPlayerLeft(this.Player);

            _enableTracking = false;
            nPosition.OnValueChanged -= OnPositionChanged;
            nRotationEuler.OnValueChanged -= OnRotationChanged;
        }
        
        void InitializeNetworkVariableTrackings()
        {
            if (!IsOwner)
            {
                nPosition.OnValueChanged += OnPositionChanged;
                nRotationEuler.OnValueChanged += OnRotationChanged;
            }
            _enableTracking = true;
        }

        void OnRequestPlayerSaveDataCompleted(PlayerSaveData saveData, Result result)
        {
            if (!IsOwner) return;

            if (result == Result.Success)
            {
                this.Player.InitializeAsPlayer(saveData, isLocalPlayer: true);
            }
        }

        void OnPositionChanged(Vector3 oldPos, Vector3 newPos)
        {
            this.Player.Position = newPos;
        }

        void OnRotationChanged(Vector3 oldPos, Vector3 newPos)
        {
            this.Player.Rotation = Quaternion.Euler(newPos);
        }

        [Rpc(SendTo.Server)]
        void RequestUserInfoByClientIdRpc(ulong clientId, RpcParams rpcParams = default)
        {
            if (EOSSubManagers.Transport.GetEOSTransport().TryGetProductUserIdMapping(clientId, out ProductUserId puid))
            {
                if (puid == null || !puid.IsValid())
                {
                    Game.Console.LogWarn($"Client:[{rpcParams.Receive.SenderClientId}] requested user info for Client:[{clientId}], but PUID:[{puid}] is invalid!");
                    return;
                }
                
                this.productUserId = puid;
                EOSSubManagers.UserInfo.QueryUserInfoByProductId(puid, OnQueryUserInfoByProductIdCompleted);
                void OnQueryUserInfoByProductIdCompleted(ExternalAccountInfo userInfo, ProductUserId userId, Epic.OnlineServices.Result result)
                {
                    if (result == Epic.OnlineServices.Result.Success &&
                        userId == puid)
                    {
                        this.accountInfo = userInfo;
                        Player.DisplayName = userInfo.DisplayName;
                        ReceiveUserInfoRpc(userInfo.DisplayName, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
                        Player.SetNametagVisible(true);
                    }
                }
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ReceiveUserInfoRpc(string displayName, RpcParams rpcParams)
        {
            Player.DisplayName = displayName;
            if (!IsOwner)
            {
                Player.SetNametagVisible(true);
            }
        }
    }
}