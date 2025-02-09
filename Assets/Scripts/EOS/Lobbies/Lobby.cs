using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using static Epic.OnlineServices.Result;
using PlayEveryWare.EpicOnlineServices;

using UZSG.Worlds;


namespace UZSG.EOS.Lobbies
{
    /// <summary>
    /// Represents lobby properties.
    /// </summary>
    public class Lobby
    {
        public string Id { get; internal set; }
        /// <summary>
        /// The top-level, game-specific filtering information for session searches.
        /// This criteria should be set with mostly static, coarse settings.
        /// Format <c>Region:MapName</c>.
        /// </summary>
        public string BucketId { get; internal set; }
        public ProductUserId OwnerProductUserId { get; internal set; }
        public EpicAccountId OwnerEpicId { get; internal set; }
        public LobbyPermissionLevel LobbyPermissionLevel = LobbyPermissionLevel.Publicadvertised;
        public uint MaxNumLobbyMembers = WorldAttributes.DEFAULT_MAX_NUM_PLAYERS;
        public uint AvailableSlots { get; private set; }
        public bool AllowInvites = true;
        public bool? DisableHostMigration;
        public string LobbyOwnerDisplayName;
        /// <summary>
        /// Cached copy of the RoomName of the RTC room that our lobby has, if any
        /// </summary>
        public string RTCRoomName = string.Empty;
        /// <summary>
        /// Are we currently connected to an RTC room?
        /// </summary>
        public bool RTCRoomConnected = false;
        /// <summary>
        /// Notification for RTC connection status changes
        /// </summary>
        public NotifyEventHandle RTCRoomConnectionChanged; /// EOS_INVALID_NOTIFICATIONID;
        /// <summary>
        // /Notification for RTC room participant updates (new players or players leaving)
        /// </summary>
        public NotifyEventHandle RTCRoomParticipantUpdate; /// EOS_INVALID_NOTIFICATIONID;
        /// <summary>
        /// Notification for RTC audio updates (talking status or mute changes)
        /// </summary>
        public NotifyEventHandle RTCRoomParticipantAudioUpdate; /// EOS_INVALID_NOTIFICATIONID;

        public bool PresenceEnabled = false;
        public bool RTCRoomEnabled = false;
        /// <summary>
        /// <c>string</c> is AttributeKey.
        /// </summary>
        Dictionary<string, LobbyAttribute> _attributesDict = new();
        /// <summary>
        /// Lobby attributes. [Read Only]
        /// </summary>
        public List<LobbyAttribute> Attributes => _attributesDict.Values.ToList();

        List<LobbyMember> members = new();
        public List<LobbyMember> Members => members;

        /// Utility data
        public bool _isSearchResult = false;
        public bool _isBeingCreated = false;

        public void AddAttribute(LobbyAttribute attribute)
        {
            _attributesDict[attribute.Key] = attribute;
        }

        /// <summary>
        /// Checks if Lobby Id is valid
        /// </summary>
        /// <returns>True if valid</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Id);
        }

        public bool TryGetAttribute(string key, out LobbyAttribute attribute)
        {
            return _attributesDict.TryGetValue(key, out attribute);
        }

        /// <summary>
        /// Checks if the member Id already exists in the lobby
        /// </summary>
        public bool FindLobbyMember(ProductUserId productId, out LobbyMember lobbyMember)
        {
            lobbyMember = members.Find((LobbyMember member) =>
            {
                return member.ProductUserId == productId;
            });
            return lobbyMember != null;
        }

        public List<LobbyMember> GetMembers()
        {
            return new();
        }

        /// <summary>
        /// Checks if the specified <c>ProductUserId</c> is the owner if this lobby.
        /// </summary>
        /// <param name="userProductId">Specified <c>ProductUserId</c></param>
        /// <returns>True if specified user is owner</returns>
        public bool IsOwner(ProductUserId userProductId)
        {
            return userProductId == OwnerProductUserId;
        }

        /// <summary>
        /// Clears local cache of Lobby Id, owner, attributes and members
        /// </summary>
        public void ClearCache()
        {
            Id = string.Empty;
            OwnerProductUserId = new ProductUserId();
            _attributesDict.Clear();
            members.Clear();
        }

        /// <summary>
        /// Initializing the given Lobby Id and caches all relevant attributes
        /// </summary>
        /// <param name="lobbyId">Specified Lobby Id</param>
        public void InitializeFromLobbyHandle(string lobbyId)
        {
            if (string.IsNullOrEmpty(lobbyId)) return;

            this.Id = lobbyId;
            var options = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = Id,
                LocalUserId = Game.EOS.GetProductUserId()
            };
            var copyLobbyDetailsResult = Game.EOS.GetEOSLobbyInterface().CopyLobbyDetailsHandle(ref options, out LobbyDetails lobbyDetails); 
            
            if (copyLobbyDetailsResult != Success)
            {
                Debug.LogErrorFormat("Lobbies (InitFromLobbyHandle): can't get lobby info handle. Error code: {0}", copyLobbyDetailsResult);
                return;
            }
            if (lobbyDetails == null)
            {
                Debug.LogError("Lobbies (InitFromLobbyHandle): can't get lobby info handle. outLobbyDetailsHandle is null");
                return;
            }

            InitializeFromLobbyDetails(lobbyDetails);
        }

        /// <summary>
        /// Initializing the given <c>LobbyDetails</c> handle and caches all relevant attributes
        /// </summary>
        /// <param name="lobbyId">Specified <c>LobbyDetails</c> handle</param>
        public void InitializeFromLobbyDetails(LobbyDetails lobbyDetails)
        {
            /// Get owner
            var lobbyDetailsGetLobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId newLobbyOwnerProductId = lobbyDetails.GetLobbyOwner(ref lobbyDetailsGetLobbyOwnerOptions);
            if (!IsOwner(newLobbyOwnerProductId))
            {
                OwnerProductUserId = newLobbyOwnerProductId;
                OwnerEpicId = new EpicAccountId();
                LobbyOwnerDisplayName = string.Empty; /// TODO:
            }

            /// Copy lobby info
            var lobbyDetailsCopyInfoOptions = new LobbyDetailsCopyInfoOptions();
            var copyInfoResult = lobbyDetails.CopyInfo(ref lobbyDetailsCopyInfoOptions, out var outLobbyDetailsInfo);
            if (copyInfoResult != Success)
            {
                Debug.LogErrorFormat("Lobbies (InitFromLobbyDetails): can't copy lobby info. Error code: {0}", copyInfoResult);
                return;
            }
            if (outLobbyDetailsInfo == null || !outLobbyDetailsInfo.HasValue)
            {
                Debug.LogError("Lobbies: (InitFromLobbyDetails) could not copy info: outLobbyDetailsInfo is null.");
                return;
            }

            this.SetValuesFromLobbyDetailsInfo(outLobbyDetailsInfo.Value);

            /// Get lobby attributes
            _attributesDict.Clear();
            var lobbyDetailsGetAttributeCountOptions = new LobbyDetailsGetAttributeCountOptions();
            uint attrCount = lobbyDetails.GetAttributeCount(ref lobbyDetailsGetAttributeCountOptions);
            for (uint i = 0; i < attrCount; i++)
            {
                var attrOptions = new LobbyDetailsCopyAttributeByIndexOptions()
                {
                    AttrIndex = i
                };

                var copyAttrResult = lobbyDetails.CopyAttributeByIndex(ref attrOptions, out var outAttribute);
                if (copyAttrResult == Success && outAttribute != null && outAttribute.HasValue)
                {
                    AddAttribute(new LobbyAttribute(outAttribute.Value));
                }
            }

            /// Store old members and get new members
            var oldMembers = new List<LobbyMember>(members);
            members.Clear();

            var lobbyDetailsGetMemberCountOptions = new LobbyDetailsGetMemberCountOptions();
            uint memberCount = lobbyDetails.GetMemberCount(ref lobbyDetailsGetMemberCountOptions);

            for (int i = 0; i < memberCount; i++)
            {
                var getMemberByIndexOptions = new LobbyDetailsGetMemberByIndexOptions()
                {
                    MemberIndex = (uint) i
                };

                var memberProductId = lobbyDetails.GetMemberByIndex(ref getMemberByIndexOptions);
                if (memberProductId == null || !memberProductId.IsValid()) continue;
                
                var newLobbyMember = new LobbyMember(memberProductId);
                members.Insert(i, newLobbyMember);

                /// Member attributes
                var lobbyDetailsGetMemberAttributeCountOptions = new LobbyDetailsGetMemberAttributeCountOptions()
                {
                    TargetUserId = memberProductId
                };
                int memberAttributeCount = (int) lobbyDetails.GetMemberAttributeCount(ref lobbyDetailsGetMemberAttributeCountOptions);

                for (int attributeIndex = 0; attributeIndex < memberAttributeCount; attributeIndex++)
                {
                    var lobbyDetailsCopyMemberAttributeByIndexOptions = new LobbyDetailsCopyMemberAttributeByIndexOptions()
                    {
                        AttrIndex = (uint) attributeIndex,
                        TargetUserId = memberProductId
                    };

                    var memberAttributeResult = lobbyDetails.CopyMemberAttributeByIndex(ref lobbyDetailsCopyMemberAttributeByIndexOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);
                    if (memberAttributeResult != Success || !outAttribute.HasValue)
                    {
                        Debug.LogFormat("Lobbies (InitFromLobbyDetails): can't copy member attribute. Error code: {0}", memberAttributeResult);
                        continue;
                    }

                    members[i].AddAttribute(new LobbyAttribute(outAttribute.Value));
                }

                /// Copy RTC Status from old members
                foreach (LobbyMember oldLobbyMember in oldMembers)
                {
                    LobbyMember newMember = members[i];
                    if (oldLobbyMember.ProductUserId != newMember.ProductUserId)
                    {
                        continue;
                    }

                    /// Copy RTC status to new object
                    newMember.RTCState = oldLobbyMember.RTCState;
                    break;
                }
            }
        }

        public void SetWorldAttributes(WorldAttributes attributes)
        {
            MaxNumLobbyMembers = (uint) attributes.MaxPlayers;
        }

        void SetValuesFromLobbyDetailsInfo(LobbyDetailsInfo info)
        {
            Id = info.LobbyId;
            MaxNumLobbyMembers = (ushort) info.MaxMembers;
            LobbyPermissionLevel = info.PermissionLevel;
            AllowInvites = info.AllowInvites;
            AvailableSlots = (ushort) info.AvailableSlots;
            BucketId = info.BucketId;
            RTCRoomEnabled = info.RTCRoomEnabled;
        }
    }
}