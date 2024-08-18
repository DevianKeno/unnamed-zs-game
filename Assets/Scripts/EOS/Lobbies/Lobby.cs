using System;
using System.Collections.Generic;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;

using UZSG.Systems;

namespace UZSG.EOS.Lobbies
{
    /// <summary>
    /// Class represents all Lobby properties
    /// </summary>
    public class Lobby
    {
        public string Id;
        public ProductUserId LobbyOwner = new ProductUserId();
        public EpicAccountId LobbyOwnerAccountId = new EpicAccountId();
        public string LobbyOwnerDisplayName;
        public string BucketId;
        public uint MaxNumLobbyMembers = 0;
        public LobbyPermissionLevel LobbyPermissionLevel = LobbyPermissionLevel.Publicadvertised;
        public uint AvailableSlots = 0;
        public bool AllowInvites = true;
        public bool? DisableHostMigration;

        /// Cached copy of the RoomName of the RTC room that our lobby has, if any
        public string RTCRoomName = string.Empty;
        /// Are we currently connected to an RTC room?
        public bool RTCRoomConnected = false;
        /** Notification for RTC connection status changes */
        public NotifyEventHandle RTCRoomConnectionChanged; /// EOS_INVALID_NOTIFICATIONID;
        /** Notification for RTC room participant updates (new players or players leaving) */
        public NotifyEventHandle RTCRoomParticipantUpdate; /// EOS_INVALID_NOTIFICATIONID;
        /** Notification for RTC audio updates (talking status or mute changes) */
        public NotifyEventHandle RTCRoomParticipantAudioUpdate; /// EOS_INVALID_NOTIFICATIONID;

        public bool PresenceEnabled = false;
        public bool RTCRoomEnabled = false;

        public List<LobbyAttribute> Attributes = new List<LobbyAttribute>();
        public List<LobbyMember> Members = new List<LobbyMember>();

        /// Utility data

        public bool _SearchResult = false;
        public bool _BeingCreated = false;

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
            attribute = Attributes.Find((LobbyAttribute attr) =>
            {
                return attr.Key == KEY;
            });
            return attribute != null;
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
            Attributes.Clear();
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

            InitFromLobbyDetails(outLobbyDetailsHandle);
        }

        /// <summary>
        /// Initializing the given <c>LobbyDetails</c> handle and caches all relevant attributes
        /// </summary>
        /// <param name="lobbyId">Specified <c>LobbyDetails</c> handle</param>
        public void InitFromLobbyDetails(LobbyDetails outLobbyDetailsHandle)
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
            MaxNumLobbyMembers = (uint) (outLobbyDetailsInfo?.MaxMembers);
            LobbyPermissionLevel = (LobbyPermissionLevel) (outLobbyDetailsInfo?.PermissionLevel);
            AllowInvites = (bool) (outLobbyDetailsInfo?.AllowInvites);
            AvailableSlots = (uint) (outLobbyDetailsInfo?.AvailableSlots);
            BucketId = outLobbyDetailsInfo?.BucketId;
            RTCRoomEnabled = (bool) (outLobbyDetailsInfo?.RTCRoomEnabled);

            /// get attributes
            Attributes.Clear();
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
                    Attributes.Add(attr);
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
    }
}