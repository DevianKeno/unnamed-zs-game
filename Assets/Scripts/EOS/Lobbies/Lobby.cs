using System;
using System.Collections.Generic;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;

using UZSG.Systems;
using UZSG.Worlds;
using System.Linq;

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
        /// This criteria should be set with mostly static, coarse settings,
        /// often formatted like <c>GameMode:Region:MapName</c>.
        /// </summary>
        public string BucketId { get; internal set; }
        public ProductUserId LobbyOwner { get; internal set; }
        public EpicAccountId LobbyOwnerAccountId { get; internal set; }
        public LobbyPermissionLevel LobbyPermissionLevel { get; internal set; } = LobbyPermissionLevel.Publicadvertised;
        public ushort MaxNumLobbyMembers { get; internal set; } = 0;
        public ushort AvailableSlots { get; internal set; } = 0;
        public bool AllowInvites { get; internal set; } = true;
        public bool? DisableHostMigration{ get; internal set; }
        public string LobbyOwnerDisplayName { get; internal set; }

        /// <summary>
        /// Cached copy of the RoomName of the RTC room that our lobby has, if any
        /// </summary>
        public string RTCRoomName { get; internal set; } = string.Empty;
        /// <summary>
        /// Are we currently connected to an RTC room?
        /// </summary>
        public bool RTCRoomConnected { get; internal set; } = false;
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

        public List<LobbyAttribute> Attributes => _attributesDict.Values.ToList();
        /// <summary>
        /// <c>string</c> is AttributeKey.
        /// </summary>
        Dictionary<string, LobbyAttribute> _attributesDict = new();
        public List<LobbyMember> Members = new();

        /// Utility data
        public bool _isSearchResult = false;
        public bool _isBeingCreated = false;

        public void AddAttribute(LobbyAttribute attribute)
        {
            _attributesDict[attribute.Key] = attribute;
            this.Attributes.Add(attribute);
        }

        /// <summary>
        /// Checks if Lobby Id is valid
        /// </summary>
        /// <returns>True if valid</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Id);
        }

        public bool TryGetAttribute(string KEY, out LobbyAttribute attribute)
        {
            return _attributesDict.TryGetValue(KEY, out attribute);
        }

        public bool FindLobbyMember(ProductUserId memberId, out LobbyMember lobbyMember)
        {
            lobbyMember = Members.Find((LobbyMember member) =>
            {
                return member.ProductId == memberId;
            });
            return lobbyMember != null;
        }

        /// <summary>
        /// Checks if the specified <c>ProductUserId</c> is the current owner
        /// </summary>
        /// <param name="userProductId">Specified <c>ProductUserId</c></param>
        /// <returns>True if specified user is owner</returns>
        public bool IsOwner(ProductUserId userProductId)
        {
            return userProductId == LobbyOwner;
        }

        /// <summary>
        /// Clears local cache of Lobby Id, owner, attributes and members
        /// </summary>
        public void Clear()
        {
            Id = string.Empty;
            LobbyOwner = new ProductUserId();
            _attributesDict.Clear();
            Members.Clear();
        }

        /// <summary>
        /// Initializing the given Lobby Id and caches all relevant attributes
        /// </summary>
        /// <param name="lobbyId">Specified Lobby Id</param>
        public void InitFromLobbyHandle(string lobbyId)
        {
            if (string.IsNullOrEmpty(lobbyId))
            {
                return;
            }

            Id = lobbyId;

            var options = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = Id,
                LocalUserId = Game.EOS.GetProductUserId()
            };

            Result result = Game.EOS.GetEOSLobbyInterface().CopyLobbyDetailsHandle(ref options, out LobbyDetails outLobbyDetailsHandle);
            if (result != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (InitFromLobbyHandle): can't get lobby info handle. Error code: {0}", result);
                return;
            }
            if (outLobbyDetailsHandle == null)
            {
                Debug.LogError("Lobbies (InitFromLobbyHandle): can't get lobby info handle. outLobbyDetailsHandle is null");
                return;
            }

            InitializeFromLobbyDetails(outLobbyDetailsHandle);
        }

        /// <summary>
        /// Initializing the given <c>LobbyDetails</c> handle and caches all relevant attributes
        /// </summary>
        /// <param name="lobbyId">Specified <c>LobbyDetails</c> handle</param>
        public void InitializeFromLobbyDetails(LobbyDetails outLobbyDetailsHandle)
        {
            /// get owner
            var lobbyDetailsGetLobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId newLobbyOwner = outLobbyDetailsHandle.GetLobbyOwner(ref lobbyDetailsGetLobbyOwnerOptions);
            if (newLobbyOwner != LobbyOwner)
            {
                LobbyOwner = newLobbyOwner;
                LobbyOwnerAccountId = new EpicAccountId();
                LobbyOwnerDisplayName = string.Empty;
            }

            /// Copy lobby info
            var lobbyDetailsCopyInfoOptions = new LobbyDetailsCopyInfoOptions();
            Result infoResult = outLobbyDetailsHandle.CopyInfo(ref lobbyDetailsCopyInfoOptions, out LobbyDetailsInfo? outLobbyDetailsInfo);
            if (infoResult != Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (InitFromLobbyDetails): can't copy lobby info. Error code: {0}", infoResult);
                return;
            }
            if (outLobbyDetailsInfo == null)
            {
                Debug.LogError("Lobbies: (InitFromLobbyDetails) could not copy info: outLobbyDetailsInfo is null.");
                return;
            }

            Id = outLobbyDetailsInfo?.LobbyId;
            MaxNumLobbyMembers = (ushort) (outLobbyDetailsInfo?.MaxMembers);
            LobbyPermissionLevel = (LobbyPermissionLevel) (outLobbyDetailsInfo?.PermissionLevel);
            AllowInvites = (bool) (outLobbyDetailsInfo?.AllowInvites);
            AvailableSlots = (ushort) (outLobbyDetailsInfo?.AvailableSlots);
            BucketId = outLobbyDetailsInfo?.BucketId;
            RTCRoomEnabled = (bool) (outLobbyDetailsInfo?.RTCRoomEnabled);

            /// get attributes
            _attributesDict.Clear();
            var lobbyDetailsGetAttributeCountOptions = new LobbyDetailsGetAttributeCountOptions();
            uint attrCount = outLobbyDetailsHandle.GetAttributeCount(ref lobbyDetailsGetAttributeCountOptions);
            for (uint i = 0; i < attrCount; i++)
            {
                var attrOptions = new LobbyDetailsCopyAttributeByIndexOptions()
                {
                    AttrIndex = i
                };
                
                Result copyAttrResult = outLobbyDetailsHandle.CopyAttributeByIndex(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);
                if (copyAttrResult == Result.Success && outAttribute != null && outAttribute?.Data != null)
                {
                    LobbyAttribute attr = new();
                    attr.InitFromAttribute(outAttribute);
                    AddAttribute(attr);
                }
            }

            /// Get old members
            var OldMembers = new List<LobbyMember>(Members);
            Members.Clear();

            var lobbyDetailsGetMemberCountOptions = new LobbyDetailsGetMemberCountOptions();
            uint memberCount = outLobbyDetailsHandle.GetMemberCount(ref lobbyDetailsGetMemberCountOptions);

            for (int memberIndex = 0; memberIndex < memberCount; memberIndex++)
            {
                var lobbyDetailsGetMemberByIndexOptions = new LobbyDetailsGetMemberByIndexOptions()
                {
                    MemberIndex = (uint) memberIndex
                };

                ProductUserId memberId = outLobbyDetailsHandle.GetMemberByIndex(ref lobbyDetailsGetMemberByIndexOptions);
                var newLobbyMember = new LobbyMember()
                {
                    ProductId = memberId
                };

                Members.Insert(memberIndex, newLobbyMember);

                /// Member attributes
                var lobbyDetailsGetMemberAttributeCountOptions = new LobbyDetailsGetMemberAttributeCountOptions()
                {
                    TargetUserId = memberId
                };
                int memberAttributeCount = (int) outLobbyDetailsHandle.GetMemberAttributeCount(ref lobbyDetailsGetMemberAttributeCountOptions);

                for (int attributeIndex = 0; attributeIndex < memberAttributeCount; attributeIndex++)
                {
                    var lobbyDetailsCopyMemberAttributeByIndexOptions = new LobbyDetailsCopyMemberAttributeByIndexOptions()
                    {
                        AttrIndex = (uint) attributeIndex,
                        TargetUserId = memberId
                    };

                    Result memberAttributeResult = outLobbyDetailsHandle.CopyMemberAttributeByIndex(ref lobbyDetailsCopyMemberAttributeByIndexOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);
                    if (memberAttributeResult != Result.Success)
                    {
                        Debug.LogFormat("Lobbies (InitFromLobbyDetails): can't copy member attribute. Error code: {0}", memberAttributeResult);
                        continue;
                    }

                    LobbyAttribute newAttribute = new();
                    newAttribute.InitFromAttribute(outAttribute);
                    Members[memberIndex].MemberAttributes.Add(newAttribute.Key, newAttribute);
                }

                /// Copy RTC Status from old members
                foreach (LobbyMember oldLobbyMember in OldMembers)
                {
                    LobbyMember newMember = Members[memberIndex];
                    if (oldLobbyMember.ProductId != newMember.ProductId)
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
            MaxNumLobbyMembers = (ushort) attributes.MaxPlayers;
        }

        #region World Data transfer


        #endregion
    }
}