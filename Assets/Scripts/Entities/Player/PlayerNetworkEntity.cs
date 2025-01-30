using System;

using Unity.Netcode;
using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.UserInfo;
using Epic.OnlineServices.Connect;

using UZSG.EOS;
using UZSG.Entities;
using UZSG.Saves;
using UZSG.Systems;

namespace UZSG.Network
{
    /// <summary>
    /// Player entity network functionalities.
    /// </summary>
    [RequireComponent(typeof(Player))]
    public class PlayerNetworkEntity : NetworkBehaviour
    {
        [SerializeField] protected Player player;
        public Player Player => player;
        ProductUserId productUserId;
        public ProductUserId ProductUserId => productUserId;
        ExternalAccountInfo accountInfo;
        public ExternalAccountInfo AccountInfo => accountInfo;

        [SerializeField] NetworkObject networkObject;

        #region Network variables
        NetworkVariable<Vector3> nPosition = new(Vector3.zero);
        NetworkVariable<Vector3> nRotationEuler = new(Vector3.zero);

        #endregion


        void Awake()
        {
            this.player = GetComponent<Player>();
            this.networkObject = GetComponent<NetworkObject>();
        }

        bool _enableTracking = false;
        void FixedUpdate()
        {
            if (!_enableTracking) return;

            if (IsServer)
            {
                nPosition.Value = player.Position;
                nRotationEuler.Value = player.Rotation.eulerAngles;
            }
        }

        public override void OnNetworkSpawn()
        {
            RequestUserInfoByClientIdRpc(this.OwnerClientId);
                        
            if (IsServer)
            {
                var psd = Game.World.CurrentWorld.GetPlayerSaveData(Game.EOS.GetProductUserId());
                this.player.InitializeAsPlayer(psd, IsLocalPlayer);
            }
            else
            {
                EOSSubManagers.P2P.RequestPlayerSaveData(EOSSubManagers.Lobbies.CurrentLobby.OwnerProductUserId, onCompleted: OnRequestPlayerSaveDataCompleted);
            }
            
            InitializeNetworkVariableTrackings();

            Game.Console.LogInfo($"[World]: {this.accountInfo.DisplayName} has entered the world");
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

        void OnRequestPlayerSaveDataCompleted(PlayerSaveData saveData, Epic.OnlineServices.Result result)
        {
            if (!IsOwner) return;

            if (result == Epic.OnlineServices.Result.Success)
            {
                this.player.InitializeAsPlayer(saveData, isLocalPlayer: true);
            }
        }

        void OnPositionChanged(Vector3 oldPos, Vector3 newPos)
        {
            this.player.Position = newPos;
        }

        void OnRotationChanged(Vector3 oldPos, Vector3 newPos)
        {
            this.player.Rotation = Quaternion.Euler(newPos);
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
                        player.DisplayName = userInfo.DisplayName;
                        ReceiveUserInfoRpc(userInfo.DisplayName, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
                        player.SetNametagVisible(true);
                    }
                }
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ReceiveUserInfoRpc(string displayName, RpcParams rpcParams)
        {
            player.DisplayName = displayName;
            if (!IsOwner)
            {
                player.SetNametagVisible(true);
            }
        }

        public override void OnNetworkDespawn()
        {
            _enableTracking = false;
            
            nPosition.OnValueChanged -= OnPositionChanged;
            nRotationEuler.OnValueChanged -= OnRotationChanged;
        }
    }
}