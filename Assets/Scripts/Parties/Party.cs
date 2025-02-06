using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Netcode;
using UnityEngine;

using Epic.OnlineServices;

using UZSG.Entities;
using UZSG.EOS;
using UZSG.EOS.Lobbies;


namespace UZSG.Parties
{
    /// <summary>
    /// Party system when <b>within a world</b>. For sharing xp and stuff.
    /// </summary>
    public class Party : NetworkBehaviour
    {
        const int INVITE_TIMEOUT_SECONDS = 60;

        public Player OwnerPlayer { get; set; }
        public ProductUserId OwnerPuid { get; set; }

        public bool OnlyHostCanInvite = true;
        public uint MaxPlayers = 8;
        public string DisplayName { get; set; }
        public string Description { get; set; }

        Dictionary<ulong, PartyMember> localMembers;
        NetworkList<ulong> membersClientId_nVar = new();
        /// <summary>
        /// List of client Ids we have incoming/outgoing invites for this party.
        /// <c>ulong</c> is clientId
        /// </summary>
        NetworkList<PendingInvite> currentInvites_nVar = new();

        public override void OnNetworkSpawn()
        {
        }

        public bool HostIs(ulong clientId)
        {
            return this.OwnerClientId == clientId;
        }
        
        public void InvitePlayer(string usernameToInvite)
        {
            if (OnlyHostCanInvite && !IsOwner)
            {
                Game.Console.LogInfo($"Only the party owner can invite other players.");
                return;
            }
            if (false == FindLobbyMemberBy(usernameToInvite, out var lobbyMemberToInvite))
            {
                Game.Console.LogInfo($"{usernameToInvite} is not in the lobby");
                return;
            }
            /// Check if invite is already in our party
            if (FindMemberBy(puid: lobbyMemberToInvite.ProductUserId, out _))
            {
                Game.Console.LogInfo($"{usernameToInvite} is already in your party.");
                return;
            }
            if (false == TryGetClientIdFromPuid(lobbyMemberToInvite.ProductUserId, out var lobbyMemberClientId))
            {
                Game.Console.LogDebug($"Failed to get client Id mapping for puid:[{lobbyMemberToInvite.ProductUserId}]");
                return;
            }

            /// Check if we have sent an invite before
            foreach (var i in currentInvites_nVar)
            {
                if (i.ClientId != lobbyMemberClientId) continue;
                /// Previous invite exists
                /// Check if the invite before is expired
                if (Time.time < i.SentTime + INVITE_TIMEOUT_SECONDS) /// Not yet expired
                {
                    Game.Console.LogInfo($"{usernameToInvite} is already being invited to this party.");
                    return;
                }
                /// Previous Invite is expired, send and notify server
            }
            
            SendPartyInvite_ServerRpc(usernameToInvite, OwnerPlayer.DisplayName, lobbyMemberClientId);
        }
        
        [Rpc(SendTo.Server)]
        void SendPartyInvite_ServerRpc(string toUsername, string fromUsername, ulong clientIdToInvite, RpcParams rpcParams = default)
        {
            PendingInvite previousInvite = default;
            bool previousInviteExists = false;
            foreach (var i in currentInvites_nVar)
            {
                if (i.ClientId != clientIdToInvite) continue;
                previousInvite = i;
                previousInviteExists = true;
            }
            if (previousInviteExists)
            {
                currentInvites_nVar.Remove(previousInvite);
            }
            
            currentInvites_nVar.Add(new PendingInvite(clientIdToInvite, Time.time));
            AcknowledgeSendPartyInvite_Rpc(toUsername, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
            ReceivePartyInvite_ServerRpc(fromUsername, RpcTarget.Single(clientIdToInvite, RpcTargetUse.Temp));
        }

        /// <summary>
        /// Send a feedback message to client that invite has been sent.
        /// </summary>
        [Rpc(SendTo.SpecifiedInParams)]
        void AcknowledgeSendPartyInvite_Rpc(string toUsername, RpcParams rpcParams = default)
        {
            Game.Console.LogInfo($"Sent party invite to {toUsername}");
        }

        /// <summary>
        /// Receive a party invite from a user.
        /// </summary>
        /// <param name="fromPuidStr">The PUID string of the user who sent the invite</param>
        [Rpc(SendTo.SpecifiedInParams)]
        void ReceivePartyInvite_ServerRpc(string fromUsername, RpcParams rpcParams = default)
        {
            if (FindLobbyMemberBy(displayName: fromUsername, out var lobbyMember))
            {
                Game.Console.LogInfo($"Received party invite from {fromUsername}. Accept with /party accept {fromUsername}");
            }
        }

        /// <summary>
        /// Accepts if you are being invited here.
        /// </summary>
        public void AcceptPartyInvite()
        {
            bool hasInvite = false;
            foreach (var invite in currentInvites_nVar)
            {
                if (invite.ClientId != NetworkManager.Singleton.LocalClientId) continue;
                hasInvite = true;
            }

            if (hasInvite)
            {
                SendAcceptPartyInvite_ServerRpc();
            }
            else
            {
                Game.Console.LogInfo($"You have no such invite from {OwnerPlayer.DisplayName}");
            }
        }

        /// <summary>
        /// Executes on the inviter's instance on the server.
        /// </summary>
        [Rpc(SendTo.Server)]
        void SendAcceptPartyInvite_ServerRpc(RpcParams rpcParams = default)
        {
            var senderClientId = rpcParams.Receive.SenderClientId;
            
            PendingInvite invite = default;
            bool hasInvite = false;
            foreach (var i in currentInvites_nVar)
            {
                if (invite.ClientId != senderClientId) continue;
                hasInvite = true;
                invite = i;
            }

            if (hasInvite)
            {
                /// Remove the invite after accepting
                currentInvites_nVar.Remove(invite);
                ReceiveResponseAcceptPartyInvite_Rpc(isAccepted: true, RpcTarget.Single(senderClientId, RpcTargetUse.Temp));
                AddMember_ServerRpc(senderClientId);
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ReceiveResponseAcceptPartyInvite_Rpc(bool isAccepted, RpcParams rpcParams = default)
        {
            if (isAccepted)
            {
                Game.Console.LogInfo($"You have joined {OwnerPlayer.DisplayName}'s party.");
            }
        }   

        [Rpc(SendTo.Server)]
        void AddMember_ServerRpc(ulong clientIdToAdd)
        {
            if (false == TryGetPuidFromClientId(clientIdToAdd, out var puid))
            {
                Game.Console.LogDebug($"[Party/AddMember()]: ClientId [{clientIdToAdd}] does not exist!");
                return;
            };
            if (false == FindLobbyMemberBy(puid, out var member))
            {
                Game.Console.LogDebug($"[Party/AddMember()]: User[{puid}] is not in the lobby!");
                return;
            }

            foreach (var memberClientId in membersClientId_nVar)
            {
                NotifyPartyMemberAdded_Rpc(clientIdToAdd, RpcTarget.Single(memberClientId, RpcTargetUse.Temp));
            }
            membersClientId_nVar.Add(clientIdToAdd);
            if (InitializePartyMember(clientIdToAdd, out var partyMember))
            {
                localMembers[clientIdToAdd] = partyMember;
                Game.Console.LogInfo($"{partyMember} has joined your party.");
            }
        }

        bool InitializePartyMember(ulong clientId, out PartyMember partyMember)
        {
            partyMember = default;
            if (false == TryGetPuidFromClientId(clientId, out var puid))
            {
                return false;
            }
            if (false == FindLobbyMemberBy(puid, out var lobbyMember))
            {
                return false;
            }
            if (false == Game.World.CurrentWorld.GetPlayer(clientId: clientId, out var player))
            {
                return false;
            }

            partyMember = new PartyMember()
            {
                ClientId = clientId,
                ProductUserId = puid,
                DisplayName = lobbyMember.DisplayName,
                Player = player,
            };
            
            return true;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void NotifyPartyMemberAdded_Rpc(ulong clientId, RpcParams rpcParams = default)
        {
            if (InitializePartyMember(clientId, out var partyMember))
            {
                localMembers[clientId] = partyMember;
                Game.Console.LogInfo($"{partyMember} has joined your party.");
            }
        }

        /// <summary>
        /// Send a request to the server that you want to leave the party.
        /// </summary>
        [Rpc(SendTo.Server)]
        void RequestLeaveParty_ServerRpc(RpcParams rpcParams = default)
        {
            var senderClientId = rpcParams.Receive.SenderClientId;

            if (false == membersClientId_nVar.Contains(senderClientId))
            {
                Game.Console.LogDebug($"Member client Id does not exist.");
                return;
            }
            if (false == FindMember(senderClientId, out var partyMember))
            {
                Game.Console.LogDebug($"Member does not exist.");
                return;
            }

            membersClientId_nVar.Remove(senderClientId);
            ReceiveLeavePartyRpc(RpcTarget.Single(senderClientId, RpcTargetUse.Temp));

            foreach (var memberClientId in membersClientId_nVar)
            {
                NotifyPartyMemberLeft_Rpc(senderClientId, RpcTarget.Single(memberClientId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ReceiveLeavePartyRpc(RpcParams rpcParams = default)
        {
            Game.Console.LogInfo("You have left the party.");
        }


        [Rpc(SendTo.SpecifiedInParams)]
        void NotifyPartyMemberLeft_Rpc(ulong clientId, RpcParams rpcParams = default)
        {
            if (localMembers.TryGetValue(clientId, out var member))
            {
                Game.Console.LogInfo($"{member.DisplayName} has left your party.");
                localMembers.Remove(clientId);
            }
        }

        public void KickFromParty(string username)
        {
            throw new NotImplementedException();
        }

        [Rpc(SendTo.Server)]
        void KickFromParty_ServerRpc(string username)
        {
            
        }
        
        public void Leave()
        {
            if (IsOwner)
            {
                Disband();
            }
            else
            {

            }
        }

        public void Disband()
        {
            if (!IsOwner)
            {
                return;
            }
        }

        [Rpc(SendTo.Everyone)]
        void NotifyPartyDisbandRpc(RpcParams rpcParams = default)
        {
            Game.Console.LogInfo("Your party has been disbanded.");
        }

        public void RemoveMember(Player player)
        {
            
        }

        public bool FindMember(ulong clientId, out PartyMember partyMember)
        {
            partyMember = localMembers.Values.ToList().Find((p) => p.ClientId == clientId);
            return false;
        }

        public bool FindMember(string displayName, out PartyMember partyMember)
        {
            partyMember = localMembers.Values.ToList().Find((p) => p.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            return partyMember != null;
        }

        public bool FindMemberBy(ProductUserId puid, out PartyMember partyMember)
        {
            partyMember = localMembers.Values.ToList().Find((p) => p.ProductUserId.Equals(puid));
            return partyMember != null;
        }
        
        #region Utils

        bool FindLobbyMemberBy(string displayName, out LobbyMember lobbyMember)
        {
            return EOSSubManagers.Lobbies.FindMemberByDisplayName(displayName, out lobbyMember);
        }

        bool FindLobbyMemberBy(ProductUserId puid, out LobbyMember lobbyMember)
        {
            return EOSSubManagers.Lobbies.FindLobbyMember(puid, out lobbyMember);
        }

        bool TryGetPuidFromClientId(ulong clientId, out ProductUserId puid)
        {
            return EOSSubManagers.Transport.GetEOSTransport().TryGetProductUserIdMapping(clientId, out puid);
        }

        bool TryGetClientIdFromPuid(ProductUserId puid, out ulong clientId)
        {
            return EOSSubManagers.Transport.GetEOSTransport().TryGetClientIdMapping(puid, out clientId);
        }

        #endregion
    }
}