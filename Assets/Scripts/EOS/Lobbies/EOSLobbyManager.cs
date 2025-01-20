using System;
using System.Collections.Generic;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;
using PlayEveryWare.EpicOnlineServices;

using UZSG.EOS.Lobbies;
using UZSG.Systems;

namespace UZSG.EOS
{
    /// <summary>
    /// Class <c>EOSLobbyManager</c> is a simplified wrapper for EOS [Lobby Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/Lobby/index.html).
    /// </summary>
    public class EOSLobbyManager : IEOSSubManager, IAuthInterfaceEventListener, IConnectInterfaceEventListener
    {
        LobbyInterface lobbyInterface;

        Lobby currentLobby;
        /// <summary>
        /// Currently active lobby
        /// </summary>
        public Lobby CurrentLobby => currentLobby;
        LobbyJoinRequest currentJoinRequest;
        /// <summary>
        /// Currently active join request
        /// </summary>
        public LobbyJoinRequest CurrentJoinRequest => currentJoinRequest;
        Dictionary<ProductUserId, LobbyInvite> pendingInvites = new();
        /// <summary>
        /// Pending invites (up to one invite per friend)
        /// </summary>
        public Dictionary<ProductUserId, LobbyInvite> PendingInvites => pendingInvites;
        LobbyInvite currentInvite;
        /// <summary>
        /// Currently active invite
        /// </summary>
        public LobbyInvite CurrentInvite => currentInvite;
        /// Search 
        LobbySearch currentSearch;
        public LobbySearch CurrentSearch => currentSearch;
        Dictionary<Lobby, LobbyDetails> searchResults = new();
        public Dictionary<Lobby, LobbyDetails> SearchResults => searchResults;

        public LocalRTCOptions? customLocalRTCOptions;
        public bool IsHosting { get; private set; }
        public bool IsInLobby { get; private set; }

        /// NotificationId
        NotifyEventHandle lobbyUpdateNotification;
        NotifyEventHandle lobbyMemberUpdateNotification;
        NotifyEventHandle lobbyMemberStatusNotification;
        NotifyEventHandle lobbyInviteNotification;
        NotifyEventHandle lobbyInviteAcceptedNotification;
        NotifyEventHandle joinLobbyAcceptedNotification;
        NotifyEventHandle leaveLobbyRequestedNotification;

        // TODO: Does this constant exist in the EOS SDK C# Wrapper?
        const ulong EOS_INVALID_NOTIFICATIONID = 0;

        public bool _isDirty = true;

        /// Manager callbacks
        OnSearchByLobbyIdCallback _lobbySearchCallback;
        public delegate void OnCreateLobbyCallback(Epic.OnlineServices.Result result);
        public delegate void OnSearchByLobbyIdCallback(Epic.OnlineServices.Result result);
        public delegate void OnMemberUpdateCallback(string lobbyId, ProductUserId memberId);
        public delegate void OnMemberStatusCallback(string lobbyId, ProductUserId memberId, LobbyMemberStatus status);
        List<OnMemberUpdateCallback> _memberUpdatedCallbacks = new();
        List<OnMemberStatusCallback> _memberStatusCallbacks = new();
        List<Action> _lobbyChangedCallbacks = new();
        List<Action> _lobbyUpdatedCallbacks = new();

        public EOSLobbyManager()
        {
            Game.EOS.AddAuthLoginListener(this);
            Game.EOS.AddAuthLogoutListener(this);
            Game.EOS.AddConnectLoginListener(this);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
            lobbyInterface = Game.EOS.GetEOSLobbyInterface();
        }

#if UNITY_EDITOR
        ~EOSLobbyManager()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }
#endif

#if UNITY_EDITOR
        void OnPlayModeChanged(UnityEditor.PlayModeStateChange modeChange)
        {
            if (modeChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                /// Prevent attempts to call native EOS code while exiting play mode, which crashes the editor
                UnsubscribeFromLobbyUpdates();
                UnsubscribeFromLobbyInvites();
                lobbyInterface = null;
            }
        }
#endif

        bool IsLobbyNotificationValid(NotifyEventHandle handle)
        {
            return handle != null && handle.IsValid();
        }

        // Helper method to keep the code cleaner
        ulong AddNotifyLobbyUpdateReceived(LobbyInterface lobbyInterface, OnLobbyUpdateReceivedCallback notificationFn)
        {
            var options = new AddNotifyLobbyUpdateReceivedOptions();
            return lobbyInterface.AddNotifyLobbyUpdateReceived(ref options, null, notificationFn);
        }

        ulong AddNotifyLobbyMemberUpdateReceived(LobbyInterface lobbyInterface, OnLobbyMemberUpdateReceivedCallback notificationFn)
        {
            var options = new AddNotifyLobbyMemberUpdateReceivedOptions();
            return lobbyInterface.AddNotifyLobbyMemberUpdateReceived(ref options, null, notificationFn);
        }

        ulong AddNotifyLobbyMemberStatusReceived(LobbyInterface lobbyInterface, OnLobbyMemberStatusReceivedCallback notificationFn)
        {
            var options = new AddNotifyLobbyMemberStatusReceivedOptions();
            return lobbyInterface.AddNotifyLobbyMemberStatusReceived(ref options, null, notificationFn);
        }

        ulong AddNotifyLeaveLobbyRequested(LobbyInterface lobbyInterface, OnLeaveLobbyRequestedCallback notificationFn)
        {
            var options = new AddNotifyLeaveLobbyRequestedOptions();
            return lobbyInterface.AddNotifyLeaveLobbyRequested(ref options, null, notificationFn);
        }

        void SubscribeToLobbyUpdates()
        {
            if (IsLobbyNotificationValid(lobbyUpdateNotification) ||
                IsLobbyNotificationValid(lobbyMemberUpdateNotification) || 
                IsLobbyNotificationValid(lobbyMemberStatusNotification) ||
                IsLobbyNotificationValid(leaveLobbyRequestedNotification))
            {
                Debug.LogError("Lobbies (SubscribeToLobbyUpdates): SubscribeToLobbyUpdates called but already subscribed!");
                return;
            }
            
            lobbyUpdateNotification = new NotifyEventHandle(AddNotifyLobbyUpdateReceived(lobbyInterface, OnLobbyUpdateReceived), (ulong handle) =>
            {
                lobbyInterface.RemoveNotifyLobbyUpdateReceived(handle);
            });

            lobbyMemberUpdateNotification = new NotifyEventHandle(AddNotifyLobbyMemberUpdateReceived(lobbyInterface, OnMemberUpdateReceived), (ulong handle) => 
            {
                lobbyInterface.RemoveNotifyLobbyMemberUpdateReceived(handle);
            });

            lobbyMemberStatusNotification = new NotifyEventHandle(AddNotifyLobbyMemberStatusReceived(lobbyInterface, OnMemberStatusReceived), (ulong handle) =>
            {
                lobbyInterface.RemoveNotifyLobbyMemberStatusReceived(handle);
            });

            leaveLobbyRequestedNotification = new NotifyEventHandle(AddNotifyLeaveLobbyRequested(lobbyInterface, OnLeaveLobbyRequested), (ulong handle) =>
            {
                lobbyInterface.RemoveNotifyLeaveLobbyRequested(handle);
            });
        }

        void UnsubscribeFromLobbyUpdates()
        {
            lobbyUpdateNotification?.Dispose();
            lobbyMemberUpdateNotification?.Dispose();
            lobbyMemberStatusNotification?.Dispose();
            leaveLobbyRequestedNotification?.Dispose();
        }

        //-------------------------------------------------------------------------
        void SubscribeToLobbyInvites()
        {
            if (IsLobbyNotificationValid(lobbyInviteNotification) || 
                IsLobbyNotificationValid(lobbyInviteAcceptedNotification) || 
                IsLobbyNotificationValid(joinLobbyAcceptedNotification) )
            {
                Debug.LogError("Lobbies (SubscribeToLobbyInvites): SubscribeToLobbyInvites called but already subscribed!");
                return;
            }

            var addNotifyLobbyInviteReceivedOptions = new AddNotifyLobbyInviteReceivedOptions();
            lobbyInviteNotification = new NotifyEventHandle(lobbyInterface.AddNotifyLobbyInviteReceived(ref addNotifyLobbyInviteReceivedOptions, null, OnLobbyInviteReceived), (ulong handle) =>
            {
                lobbyInterface.RemoveNotifyLobbyInviteReceived(handle);
            });

            var addNotifyLobbyInviteAcceptedOptions = new AddNotifyLobbyInviteAcceptedOptions();
            lobbyInviteAcceptedNotification = new NotifyEventHandle(lobbyInterface.AddNotifyLobbyInviteAccepted(ref addNotifyLobbyInviteAcceptedOptions, null, OnLobbyInviteAccepted), (ulong handle) =>
            {
                lobbyInterface.RemoveNotifyLobbyInviteAccepted(handle);
            });

            var addNotifyJoinLobbyAcceptedOptions = new AddNotifyJoinLobbyAcceptedOptions();
            joinLobbyAcceptedNotification = new NotifyEventHandle(lobbyInterface.AddNotifyJoinLobbyAccepted(ref addNotifyJoinLobbyAcceptedOptions, null, OnJoinLobbyAccepted), (ulong handle) =>
            {
                lobbyInterface.RemoveNotifyJoinLobbyAccepted(handle);
            });
        }

        //-------------------------------------------------------------------------
        void UnsubscribeFromLobbyInvites()
        {
            lobbyInviteNotification?.Dispose(); 
            lobbyInviteAcceptedNotification?.Dispose(); 
            joinLobbyAcceptedNotification?.Dispose();
        }

        string GetRTCRoomName()
        {
            GetRTCRoomNameOptions options = new()
            {
                LobbyId = currentLobby.Id,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            Epic.OnlineServices.Result result = lobbyInterface.GetRTCRoomName(ref options, out Utf8String roomName);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogFormat("Lobbies (GetRTCRoomName): Could not get RTC Room Name. Error Code: {0}", result);
                return string.Empty;
            }

            Debug.LogFormat("Lobbies (GetRTCRoomName): Found RTC Room Name for lobby. RooName={0}", roomName);

            return roomName;
        }

        void UnsubscribeFromRTCEvents()
        {
            if (!currentLobby.RTCRoomEnabled)
            {
                return;
            }

            currentLobby.RTCRoomParticipantAudioUpdate.Dispose();
            currentLobby.RTCRoomParticipantUpdate.Dispose();
            currentLobby.RTCRoomConnectionChanged.Dispose();

            currentLobby.RTCRoomName = string.Empty;
        }

        void SubscribeToRTCEvents()
        {
            if (!currentLobby.RTCRoomEnabled)
            {
                Debug.LogWarning("Lobbies (SubscribeToRTCEvents): RTC Room is disabled.");
                return;
            }

            currentLobby.RTCRoomName = GetRTCRoomName();

            if (string.IsNullOrEmpty(currentLobby.RTCRoomName))
            {
                Debug.LogError("Lobbies (SubscribeToRTCEvents): Unable to bind to RTC Room Name, failing to bind delegates.");
                return;
            }

            // Register for connection status changes
            AddNotifyRTCRoomConnectionChangedOptions addNotifyRTCRoomConnectionChangedOptions = new();
            currentLobby.RTCRoomConnectionChanged = new NotifyEventHandle(lobbyInterface.AddNotifyRTCRoomConnectionChanged(ref addNotifyRTCRoomConnectionChangedOptions, null, OnRTCRoomConnectionChangedReceived), (ulong handle) =>
            {
                lobbyInterface.RemoveNotifyRTCRoomConnectionChanged(handle);
            });

            if (!currentLobby.RTCRoomConnectionChanged.IsValid())
            {
                Debug.LogError("Lobbies (SubscribeToRTCEvents): Failed to bind to Lobby NotifyRTCRoomConnectionChanged notification.");
                return;
            }

            // Get the current room connection status now that we're listening for changes
            IsRTCRoomConnectedOptions isRTCRoomConnectedOptions = new()
            {
                LobbyId = currentLobby.Id,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            Epic.OnlineServices.Result result = lobbyInterface.IsRTCRoomConnected(ref isRTCRoomConnectedOptions, out bool isConnected);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogFormat("Lobbies (SubscribeToRTCEvents): Failed to get RTC Room connection status:. Error Code: {0}", result);
                return;
            }
            else
            {
                currentLobby.RTCRoomConnected = isConnected;
            }

            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();

            // Register for RTC Room participant changes
            AddNotifyParticipantStatusChangedOptions addNotifyParticipantsStatusChangedOptions = new AddNotifyParticipantStatusChangedOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = currentLobby.RTCRoomName
            };

            currentLobby.RTCRoomParticipantUpdate = new NotifyEventHandle(rtcHandle.AddNotifyParticipantStatusChanged(ref addNotifyParticipantsStatusChangedOptions, null, OnRTCRoomParticipantStatusChanged), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSRTCInterface().RemoveNotifyParticipantStatusChanged(handle);
            });

            if (!currentLobby.RTCRoomParticipantUpdate.IsValid())
            {
                Debug.LogError("Lobbies (SubscribeToRTCEvents): Failed to bind to RTC AddNotifyParticipantStatusChanged notification.");
            }

            // Register for talking changes
            AddNotifyParticipantUpdatedOptions addNotifyParticipantUpdatedOptions = new AddNotifyParticipantUpdatedOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = currentLobby.RTCRoomName
            };

            currentLobby.RTCRoomParticipantAudioUpdate = new NotifyEventHandle(rtcAudioHandle.AddNotifyParticipantUpdated(ref addNotifyParticipantUpdatedOptions, null, OnRTCRoomParticipantAudioUpdateRecieved), (ulong handle) =>
            {
                EOSManager.Instance.GetEOSRTCInterface().GetAudioInterface().RemoveNotifyParticipantUpdated(handle);
            });

            // Allow Manual Gain Control
            var setSetting = new SetSettingOptions();
            setSetting.SettingName = "DisableAutoGainControl";
            setSetting.SettingValue = "True";
            var disableAutoGainControlResult = rtcHandle.SetSetting(ref setSetting);
        }

        void OnRTCRoomConnectionChangedReceived(ref RTCRoomConnectionChangedCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnRTCRoomConnectionChangedReceived): RTCRoomConnectionChangedCallbackInfo data is null");
            //    return;
            //}

            Debug.LogFormat("Lobbies (OnRTCRoomConnectionChangedReceived): connection status changed. LobbyId={0}, IsConnected={1}, DisconnectReason={2}", data.LobbyId, data.IsConnected, data.DisconnectReason);

            // OnRTCRoomConnectionChanged

            if (currentLobby != null && !currentLobby.IsValid() || currentLobby.Id != data.LobbyId)
            {
                return;
            }

            if (EOSManager.Instance.GetProductUserId() != data.LocalUserId)
            {
                return;
            }

            currentLobby.RTCRoomConnected = data.IsConnected;

            foreach(LobbyMember lobbyMember in currentLobby.Members)
            {
                if (lobbyMember.ProductId == EOSManager.Instance.GetProductUserId())
                {
                    lobbyMember.RTCState.IsInRTCRoom = data.IsConnected;
                    if (!data.IsConnected)
                    {
                        lobbyMember.RTCState.IsTalking = false;
                    }
                    break;
                }
            }

            _isDirty = true;
        }

        void OnRTCRoomParticipantStatusChanged(ref ParticipantStatusChangedCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnRTCRoomParticipantStatusChanged): ParticipantStatusChangedCallbackInfo data is null");
            //    return;
            //}

            int metadataCount = 0;
            if (data.ParticipantMetadata != null)
            {
                metadataCount = data.ParticipantMetadata.Length;
            }

            Debug.LogFormat("Lobbies (OnRTCRoomParticipantStatusChanged): LocalUserId={0}, Room={1}, ParticipantUserId={2}, ParticipantStatus={3}, MetadataCount={4}",
                data.LocalUserId, 
                data.RoomName, 
                data.ParticipantId, 
                data.ParticipantStatus == RTCParticipantStatus.Joined ? "Joined" : "Left",
                metadataCount);

            // Ensure this update is for our room
            if (string.IsNullOrEmpty(currentLobby.RTCRoomName) || !currentLobby.RTCRoomName.Equals(data.RoomName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            //OnRTCRoomParticipantJoined / OnRTCRoomParticipantLeft

            // Find this participant in our list
            foreach (LobbyMember lobbyMember in currentLobby.Members)
            {
                if (lobbyMember.ProductId != data.ParticipantId)
                {
                    continue;
                }

                // Update in-room status
                if (data.ParticipantStatus == RTCParticipantStatus.Joined)
                {
                    lobbyMember.RTCState.IsInRTCRoom = true;
                }
                else
                {
                    lobbyMember.RTCState.IsInRTCRoom = false;
                    lobbyMember.RTCState.IsTalking = false;
                }

                _isDirty = true;
                break;
            }
        }

        void OnRTCRoomParticipantAudioUpdateRecieved(ref ParticipantUpdatedCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnRTCRoomParticipantAudioUpdateRecieved): ParticipantUpdatedCallbackInfo data is null");
            //    return;
            //}

            /* Verbose Logging: Uncomment to print each time audio is received.
            Debug.LogFormat("Lobbies (OnRTCRoomParticipantAudioUpdateRecieved): participant audio updated. LocalUserId={0}, Room={1}, ParticipantUserId={2}, IsTalking={3}, IsAudioDisabled={4}",
                data.LocalUserId,
                data.RoomName,
                data.ParticipantId,
                data.Speaking,
                data.AudioStatus != RTCAudioStatus.Enabled);
            */

            // OnRTCRoomParticipantAudioUpdated

            // Ensure this update is for our room
            if (string.IsNullOrEmpty(currentLobby.RTCRoomName) || !currentLobby.RTCRoomName.Equals(data.RoomName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Find this participant in our list
            foreach(LobbyMember lobbyMember in currentLobby.Members)
            {
                if (lobbyMember.ProductId != data.ParticipantId)
                {
                    continue;
                }

                // Update talking status
                if (lobbyMember.RTCState.IsTalking != data.Speaking)
                {
                    lobbyMember.RTCState.IsTalking = data.Speaking;
                }

                // Only update the audio status for other players (we control their own status)
                if (lobbyMember.ProductId != EOSManager.Instance.GetProductUserId())
                {
                    lobbyMember.RTCState.IsAudioOutputDisabled = data.AudioStatus != RTCAudioStatus.Enabled;
                }

                _isDirty = true;
                break;
            }
        }

        public void OnAuthLogin(Epic.OnlineServices.Auth.LoginCallbackInfo info)
        {
            /// NOTE: Idk really know which is used when logging if its either Auth or Connect
            /// but I'm team connect for now
            
            // if (info.ResultCode == Result.Success)
            // {
            //     InitializeLoggedIn();
            // }
        }

        public void OnAuthLogout(Epic.OnlineServices.Auth.LogoutCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                DeinitializeLoggedOut();
            }
        }

        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                InitializeLoggedIn();
            }
        }

        void InitializeLoggedIn()
        {
            _isDirty = true;
            currentInvite = null;
            currentLobby = new Lobby();

            SubscribeToLobbyUpdates();
            SubscribeToLobbyInvites();

            _lobbySearchCallback = null;

            EOSManagerPlatformSpecificsSingleton.Instance.SetDefaultAudioSession();
        }

        /// <summary>User Logged Out actions</summary>
        /// <list type="bullet">
        ///     <item><description>Leaves current lobby</description></item>
        ///     <item><description>Unsubscribe from Lobby invites and updates</description></item>
        ///     <item><description>Reset local cache for <c>Lobby</c>, <c>LobbyJoinRequest</c>, Invites, <c>LobbySearch</c> and </description></item>
        /// </list>
        public void DeinitializeLoggedOut()
        {
            LeaveCurrentLobby(null);
            UnsubscribeFromLobbyInvites();
            UnsubscribeFromLobbyUpdates();

            currentLobby = new Lobby();
            currentJoinRequest = new LobbyJoinRequest();

            pendingInvites.Clear();
            currentInvite = null;

            currentSearch = new LobbySearch();
            searchResults.Clear();
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_CreateLobby](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_CreateLobby/index.html)
        /// </summary>
        /// <param name="lobby"><b>Lobby</b> properties used to create new lobby</param>
        /// <param name="onCompleted">Callback when create lobby is completed</param>
        public void CreateLobby(Lobby lobby, OnCreateLobbyCallback onCompleted = null)
        {
            var localProductUserId = Game.EOS.GetProductUserId();
            if (localProductUserId == null || !localProductUserId.IsValid())
            {
                Debug.LogError("[LobbyManager/CreateLobby()]: Current user is invalid!");
                onCompleted?.Invoke(Epic.OnlineServices.Result.InvalidProductUserID);
                return;
            }

            /// Check if there is current session. Leave it.
            if (currentLobby != null && currentLobby.IsValid())
            {
                Debug.Log($"[LobbyManager/CreateLobby()]: Leaving Current Lobby '{currentLobby.Id}'");
                onCompleted?.Invoke(Epic.OnlineServices.Result.LobbyInvalidSession);
                LeaveCurrentLobby(null);
            }

            /// Create new lobby
            /// Max Players
            var createLobbyOptions = new CreateLobbyOptions()
            {
                LocalUserId = localProductUserId,
                MaxLobbyMembers = lobby.MaxNumLobbyMembers,
                PermissionLevel = lobby.LobbyPermissionLevel,
                PresenceEnabled = lobby.PresenceEnabled,
                AllowInvites = lobby.AllowInvites,
                BucketId = lobby.BucketId
            };
            if (lobby.DisableHostMigration != null)
            {
                createLobbyOptions.DisableHostMigration = (bool)lobby.DisableHostMigration;
            }
            /// Voice Chat
            if (lobby.RTCRoomEnabled)
            {
                if (customLocalRTCOptions != null)
                {
                    createLobbyOptions.LocalRTCOptions = customLocalRTCOptions;
                }
                createLobbyOptions.EnableRTCRoom = true;      
            }
            else
            {
                createLobbyOptions.EnableRTCRoom = false;
            }

            lobbyInterface.CreateLobby(ref createLobbyOptions, onCompleted, OnCreateLobbyCompleted);

            /// Save lobby data for modification
            currentLobby = lobby;
            currentLobby._isBeingCreated = true;
            currentLobby.LobbyOwner = localProductUserId;
            IsHosting = true;
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_UpdateLobby](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_UpdateLobby/index.html)
        /// </summary>
        /// <param name="lobbyUpdates"><b>Lobby</b> properties used to update current lobby</param>
        /// <param name="onModifyLobbyCompleted">Callback when modify lobby is completed</param>
        public void ModifyLobby(Lobby lobbyUpdates, OnCreateLobbyCallback onModifyLobbyCompleted)
        {
            // Validate current lobby
            if (!currentLobby.IsValid())
            {
                Debug.LogError("Lobbies (ModifyLobby): Current Lobby {0} is invalid!");
                onModifyLobbyCompleted?.Invoke(Epic.OnlineServices.Result.InvalidState);
                return;
            }

            ProductUserId currentProductUserId = EOSManager.Instance.GetProductUserId();
            if (!currentProductUserId.IsValid())
            {
                Debug.LogError("Lobbies (ModifyLobby): Current player is invalid!");
                onModifyLobbyCompleted?.Invoke(Epic.OnlineServices.Result.InvalidProductUserID);
                return;
            }

            if (!currentLobby.IsOwner(currentProductUserId))
            {
                Debug.LogError("Lobbies (ModifyLobby): Current player is not lobby owner!");
                onModifyLobbyCompleted?.Invoke(Epic.OnlineServices.Result.LobbyNotOwner);
                return;
            }

            UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions();
            options.LobbyId = currentLobby.Id;
            options.LocalUserId = currentProductUserId;

            // Get LobbyModification object handle
            Epic.OnlineServices.Result result = lobbyInterface.UpdateLobbyModification(ref options, out LobbyModification outLobbyModificationHandle);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not create lobby modification. Error code: {0}", result);
                onModifyLobbyCompleted?.Invoke(result);
                return;
            }

            // Bucket Id
            if (!string.Equals(lobbyUpdates.BucketId, currentLobby.BucketId))
            {
                var lobbyModificationSetBucketIdOptions = new LobbyModificationSetBucketIdOptions() { BucketId = lobbyUpdates.BucketId };
                result = outLobbyModificationHandle.SetBucketId(ref lobbyModificationSetBucketIdOptions);

                if (result != Epic.OnlineServices.Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not set bucket id. Error code: {0}", result);
                    onModifyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            // Max Players
            if (lobbyUpdates.MaxNumLobbyMembers > 0)
            {
                var lobbyModificationSetMaxMembersOptions = new LobbyModificationSetMaxMembersOptions() { MaxMembers = lobbyUpdates.MaxNumLobbyMembers };
                result = outLobbyModificationHandle.SetMaxMembers(ref lobbyModificationSetMaxMembersOptions);

                if (result != Epic.OnlineServices.Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not set max players. Error code: {0}", result);
                    onModifyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            // Add Lobby Attributes
            for (int attributeIndex = 0; attributeIndex < lobbyUpdates.Attributes.Count; attributeIndex++)
            {
                AttributeData attributeData = lobbyUpdates.Attributes[attributeIndex].AsAttribute;

                LobbyModificationAddAttributeOptions addAttributeOptions = new LobbyModificationAddAttributeOptions();
                addAttributeOptions.Attribute = attributeData;
                addAttributeOptions.Visibility = lobbyUpdates.Attributes[attributeIndex].Visibility;

                //if (attributeData.Key == null)
                //{
                //    Debug.LogWarning("Lobbies (ModifyLobby): Attributes with null key! Do not add!");
                //    continue;
                //}

                //if (attributeData.Value == null)
                //{
                //    Debug.LogWarningFormat("Lobbies (ModifyLobby): Attributes with key '{0}' has null value! Do not add!", attributeData.Key);
                //    continue;
                //}

                result = outLobbyModificationHandle.AddAttribute(ref addAttributeOptions);
                if (result != Epic.OnlineServices.Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not add attribute. Error code: {0}", result);
                    onModifyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            // Permission
            if (lobbyUpdates.LobbyPermissionLevel != currentLobby.LobbyPermissionLevel)
            {
                var lobbyModificationSetPermissionLevelOptions = new LobbyModificationSetPermissionLevelOptions() { PermissionLevel = lobbyUpdates.LobbyPermissionLevel };
                result = outLobbyModificationHandle.SetPermissionLevel(ref lobbyModificationSetPermissionLevelOptions);

                if (result != Epic.OnlineServices.Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not set permission level. Error code: {0}", result);
                    onModifyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            // Allow Invites
            if (lobbyUpdates.AllowInvites != currentLobby.AllowInvites)
            {
                var lobbyModificationSetInvitesAllowedOptions = new LobbyModificationSetInvitesAllowedOptions() { InvitesAllowed = lobbyUpdates.AllowInvites };
                result = outLobbyModificationHandle.SetInvitesAllowed(ref lobbyModificationSetInvitesAllowedOptions);

                if (result != Epic.OnlineServices.Result.Success)
                {
                    Debug.LogErrorFormat("Lobbies (ModifyLobby): Could not set allow invites. Error code: {0}", result);
                    onModifyLobbyCompleted?.Invoke(result);
                    return;
                }
            }

            //Trigger lobby update
            var updateLobbyOptions = new UpdateLobbyOptions()
            {
                LobbyModificationHandle = outLobbyModificationHandle
            };
            lobbyInterface.UpdateLobby(ref updateLobbyOptions, onModifyLobbyCompleted, OnUpdateLobbyCallBack);
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_LeaveLobby](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_LeaveLobby/index.html)
        /// </summary>
        /// <param name="onCompleted">Callback when leave lobby is completed</param>
        public void LeaveCurrentLobby(OnCreateLobbyCallback onCompleted = null)
        {
            if (currentLobby == null || string.IsNullOrEmpty(currentLobby.Id) || !EOSManager.Instance.GetProductUserId().IsValid())
            {
                Debug.LogWarning("Lobbies (LeaveLobby): Not currently in a lobby.");
                onCompleted?.Invoke(Epic.OnlineServices.Result.NotFound);
                return;
            }

            UnsubscribeFromRTCEvents();

            LeaveLobbyOptions options = new LeaveLobbyOptions();
            options.LobbyId = currentLobby.Id;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            Debug.LogFormat("Lobbies (LeaveLobby): Attempting to leave lobby: Id='{0}', LocalUserId='{1}'", options.LobbyId, options.LocalUserId);

            lobbyInterface.LeaveLobby(ref options, onCompleted, OnLeaveLobbyCompleted);
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_SendInvite](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_SendInvite/index.html)
        /// </summary>
        /// <param name="targetUserId">Target <c>ProductUserId</c> to send invite</param>
        public void SendInvite(ProductUserId targetUserId)
        {
            if (!targetUserId.IsValid())
            {
                Debug.LogWarning("Lobbies (SendInvite): targetUserId parameter is not valid.");
                return;
            }

            if (!currentLobby.IsValid())
            {
                Debug.LogWarning("Lobbies (SendInvite): CurrentLobby is not valid.");
                return;
            }

            SendInviteOptions options = new SendInviteOptions()
            {
                LobbyId = currentLobby.Id,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                TargetUserId = targetUserId
            };

            lobbyInterface.SendInvite(ref options, null, OnSendInviteCompleted);
        }

        /// <summary>
        /// Wrapper for calling [EOS_LobbyModification_AddMemberAttribute](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_LobbyModification_AddMemberAttribute/index.html)
        /// </summary>
        /// <param name="memberAttribute"><c>LobbyAttribute</c> to be added to the current lobby</param>
        public void SetMemberAttribute(LobbyAttribute memberAttribute)
        {
            if (!currentLobby.IsValid())
            {
                Debug.LogError("Lobbies (SetMemberAttribute): CurrentLobby is not valid.");
                return;
            }

            /// Modify Lobby
            var options = new UpdateLobbyModificationOptions()
            {
                LobbyId = currentLobby.Id,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            Epic.OnlineServices.Result result = lobbyInterface.UpdateLobbyModification(ref options, out LobbyModification lobbyModificationHandle);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SetMemberAttribute): Could not create lobby modification: Error code: {0}", result);
                return;
            }

            /// Update member attribute
            AttributeData attributeData = memberAttribute.AsAttribute;

            var attrOptions = new LobbyModificationAddMemberAttributeOptions()
            {
                Attribute = attributeData,
                Visibility = LobbyAttributeVisibility.Public
            };
            result = lobbyModificationHandle.AddMemberAttribute(ref attrOptions);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SetMemberAttribute): Could not add member attribute: Error code: {0}", result);
                return;
            }

            /// Trigger lobby update
            var updateOptions = new UpdateLobbyOptions()
            {
                LobbyModificationHandle = lobbyModificationHandle
            };

            lobbyInterface.UpdateLobby(ref updateOptions, null, OnUpdateLobbyCallBack);
        }

        void OnSendInviteCompleted(ref SendInviteCallbackInfo data)
        {
            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnSendInviteCompleted): error code: {0}", data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnSendInviteCompleted): invite sent.");
        }

        void OnCreateLobbyCompleted(ref CreateLobbyCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                Game.Console.LogDebug($"LobbyManager/OnCreateLobbyCompleted(): Lobby create finished.");

                /// OnLobbyCreated
                if (!string.IsNullOrEmpty(info.LobbyId) && currentLobby._isBeingCreated)
                {
                    currentLobby.Id = info.LobbyId;
                    ModifyLobby(currentLobby, null);

                    if (currentLobby.RTCRoomEnabled)
                    {
                        SubscribeToRTCEvents();
                    }
                }

                _isDirty = true;

                if (info.ClientData is OnCreateLobbyCallback callback)
                {
                    callback?.Invoke(Epic.OnlineServices.Result.Success);
                }

                OnCurrentLobbyChanged();
            }
            else
            {
                Game.Console.LogDebug($"LobbyManager/OnCreateLobbyCompleted(): Error creating lobby: [{info.ResultCode}]");
                if (info.ClientData is OnCreateLobbyCallback callback)
                {
                    callback?.Invoke(info.ResultCode);
                }
            }
        }

        void OnCurrentLobbyChanged()
        {
            if (currentLobby != null && currentLobby.IsValid())
            {
                AddLocalUserAttributes();
            }
            foreach (var callback in _lobbyChangedCallbacks)
            {
                callback?.Invoke();
            }
        }

        void OnUpdateLobbyCallBack(ref UpdateLobbyCallbackInfo data)
        {
            OnCreateLobbyCallback LobbyModifyCallback = data.ClientData as OnCreateLobbyCallback;

            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnUpdateLobbyCallBack): error code: {0}", data.ResultCode);
                LobbyModifyCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnUpdateLobbyCallBack): lobby updated.");

            OnLobbyUpdated(data.LobbyId, LobbyModifyCallback);
        }

        void OnLobbyUpdated(string lobbyId, OnCreateLobbyCallback LobbyUpdateCompleted)
        {
            // Update Lobby
            if (!string.IsNullOrEmpty(lobbyId) && currentLobby.Id == lobbyId)
            {
                currentLobby.InitFromLobbyHandle(lobbyId);

                LobbyUpdateCompleted?.Invoke(Epic.OnlineServices.Result.Success);

                foreach (var callback in _lobbyUpdatedCallbacks)
                {
                    callback?.Invoke();
                }
            }
        }

        void OnLobbyUpdateReceived(ref LobbyUpdateReceivedCallbackInfo data)
        {
            // Callback for LobbyUpdateNotification

            //if (data != null)
            //{
                Debug.Log("Lobbies (OnLobbyUpdateReceived): lobby update received.");
                OnLobbyUpdated(data.LobbyId, null);
            //}
            //else
            //{
            //    Debug.LogError("Lobbies (OnLobbyUpdateReceived): EOS_Lobby_LobbyUpdateReceivedCallbackInfo is null");
            //}
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_DestroyLobby](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_DestroyLobby/index.html)
        /// </summary>
        /// <param name="DestroyCurrentLobbyCompleted">Callback when destroy lobby is completed</param>
        public void DestroyCurrentLobby(ref OnCreateLobbyCallback DestroyCurrentLobbyCompleted)
        {
            if (currentLobby != null && !currentLobby.IsValid())
            {
                Debug.LogError("Lobbies (DestroyCurrentLobby): CurrentLobby is invalid!");
                DestroyCurrentLobbyCompleted?.Invoke(Epic.OnlineServices.Result.InvalidState);
                return;
            }

            UnsubscribeFromRTCEvents();

            ProductUserId currentProductUserId = EOSManager.Instance.GetProductUserId();
            if (!currentProductUserId.IsValid())
            {
                Debug.LogError("Lobbies (DestroyCurrentLobby): Current player is invalid!");
                DestroyCurrentLobbyCompleted?.Invoke(Epic.OnlineServices.Result.InvalidProductUserID);
                return;
            }

            if (!currentLobby.IsOwner(currentProductUserId))
            {
                Debug.LogError("Lobbies (DestroyCurrentLobby): Current player is now lobby owner!");
                DestroyCurrentLobbyCompleted?.Invoke(Epic.OnlineServices.Result.LobbyNotOwner);
                return;
            }

            var options = new DestroyLobbyOptions()
            {
                LocalUserId = currentProductUserId,
                LobbyId = currentLobby.Id
            };
            lobbyInterface.DestroyLobby(ref options, DestroyCurrentLobbyCompleted, OnDestroyLobbyCompleted);

            // Clear current lobby
            currentLobby.ClearCache();
        }

        void OnDestroyLobbyCompleted(ref DestroyLobbyCallbackInfo data)
        {
            OnCreateLobbyCallback DestroyLobbyCallback = data.ClientData as OnCreateLobbyCallback;

            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnDestroyLobbyCompleted): error code: {0}", data.ResultCode);
                DestroyLobbyCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnDestroyLobbyCompleted): Lobby destroyed.");

            //_LobbyLeaveInProgress = false;
            if (currentJoinRequest.IsValid())
            {
                LobbyJoinRequest lobbyToJoin = currentJoinRequest;
                currentJoinRequest.Clear();

            }

            DestroyLobbyCallback?.Invoke(Epic.OnlineServices.Result.Success);
        }

        public void EnablePressToTalk(ProductUserId targetUserId, OnCreateLobbyCallback EnablePressToTalkCompleted)
        {
            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();

            foreach (LobbyMember lobbyMember in currentLobby.Members)
            {
                // Find the correct lobby member
                if (lobbyMember.ProductId != targetUserId)
                {
                    continue;
                }

                lobbyMember.RTCState.PressToTalkEnabled = !lobbyMember.RTCState.PressToTalkEnabled;

                var sendVolumeOptions = new UpdateSendingVolumeOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RoomName = currentLobby.RTCRoomName,
                    Volume = lobbyMember.RTCState.PressToTalkEnabled ? 0 : 50
                };

                Debug.LogFormat("Press To Talk Enabled : {0} : Current self Audio output volume is {1}", lobbyMember.RTCState.PressToTalkEnabled, sendVolumeOptions.Volume);

                rtcAudioHandle.UpdateSendingVolume(ref sendVolumeOptions, EnablePressToTalkCompleted, null);
            }
        }
        // Member Events
        public void PressToTalk(KeyCode PTTKeyCode, OnCreateLobbyCallback TogglePressToTalkCompleted)
        {
            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();
            ProductUserId targetUserId = EOSManager.Instance.GetProductUserId();

            foreach (LobbyMember lobbyMember in currentLobby.Members)
            {
                // Find the correct lobby member
                if (lobbyMember.ProductId != targetUserId)
                {
                    continue;
                }

                var sendVolumeOptions = new UpdateSendingVolumeOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RoomName = currentLobby.RTCRoomName,
                    Volume = Input.GetKey(PTTKeyCode) ? 50 : 0
                };

                Debug.LogFormat("Lobbies (TogglePressToTalk): Setting self audio output volume to {0}", sendVolumeOptions.Volume);

                rtcAudioHandle.UpdateSendingVolume(ref sendVolumeOptions, TogglePressToTalkCompleted, null);
            }
        }

        /// <summary>
        /// Wrapper for calling [EOS_RTCAudio_UpdateReceiving](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/NoInterface/EOS_RTCAudio_UpdateReceiving/index.html)
        /// </summary>
        /// <param name="targetUserId">Target <c>ProductUserId</c> to mute or unmute</param>
        /// <param name="ToggleMuteCompleted">Callback when toggle mute is completed</param>
        public void ToggleMute(ProductUserId targetUserId, OnCreateLobbyCallback ToggleMuteCompleted)
        {
            RTCInterface rtcHandle = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface rtcAudioHandle = rtcHandle.GetAudioInterface();

            foreach (LobbyMember lobbyMember in currentLobby.Members)
            {
                // Find the correct lobby member
                if (lobbyMember.ProductId != targetUserId)
                {
                    continue;
                }

                // Do not allow multiple local mute toggles at the same time
                if (lobbyMember.RTCState.MuteActionInProgress)
                {
                    Debug.LogWarningFormat("Lobbies (ToggleMute): 'MuteActionInProgress' for productUserId {0}.", targetUserId);
                    ToggleMuteCompleted?.Invoke(Epic.OnlineServices.Result.RequestInProgress);
                    return;
                }

                // Set mute action as in progress
                lobbyMember.RTCState.MuteActionInProgress = true;

                // Check if muting ourselves vs other member
                if (EOSManager.Instance.GetProductUserId() == targetUserId)
                {
                    // Toggle our mute status
                    UpdateSendingOptions sendOptions = new UpdateSendingOptions()
                    {
                        LocalUserId = EOSManager.Instance.GetProductUserId(),
                        RoomName = currentLobby.RTCRoomName,
                        AudioStatus = lobbyMember.RTCState.IsLocalMuted ? RTCAudioStatus.Enabled : RTCAudioStatus.Disabled
                    };

                    Debug.LogFormat("Lobbies (ToggleMute): Setting self audio output status to {0}", sendOptions.AudioStatus == RTCAudioStatus.Enabled ? "Unmuted" : "Muted");

                    rtcAudioHandle.UpdateSending(ref sendOptions, ToggleMuteCompleted, OnRTCRoomUpdateSendingCompleted);
                }
                else
                {
                    // Toggle mute for remote member (this is a local-only action and does not block the other user from receiving your audio stream)

                    UpdateReceivingOptions recevingOptions = new UpdateReceivingOptions()
                    {
                        LocalUserId = EOSManager.Instance.GetProductUserId(),
                        RoomName = currentLobby.RTCRoomName,
                        ParticipantId = targetUserId,
                        AudioEnabled = lobbyMember.RTCState.IsLocalMuted
                    };

                    Debug.LogFormat("Lobbies (ToggleMute): {0} remote player {1}", recevingOptions.AudioEnabled ? "Unmuting" : "Muting", targetUserId);

                    rtcAudioHandle.UpdateReceiving(ref recevingOptions, ToggleMuteCompleted, OnRTCRoomUpdateReceivingCompleted);
                }
            }
        }

        void OnRTCRoomUpdateSendingCompleted(ref UpdateSendingCallbackInfo data)
        {
            OnCreateLobbyCallback ToggleMuteCallback = data.ClientData as OnCreateLobbyCallback;

            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): error code: {0}", data.ResultCode);
                ToggleMuteCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.LogFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): Updated sending status successfully. Room={0}, AudioStatus={1}", data.RoomName, data.AudioStatus);

            /// Ensure this update is for our room
            if (!currentLobby.RTCRoomName.Equals(data.RoomName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): Incorrect Room! CurrentLobby.RTCRoomName={0} != data.RoomName", currentLobby.RTCRoomName, data.RoomName);
                return;
            }

            /// Ensure this update is for us
            if (EOSManager.Instance.GetProductUserId() != data.LocalUserId)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): Incorrect LocalUserId! LocalProductId={0} != data.LocalUserId", EOSManager.Instance.GetProductUserId(), data.LocalUserId);
                return;
            }

            /// Update our mute status
            foreach(LobbyMember lobbyMember in currentLobby.Members)
            {
                // Find ourselves
                if (lobbyMember.ProductId != data.LocalUserId)
                {
                    continue;
                }

                lobbyMember.RTCState.IsAudioOutputDisabled = data.AudioStatus == RTCAudioStatus.Disabled;
                lobbyMember.RTCState.MuteActionInProgress = false;

                Debug.LogFormat("Lobbies (OnRTCRoomUpdateSendingCompleted): Cache updated for '{0}'", lobbyMember.ProductId);

                _isDirty = true;
                break;
            }

            ToggleMuteCallback?.Invoke(data.ResultCode);
        }

        void OnRTCRoomUpdateReceivingCompleted(ref UpdateReceivingCallbackInfo data)
        {
            OnCreateLobbyCallback ToggleMuteCallback = data.ClientData as OnCreateLobbyCallback;

            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): error code: {0}", data.ResultCode);
                ToggleMuteCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.LogFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): Updated receiving status successfully. LocalUserId={0} Room={1}, IsMuted={2}", data.LocalUserId, data.RoomName, data.AudioEnabled == false);

            // Ensure this update is for our room
            if (!currentLobby.RTCRoomName.Equals(data.RoomName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): Incorrect Room! CurrentLobby.RTCRoomName={0} != data.RoomName", currentLobby.RTCRoomName, data.RoomName);
                return;
            }

            // Update should be for remote user
            if (EOSManager.Instance.GetProductUserId() != data.LocalUserId)
            {
                Debug.LogErrorFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): Incorrect call for local member.");
                return;
            }

            foreach(LobbyMember lobbyMember in currentLobby.Members)
            { 
                if (lobbyMember.ProductId != data.ParticipantId)
                {
                    continue;
                }

                lobbyMember.RTCState.IsLocalMuted = data.AudioEnabled == false;
                lobbyMember.RTCState.MuteActionInProgress = false;

                Debug.LogFormat("Lobbies (OnRTCRoomUpdateReceivingCompleted): Cache updated for '{0}'", lobbyMember.ProductId);

                _isDirty = true;
                break;
            }
        }

        /// <summary>
        /// Wrapper for calling [EOS_Lobby_KickMember](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_KickMember/index.html)
        /// </summary>
        /// <param name="productUserId">Target <c>ProductUserId</c> to kick from current lobby</param>
        /// <param name="KickMemberCompleted">Callback when kick member is completed</param>
        public void KickMember(ProductUserId productUserId, OnCreateLobbyCallback KickMemberCompleted)
        {
            if (!productUserId.IsValid())
            {
                Debug.LogError("Lobbies (KickMember): productUserId is invalid!");
                KickMemberCompleted?.Invoke(Epic.OnlineServices.Result.InvalidState);
                return;
            }

            ProductUserId currentUserId = EOSManager.Instance.GetProductUserId();
            if (!currentUserId.IsValid())
            {
                Debug.LogError("Lobbies (KickMember): Current player is invalid!");
                KickMemberCompleted?.Invoke(Epic.OnlineServices.Result.InvalidState);
                return;
            }

            KickMemberOptions kickOptions = new KickMemberOptions();
            kickOptions.TargetUserId = productUserId;
            kickOptions.LobbyId = currentLobby.Id;
            kickOptions.LocalUserId = currentUserId;

            lobbyInterface.KickMember(ref kickOptions, KickMemberCompleted, OnKickMemberCompleted);
        }

        void OnKickMemberCompleted(ref KickMemberCallbackInfo data)
        {
            OnCreateLobbyCallback KickMemberCallback = data.ClientData as OnCreateLobbyCallback;

            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnKickMemberFinished): error code: {0}", data.ResultCode);
                KickMemberCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnKickMemberFinished): Member kicked.");
            KickMemberCallback?.Invoke(Epic.OnlineServices.Result.Success);
        }

        void OnKickedFromLobby(string lobbyId)
        {
            Debug.LogFormat("Lobbies (OnKickedFromLobby):  Kicked from lobby: {0}", lobbyId);
            if (currentLobby != null && currentLobby.IsValid() && currentLobby.Id.Equals(lobbyId, StringComparison.OrdinalIgnoreCase))
            {
                currentLobby.ClearCache();
                _isDirty = true;

                OnCurrentLobbyChanged();
            }
        }

        /// <summary>
        /// Promote an existing member of the lobby to owner, allowing them to make lobby data modifications.
        /// Wrapper for calling [EOS_Lobby_PromoteMember](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_PromoteMember/index.html)
        /// </summary>
        /// <param name="productUserId">Target <c>ProductUserId</c> to promote</param>
        /// <param name="promoteMemberCompleted">Callback when promote member is completed</param>
        public void PromoteMember(ProductUserId productUserId, OnCreateLobbyCallback promoteMemberCompleted)
        {
            if (productUserId == null || !productUserId.IsValid())
            {
                Debug.LogError("[LobbyManager/PromoteMember()]: productUserId is invalid!");
                promoteMemberCompleted?.Invoke(Epic.OnlineServices.Result.InvalidProductUserID);
                return;
            }

            ProductUserId currentUserId = EOSManager.Instance.GetProductUserId();
            if (currentUserId == null || !currentUserId.IsValid())
            {
                Debug.LogError("[LobbyManager/PromoteMember()]: Current player is invalid!");
                promoteMemberCompleted?.Invoke(Epic.OnlineServices.Result.InvalidProductUserID);
                return;
            }

            var promoteOptions = new PromoteMemberOptions
            {
                TargetUserId = productUserId,
                LocalUserId = currentUserId,
                LobbyId = currentLobby.Id
            };
            lobbyInterface.PromoteMember(ref promoteOptions, promoteMemberCompleted, OnPromoteMemberCompleted);
        }

        void OnPromoteMemberCompleted(ref PromoteMemberCallbackInfo info)
        {
            if (info.ResultCode != Epic.OnlineServices.Result.Success)
            {
                if (info.ClientData is OnCreateLobbyCallback callback1)
                {
                    Debug.LogError($"[LobbyManager/OnPromoteMemberFinished()]: Error promoting member: {info.ResultCode}");
                    callback1?.Invoke(info.ResultCode);
                }
                return;
            }

            Debug.Log("[LobbyManager/OnPromoteMemberCompleted()]: Member promoted.");
            if (info.ClientData is OnCreateLobbyCallback callback)
            {
                callback?.Invoke(Epic.OnlineServices.Result.Success);
            }
        }

        void OnLeaveLobbyRequested(ref LeaveLobbyRequestedCallbackInfo data)
        {
            Debug.Log("Lobbies (OnLeaveLobbyRequested): Leave Lobby Requested via Overlay.");
            LeaveCurrentLobby(null);
        }

        void OnMemberStatusReceived(ref LobbyMemberStatusReceivedCallbackInfo info)
        {
            Game.Console.LogDebug($"[LobbyManager/OnMemberStatusReceived()]: Member status update received");

            if (!info.TargetUserId.IsValid())
            {
                Game.Console.LogDebug($"[LobbyManager/OnMemberStatusReceived()]: Invalid user");
                /// Simply update the whole lobby
                OnLobbyUpdated(info.LobbyId, null);
                return;
            }

            bool updateLobby = true;

            if (info.CurrentStatus == LobbyMemberStatus.Joined)
            {
                Game.Console.LogDebug($"[LobbyManager/OnMemberStatusReceived()]: User[{info.TargetUserId}] joined the lobby");
                Game.Console.LogInfo($"User[{info.TargetUserId}] joined the lobby");
            }

            /// Update target member status for everyone
            /// Current player updates need special handing
            ProductUserId currentPlayer = Game.EOS.GetProductUserId();

            if (info.TargetUserId == currentPlayer)
            {
                if (info.CurrentStatus == LobbyMemberStatus.Closed ||
                    info.CurrentStatus == LobbyMemberStatus.Kicked ||
                    info.CurrentStatus == LobbyMemberStatus.Disconnected)
                {
                    OnKickedFromLobby(info.LobbyId);
                    updateLobby = false;
                }
            }

            if (updateLobby)
            {
                OnLobbyUpdated(info.LobbyId, null);
            }

            foreach (var callback in _memberStatusCallbacks)
            {
                callback?.Invoke(info.LobbyId, info.TargetUserId, info.CurrentStatus);
            }
        }

        void OnMemberUpdateReceived(ref LobbyMemberUpdateReceivedCallbackInfo data)
        {
            // Callback for LobbyMemberUpdateNotification

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnMemberUpdateReceived): LobbyMemberUpdateReceivedCallbackInfo data is null");
            //    return;
            //}

            Game.Console.LogDebug($"[LobbyManager/OnMemberUpdateReceived()]: Member update received.");
            OnLobbyUpdated(data.LobbyId, null);

            foreach (var callback in _memberUpdatedCallbacks)
            {
                callback?.Invoke(data.LobbyId, data.TargetUserId);
            }
        }

        /// <summary>
        /// Use to access functionality of [EOS_Lobby_AddNotifyLobbyMemberUpdateReceived](https://dev.epicgames.com/docs/api-ref/functions/eos-lobby-add-notify-lobby-member-status-received)
        /// </summary>
        /// <param name="callback">Callback to receive notification when lobby member update is received</param>
        public void AddNotifyMemberUpdateReceived(OnMemberUpdateCallback callback)
        {
            _memberUpdatedCallbacks.Add(callback);
        }

        public void RemoveNotifyMemberUpdate(OnMemberUpdateCallback callback)
        {
            _memberUpdatedCallbacks.Remove(callback);
        }
        
        /// <summary>
        /// Use to access functionality of [EOS_Lobby_AddNotifyLobbyMemberUpdateReceived](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_AddNotifyLobbyMemberUpdateReceived/index.html)
        /// </summary>
        /// <param name="callback">Callback to receive notification when lobby member update is received</param>
        public void AddNotifyMemberStatusReceived(OnMemberStatusCallback Callback)
        {
            _memberStatusCallbacks.Add(Callback);
        }
        public void RemoveNotifyMemberStatusReceived(OnMemberStatusCallback Callback)
        {
            _memberStatusCallbacks.Remove(Callback);
        }

        /// <summary>
        /// Subscribe to event callback for when the user has changed lobbies
        /// </summary>
        /// <param name="Callback">Callback to receive notification when lobby is changed</param>
        public void AddNotifyLobbyChange(Action Callback)
        {
            _lobbyChangedCallbacks.Add(Callback);
        }

        public void RemoveNotifyLobbyChange(Action Callback)
        {
            _lobbyChangedCallbacks.Remove(Callback);
        }

        /// <summary>
        /// Subscribe to event callback for when the current lobby data has been updated
        /// </summary>
        /// <param name="Callback">Callback to receive notification when lobby data is updated</param>
        public void AddNotifyLobbyUpdate(Action Callback)
        {
            _lobbyUpdatedCallbacks.Add(Callback);
        }

        public void RemoveNotifyLobbyUpdate(Action Callback)
        {
            _lobbyUpdatedCallbacks.Remove(Callback);
        }
        // Search Events

        /// <summary>
        /// Wrapper for calling [EOS_LobbySearch_Find](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_LobbySearch_Find/index.html)
        /// </summary>
        /// <param name="lobbyId"><c>string</c> of lobbyId to search</param>
        /// <param name="SearchCompleted">Callback when search is completed</param>
        public void SearchByLobbyId(string lobbyId, OnSearchByLobbyIdCallback SearchCompleted)
        {
            Debug.LogFormat("Lobbies (SearchByLobbyId): lobbyId='{0}'", lobbyId);

            if (string.IsNullOrEmpty(lobbyId))
            {
                Debug.LogWarning("Lobbies (SearchByLobbyId): lobbyId is null or empty!");
                SearchCompleted?.Invoke(Epic.OnlineServices.Result.InvalidParameters);
                return;
            }

            ProductUserId currentProductUserId = EOSManager.Instance.GetProductUserId();
            if (!currentProductUserId.IsValid())
            {
                Debug.LogError("Lobbies (SearchByLobbyId): Current player is invalid!");
                SearchCompleted?.Invoke(Epic.OnlineServices.Result.InvalidProductUserID);
                return;
            }

            var createLobbySearchOptions = new CreateLobbySearchOptions() { MaxResults = 10 };
            Epic.OnlineServices.Result result = lobbyInterface.CreateLobbySearch(ref createLobbySearchOptions, out LobbySearch outLobbySearchHandle);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByLobbyId): could not create SearchByLobbyId. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            currentSearch = outLobbySearchHandle;

            LobbySearchSetLobbyIdOptions setLobbyOptions = new LobbySearchSetLobbyIdOptions();
            setLobbyOptions.LobbyId = lobbyId;

            result = outLobbySearchHandle.SetLobbyId(ref setLobbyOptions);
            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByLobbyId): failed to update SearchByLobbyId with lobby id. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            _lobbySearchCallback = SearchCompleted;
            var lobbySearchFindOptions = new LobbySearchFindOptions() { LocalUserId = currentProductUserId };
            outLobbySearchHandle.Find(ref lobbySearchFindOptions, null, OnLobbySearchCompleted);
        }

        const int MAX_LOBBY_SEARCH_RESULTS = 16;
        /// <summary>
        /// Wrapper for calling [EOS_LobbySearch_Find](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_LobbySearch_Find/index.html)
        /// </summary>
        /// <param name="attributeKey"><c>string</c> of attributeKey to search</param>
        /// <param name="attributeValue"><c>string</c> of attributeValue to search</param>
        /// <param name="SearchCompleted">Callback when search is completed</param>
        public void SearchByAttribute(string attributeKey, string attributeValue, OnSearchByLobbyIdCallback SearchCompleted)
        {
            Debug.LogFormat("Lobbies (SearchByAttribute): searchString='{0}'", attributeKey);

            if (string.IsNullOrEmpty(attributeKey))
            {
                Debug.LogError("[LobbyManager/SearchByAttribute()]: Search string is null or empty!");
                SearchCompleted?.Invoke(Epic.OnlineServices.Result.InvalidParameters);
                return;
            }

            ProductUserId currentProductUserId = EOSManager.Instance.GetProductUserId();
            if (currentProductUserId == null || !currentProductUserId.IsValid())
            {
                Debug.LogError("Lobbies (SearchByAttribute): Current player is invalid!");
                SearchCompleted?.Invoke(Epic.OnlineServices.Result.InvalidProductUserID);
                return;
            }

            var createLobbySearchOptions = new CreateLobbySearchOptions() { MaxResults = MAX_LOBBY_SEARCH_RESULTS };
            Epic.OnlineServices.Result result = lobbyInterface.CreateLobbySearch(ref createLobbySearchOptions, out LobbySearch outLobbySearchHandle);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByAttribute): could not create SearchByAttribute. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            currentSearch = outLobbySearchHandle;

            var paramOptions = new LobbySearchSetParameterOptions
            {
                ComparisonOp = ComparisonOp.Equal
            };
            /// Turn SearchString into AttributeData
            var attrData = new AttributeData
            {
                Key = attributeKey.Trim(),
                Value = new AttributeDataValue()
            };
            attrData.Value = attributeValue.Trim();
            paramOptions.Parameter = attrData;

            result = outLobbySearchHandle.SetParameter(ref paramOptions);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (SearchByAttribute): failed to update SearchByAttribute with parameter. Error code: {0}", result);
                SearchCompleted?.Invoke(result);
                return;
            }

            _lobbySearchCallback = SearchCompleted;
            var lobbySearchFindOptions = new LobbySearchFindOptions() { LocalUserId = currentProductUserId };
            outLobbySearchHandle.Find(ref lobbySearchFindOptions, null, OnLobbySearchCompleted);
        }

        void OnLobbySearchCompleted(ref LobbySearchFindCallbackInfo data)
        {
            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                if (data.ResultCode == Epic.OnlineServices.Result.NotFound)
                {
                    // It's not an error if there's no results found when searching
                    Debug.Log("Lobbies (OnLobbySearchCompleted): No results found.");
                }
                else
                {
                    Debug.LogErrorFormat("Lobbies (OnLobbySearchCompleted): error code: {0}", data.ResultCode);
                }

                _lobbySearchCallback?.Invoke(data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnLobbySearchCompleted): Search finished.");

            // Process Search Results
            var lobbySearchGetSearchResultCountOptions = new LobbySearchGetSearchResultCountOptions(); 
            uint searchResultCount = currentSearch.GetSearchResultCount(ref lobbySearchGetSearchResultCountOptions);

            Debug.LogFormat("Lobbies (OnLobbySearchCompleted): searchResultCount = {0}", searchResultCount);

            searchResults.Clear();

            LobbySearchCopySearchResultByIndexOptions indexOptions = new LobbySearchCopySearchResultByIndexOptions();

            for (uint i = 0; i < searchResultCount; i++)
            {
                Lobby lobbyObj = new Lobby();

                indexOptions.LobbyIndex = i;

                Epic.OnlineServices.Result result = currentSearch.CopySearchResultByIndex(ref indexOptions, out LobbyDetails outLobbyDetailsHandle);

                if (result == Epic.OnlineServices.Result.Success && outLobbyDetailsHandle != null)
                {
                    lobbyObj.InitializeFromLobbyDetails(outLobbyDetailsHandle);

                    if (lobbyObj == null)
                    {
                        Debug.LogWarning("Lobbies (OnLobbySearchCompleted): lobbyObj is null!");
                        continue;
                    }

                    if (!lobbyObj.IsValid())
                    {
                        Debug.LogWarning("Lobbies (OnLobbySearchCompleted): Lobby is invalid, skip.");
                        continue;
                    }

                    if (outLobbyDetailsHandle == null)
                    {
                        Debug.LogWarning("Lobbies (OnLobbySearchCompleted): outLobbyDetailsHandle is null!");
                        continue;
                    }

                    searchResults.Add(lobbyObj, outLobbyDetailsHandle);

                    Debug.LogFormat("Lobbies (OnLobbySearchCompleted): Added lobbyId: '{0}'", lobbyObj.Id);
                }
            }

            Debug.Log("Lobbies  (OnLobbySearchCompleted):  SearchResults Lobby objects = " + searchResults.Count);

            _lobbySearchCallback?.Invoke(Epic.OnlineServices.Result.Success);
        }

        void OnLobbyJoinFailed(string lobbyId)
        {
            _isDirty = true;

            PopLobbyInvite();
        }

        // Invite
        void OnLobbyInvite(string inviteId, ProductUserId senderUserId)
        {
            LobbyInvite newLobbyInvite = new LobbyInvite();

            CopyLobbyDetailsHandleByInviteIdOptions options = new CopyLobbyDetailsHandleByInviteIdOptions();
            options.InviteId = inviteId;

            Epic.OnlineServices.Result result = lobbyInterface.CopyLobbyDetailsHandleByInviteId(ref options, out LobbyDetails outLobbyDetailsHandle);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnLobbyInvite): could not get lobby details: error code: {0}", result);
                return;
            }
            if (outLobbyDetailsHandle == null)
            {
                Debug.LogError("Lobbies (OnLobbyInvite): could not get lobby details: null details handle.");
                return;
            }

            newLobbyInvite.Lobby.InitializeFromLobbyDetails(outLobbyDetailsHandle);
            newLobbyInvite.LobbyInfo = outLobbyDetailsHandle;
            newLobbyInvite.InviteId = inviteId;

            newLobbyInvite.FriendId = senderUserId;
            //newLobbyInvite.FriendEpicId = new EpicAccountId(); // TODO!!!!

            // If there's already an invite, check to see if the sender is the same as the current.
            // If it is, then update the current invite with the new invite.
            // If not, then add/update the new invite for the new sender.
            if (currentInvite == null ||
                (currentInvite != null && senderUserId == currentInvite.FriendId))
            {
                currentInvite = newLobbyInvite;
            }
            else
            {
                // This is not the current invite by the invite sender. Add it to the dictionary
                // if it doesn't exist, or update it if there's already an invite from this sender.
                pendingInvites.TryGetValue(senderUserId, out LobbyInvite invite);

                if (invite == null)
                {
                    pendingInvites.Add(senderUserId, newLobbyInvite);
                }
                else
                {
                    pendingInvites[senderUserId] = newLobbyInvite;
                }
            }

            _isDirty = true;
        }

        void OnJoinLobbyAccepted(ref JoinLobbyAcceptedCallbackInfo data)
        {
            // Callback for JoinLobbyAcceptedNotification

            CopyLobbyDetailsHandleByUiEventIdOptions options = new CopyLobbyDetailsHandleByUiEventIdOptions();
            options.UiEventId = data.UiEventId;

            Epic.OnlineServices.Result result = lobbyInterface.CopyLobbyDetailsHandleByUiEventId(ref options, out LobbyDetails outLobbyDetailsHandle);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnJoinLobbyAccepted): could not get lobby details: error code: {0}", result);
                return;
            }
            if (outLobbyDetailsHandle == null)
            {
                Debug.LogError("Lobbies (OnJoinLobbyAccepted): could not get lobby details: null details handle.");
                return;
            }

            Lobby newLobby = new Lobby();
            newLobby.InitializeFromLobbyDetails(outLobbyDetailsHandle);

            JoinLobby(newLobby.Id, outLobbyDetailsHandle, true, null);
            PopLobbyInvite();
        }

        void OnLobbyInviteReceived(ref LobbyInviteReceivedCallbackInfo data)
        {
            // Callback for LobbyInviteNotification

            //if (data == null)
            //{
            //    Debug.LogFormat("Lobbies (OnLobbyInviteReceived): LobbyInviteReceivedCallbackInfo data is null");
            //    return;
            //}

            CopyLobbyDetailsHandleByInviteIdOptions options = new CopyLobbyDetailsHandleByInviteIdOptions();
            options.InviteId = data.InviteId;

            Epic.OnlineServices.Result result = lobbyInterface.CopyLobbyDetailsHandleByInviteId(ref options, out LobbyDetails outLobbyDetailsHandle);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnLobbyInvite): could not get lobby details: error code: {0}", result);
                return;
            }
            if (outLobbyDetailsHandle == null)
            {
                Debug.LogError("Lobbies (OnLobbyInvite): could not get lobby details: null details handle.");
                return;
            }

            OnLobbyInvite(data.InviteId, data.TargetUserId);
        }

        void OnLobbyInviteAccepted(ref LobbyInviteAcceptedCallbackInfo data)
        {
            // Callback for LobbyInviteAcceptedNotification

            //if (data == null)
            //{
            //    Debug.LogError("Lobbies  (OnLobbyInviteAccepted): LobbyInviteAcceptedCallbackInfo data is null");
            //    return;
            //}

            CopyLobbyDetailsHandleByInviteIdOptions options = new CopyLobbyDetailsHandleByInviteIdOptions()
            {
                InviteId = data.InviteId
            };

            Epic.OnlineServices.Result lobbyDetailsResult = lobbyInterface.CopyLobbyDetailsHandleByInviteId(ref options, out LobbyDetails outLobbyDetailsHandle);

            if (lobbyDetailsResult != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnLobbyInviteAccepted) could not get lobby details: error code: {0}", lobbyDetailsResult);
                return;
            }
            if (outLobbyDetailsHandle == null)
            {
                Debug.LogError("Lobbies (OnLobbyInviteAccepted) could not get lobby details: null details handle.");
                return;
            }

            Lobby lobby = new Lobby();
            lobby.InitializeFromLobbyDetails(outLobbyDetailsHandle);

            JoinLobby(lobby.Id, outLobbyDetailsHandle, true, null);
        }

        void PopLobbyInvite()
        {
            if (pendingInvites.Count > 0)
            {
                var nextInvite = pendingInvites.GetEnumerator();
                nextInvite.MoveNext();
                currentInvite = nextInvite.Current.Value;
                pendingInvites.Remove(nextInvite.Current.Key);
            }
            else
            {
                currentInvite = null;
            }
        }

        // CanInviteToCurrentLobby()

        //-------------------------------------------------------------------------
        // It appears that to get RTC to work on some platforms, the RTC API needs to be
        // 'kicked' to enumerate the input and output devices.
        //
        void HackWorkaroundRTCInitIssues()
        {
            // Hack to get RTC working
            var audioOptions = new Epic.OnlineServices.RTCAudio.GetAudioInputDevicesCountOptions();
            EOSManager.Instance.GetEOSRTCInterface().GetAudioInterface().GetAudioInputDevicesCount(ref audioOptions);

            var audioOutputOptions = new Epic.OnlineServices.RTCAudio.GetAudioOutputDevicesCountOptions();
            EOSManager.Instance.GetEOSRTCInterface().GetAudioInterface().GetAudioOutputDevicesCount(ref audioOutputOptions);
        }

        void AddLocalUserAttributes()
        {
            var userInfo = EOSSubManagers.UserInfo.GetLocalUserInfo();

            if (!string.IsNullOrEmpty(userInfo.DisplayName))
            {
                var attr = new LobbyAttribute()
                {
                    Key = AttributeKeys.LOBBY_OWNER_DISPLAY_NAME,
                    AsString = userInfo.DisplayName,
                    ValueType = AttributeType.String
                };
                SetMemberAttribute(attr);
            }
        }


        #region Join Events

        //-------------------------------------------------------------------------
        /// <summary>
        /// Wrapper for calling [EOS_Lobby_JoinLobby](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_JoinLobby/index.html)
        /// </summary>
        /// <param name="lobbyId">Target lobbyId of lobby to join</param>
        /// <param name="lobbyDetails">Reference to <c>LobbyDetails</c> of lobby to join</param>
        /// <param name="presenceEnabled">Presence Enabled if <c>true</c></param>
        /// <param name="onCompleted">Callback when join lobby is completed</param>
        public void JoinLobby(string lobbyId, LobbyDetails lobbyDetails, bool presenceEnabled, OnCreateLobbyCallback onCompleted)
        {
            Game.Console.LogInfo($"Joining selected lobby [{lobbyId}]");

            HackWorkaroundRTCInitIssues();

            if (string.IsNullOrEmpty(lobbyId))
            {
                Debug.LogError("Lobbies (JoinButtonOnClick): lobbyId is null or empty!");
                onCompleted?.Invoke(Epic.OnlineServices.Result.InvalidParameters);
                return;
            }

            if (lobbyDetails == null)
            {
                Debug.LogError("Lobbies (JoinButtonOnClick): lobbyDetails is null!");
                onCompleted?.Invoke(Epic.OnlineServices.Result.InvalidParameters);
                return;
            }

            if (currentLobby != null && currentLobby.IsValid())
            {
                if (string.Equals(currentLobby.Id, lobbyId, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("Lobbies (JoinLobby): Already in the same lobby!");
                    return;
                }

                // TODO Active Join
                //ActiveJoin = 
                LeaveCurrentLobby(null);
                Debug.LogError("Lobbies (JoinLobby): Leaving lobby now (must Join again, Active Join Not Implemented)!");
                onCompleted?.Invoke(Epic.OnlineServices.Result.InvalidState);

                return;
            }

            var joinOptions = new JoinLobbyOptions
            {
                LobbyDetailsHandle = lobbyDetails,
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                PresenceEnabled = presenceEnabled
            };
            if (customLocalRTCOptions != null)
            {
                joinOptions.LocalRTCOptions = customLocalRTCOptions;
            }
            lobbyInterface.JoinLobby(ref joinOptions, onCompleted, OnJoinLobbyCompleted);
        }

        void OnJoinLobbyCompleted(ref JoinLobbyCallbackInfo data)
        {
            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Debug.LogError($"[LobbyManager/OnJoinLobbyCompleted()]: Error joining lobby {data.ResultCode}");
                if (data.ClientData is OnCreateLobbyCallback callback1)
                {
                    callback1?.Invoke(data.ResultCode);
                }
                OnLobbyJoinFailed(data.LobbyId);
                return;
            }

            Game.Console.LogInfo($"Joined lobby [{data.LobbyId}]");
            Game.Console.LogDebug($"[LobbyManager/OnJoinLobbyCompleted()]: Lobby join finished.");

            /// If there is a current lobby, and it's not the joined lobby, leave current
            if (currentLobby != null && currentLobby.IsValid() && !string.Equals(currentLobby.Id, data.LobbyId))
            {
                LeaveCurrentLobby(null);
            }

            currentLobby = new();
            currentLobby.InitFromLobbyHandle(data.LobbyId);
            if (currentLobby.RTCRoomEnabled)
            {
                SubscribeToRTCEvents();
            }
            _isDirty = true;
            PopLobbyInvite();

            if (data.ClientData is OnCreateLobbyCallback callback)
            {
                callback?.Invoke(Epic.OnlineServices.Result.Success);
            }
            OnCurrentLobbyChanged();
        }

        void OnLeaveLobbyCompleted(ref LeaveLobbyCallbackInfo data)
        {
            OnCreateLobbyCallback LeaveLobbyCallback = data.ClientData as OnCreateLobbyCallback;

            //if (data == null)
            //{
            //    Debug.LogFormat("Lobbies (OnLeaveLobbyCompleted): LobbyInviteReceivedCallbackInfo data is null");
            //    LeaveLobbyCallback?.Invoke(Result.InvalidState);
            //    return;
            //}

            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Debug.LogFormat("Lobbies (OnLeaveLobbyCompleted): error code: {0}", data.ResultCode);
                LeaveLobbyCallback?.Invoke(data.ResultCode);
            }
            else
            {
                Debug.Log("Lobbies (OnLeaveLobbyCompleted): Successfully left lobby: " + data.LobbyId);

                currentLobby.ClearCache();

                LeaveLobbyCallback?.Invoke(Epic.OnlineServices.Result.Success);

                OnCurrentLobbyChanged();
            }
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Wrapper for calling [EOS_Lobby_RejectInvite](https://dev.epicgames.com/docs/services/en-US/API/Members/Functions/Lobby/EOS_Lobby_RejectInvite/index.html)
        /// </summary>
        public void DeclineLobbyInvite()
        {
            if (currentInvite != null && currentInvite.IsValid())
            {
                ProductUserId currentUserProductId = EOSManager.Instance.GetProductUserId();
                if (!currentUserProductId.IsValid())
                {
                    Debug.LogError("Lobbies (DeclineLobbyInvite): Current player is invalid!");
                    return;
                }

                RejectInviteOptions rejectOptions = new RejectInviteOptions();
                rejectOptions.InviteId = currentInvite.InviteId;
                rejectOptions.LocalUserId = currentUserProductId;

                lobbyInterface.RejectInvite(ref rejectOptions, null, OnDeclineInviteCompleted);
            }

            // LobbyId does not match current invite, reject can be ignored
        }

        void OnDeclineInviteCompleted(ref RejectInviteCallbackInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("Lobbies (OnDeclineInviteCompleted): RejectInviteCallbackInfo data is null");
            //    return;
            //}

            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("Lobbies (OnDeclineInviteCompleted): error code: {0}", data.ResultCode);
                return;
            }

            Debug.Log("Lobbies (OnDeclineInviteCompleted): Invite rejected");

            PopLobbyInvite();
        }

        /// <summary>
        /// If there is a current invite, calls <c>JoinLobby</c>
        /// </summary>
        /// <param name="enablePresence">Presence Enabled if <c>true</c></param>
        /// <param name="AcceptLobbyInviteCompleted">Callback when join lobby is completed</param>
        public void AcceptCurrentLobbyInvite(bool enablePresence, OnCreateLobbyCallback AcceptLobbyInviteCompleted)
        {
            if (currentInvite != null && currentInvite.IsValid())
            {
                Debug.Log("Lobbies (AcceptCurrentLobbyInvite): Accepted invite, joining lobby.");

                JoinLobby(currentInvite.Lobby.Id, currentInvite.LobbyInfo, enablePresence, AcceptLobbyInviteCompleted);
            }
            else
            {
                Debug.LogError("Lobbies (AcceptCurrentLobbyInvite): Current invite is null or invalid!");

                AcceptLobbyInviteCompleted(Epic.OnlineServices.Result.InvalidState);
            }
        }

        /// <summary>
        /// Calls <c>JoinLobby</c> on specified <c>LobbyInvite</c>
        /// </summary>
        /// <param name="lobbyInvite">Specified invite to accept</param>
        /// <param name="enablePresence">Presence Enabled if <c>true</c></param>
        /// <param name="AcceptLobbyInviteCompleted">Callback when join lobby is completed</param>
        public void AcceptLobbyInvite(LobbyInvite lobbyInvite, bool enablePresence, OnCreateLobbyCallback AcceptLobbyInviteCompleted)
        {
            if (lobbyInvite != null && lobbyInvite.IsValid())
            {
                Debug.Log("Lobbies (AcceptLobbyInvite): Accepted invite, joining lobby.");

                JoinLobby(lobbyInvite.Lobby.Id, lobbyInvite.LobbyInfo, enablePresence, AcceptLobbyInviteCompleted);
            }
            else
            {
                Debug.LogError("Lobbies (AcceptLobbyInvite): lobbyInvite parameter is null or invalid!");

                AcceptLobbyInviteCompleted(Epic.OnlineServices.Result.InvalidState);
            }
        }

        #endregion
    }
}
