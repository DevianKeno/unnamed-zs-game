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
        ExternalAccountInfo userInfo;

        [SerializeField] NetworkObject networkObject;

        #region Tracked network variables
        NetworkVariable<Vector3> nPosition = new(Vector3.zero);
        // NetworkVariable<Quaternion> nRotation = new(Quaternion.identity);

        #endregion

        void Awake()
        {
            this.player = GetComponent<Player>();
            this.networkObject = GetComponent<NetworkObject>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner) 
            {
                SetUserIdServerRpc(Game.EOS.GetProductUserId().ToString());

                /// NOTE: should you allow player movement controls only on all initializations completed? 
            }

            if (IsServer)
            {
                var psd = Game.World.CurrentWorld.GetPlayerSaveData(Game.EOS.GetProductUserId());
                player.ReadSaveData(psd);
                if (IsOwner) player.InitializeAsClient();
            }
            else
            {
                EOSSubManagers.P2P.RequestPlayerSaveData(
                    EOSSubManagers.Lobbies.CurrentLobby.OwnerProductUserId,
                    onCompleted: OnRequestPlayerSaveDataCompleted);
            }
            
            InitializeNetworkVariableTrackings();

            Game.Console.LogInfo($"[World]: {this.userInfo.DisplayName} has entered the world");
        }

        void InitializeNetworkVariableTrackings()
        {
            nPosition.OnValueChanged += OnPositionChanged;
        }

        void OnRequestPlayerSaveDataCompleted(PlayerSaveData saveData, Epic.OnlineServices.Result result)
        {
            if (!IsOwner) return;

            if (result == Epic.OnlineServices.Result.Success && saveData != null)
            {
                player.ReadSaveData(saveData);
                player.InitializeAsClient();
            }
        }

        void OnPositionChanged(Vector3 oldPos, Vector3 newPos)
        {
            player.Position = newPos;
        }

        [ServerRpc]
        void SetUserIdServerRpc(string uid)
        {
            var puid = ProductUserId.FromString(uid);
            if (puid != null && puid.IsValid())
            {
                this.productUserId = puid;
                EOSSubManagers.UserInfo.QueryUserInfoByProductId(puid, onCompleted: (accountInfo, userId, result) =>
                {
                    if (result == Epic.OnlineServices.Result.Success &&
                        userId == puid)
                    {
                        this.userInfo = accountInfo;
                    }
                });
            }
            else
            {
                Game.Console.LogWarn($"Error setting puid for network player entity: [{uid}]");
            }
        }
    }
}