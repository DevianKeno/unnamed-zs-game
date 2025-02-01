using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

using Epic.OnlineServices;

using UZSG.Entities;
using UZSG.EOS;
using UZSG.EOS.Lobbies;
using UZSG.Parties;
using UZSG.Systems;

namespace UZSG.Network
{
    public partial class PlayerNetworkEntity : NetworkBehaviour
    {
        #region Party handling

        public void CreateParty()
        {
            if (!IsLocalPlayer) return;

            if (!NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer)
            {
                if (CurrentParty != null)
                {
                    Game.Console.LogInfo($"You're still currently in a party. If you want, leave it first with /party leave");
                    return;
                }

                CreateParty_ServerRpc();
            }
        }

        /// <summary>
        /// Send a request to server that you want to create a party.
        /// </summary>
        [Rpc(SendTo.Server)]
        void CreateParty_ServerRpc(RpcParams rpcParams = default)
        {
            var senderClientId = rpcParams.Receive.SenderClientId;
            
            if (false == EOSSubManagers.Transport.GetEOSTransport().TryGetProductUserIdMapping(senderClientId, out ProductUserId puid))
            {
                Game.Console.LogDebug($"Failed to retrieve puid mapping for ClientId: [{senderClientId}]");
                return;
            }
            if (false == EOSSubManagers.Lobbies.FindLobbyMember(puid, out var lobbyMember))
            {
                Game.Console.LogDebug($"User[{puid}] is not in the lobby.");
                return;
            }

            /// TODO: VALIDATE IF ALLOW OTHER PLAYERS TO CREATE PARTIES
            /// TODO: if parties are named, check if they already exists
            bool canCreate = true;
            if (!canCreate)
            {
                Game.Console.LogInfo("You are not allowed to create a party in this world.");
                return;
            }
            
            var res = Resources.Load("Prefabs/Party (Network Object)");
            var newParty = Instantiate(res).GetComponent<Party>();
            newParty.NetworkObject.SpawnWithOwnership(senderClientId, destroyWithScene: true);

            CurrentParty = newParty;
            if (IsServer)
            {
                parties.Add(newParty);
            }

            Game.Console.LogInfo("You have created a party. Invite others with /party invite <username>");
        }
        
        public void InviteToParty(string usernameToInvite)
        {
            if (CurrentParty == null)
            {
                Game.Console.LogInfo($"Must be in a party to invite! Join one, or create one using /party create");
                return;
            }
            
            CurrentParty.InvitePlayer(usernameToInvite);
        }

        public void LeaveCurrentParty()
        {
            if (CurrentParty == null)
            {
                Game.Console.LogInfo("You're not in a party.");
                return;
            }

            CurrentParty.Leave();
            // if (CurrentParty.HostIs(this.Player))
            // {
            //     /// Notify all members before disbanding
            //     foreach (var member in CurrentParty.Members)
            //     {
            //         if (member == this.Player) continue;
                    
            //         if (EOSSubManagers.Transport.GetEOSTransport().TryGetClientIdMapping(member.NetworkEntity.ProductUserId, out ulong clientId))
            //         {
            //             NotifyPartyDisbandRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
            //         }
            //     }
            //     CurrentParty.Disband();
            // }
            // else
            // {
            //     RequestLeaveParty_ServerRpc();
            // }
        }
        
        #endregion
    }
}