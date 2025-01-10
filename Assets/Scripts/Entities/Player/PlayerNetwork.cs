using System;

using Unity.Netcode;
using UnityEngine;

using UZSG.EOS;
using UZSG.Entities;

namespace UZSG.Network
{
    /// <summary>
    /// Player entity network functionalities.
    /// </summary>
    public class PlayerNetworkObject : NetworkBehaviour
    {
        Player player;
        NetworkVariable<Vector3> nPosition = new(Vector3.zero);
        NetworkVariable<Quaternion> nRotation = new(Quaternion.identity);

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                SetUserIdServerRpc(EOSSubManagers.UserInfo.GetLocalUserInfo().ToString());
            }

            nPosition.OnValueChanged += OnPositionChanged;
        }

        void OnPositionChanged(Vector3 oldPos, Vector3 newPos)
        {
            UpdatePosition();
        }

        void UpdatePosition()
        {
            player.Position = nPosition.Value;
        }

        [ServerRpc]
        private void SetUserIdServerRpc(string id)
        {
            // userId.Value = id;
        }
    }
}