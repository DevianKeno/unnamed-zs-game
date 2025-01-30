/*
* Copyright (c) 2021 PlayEveryWare
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System;
using System.Collections.Generic;

using UnityEngine;
using Unity.Netcode;

using Epic.OnlineServices;
using Epic.OnlineServices.Auth;

using UZSG.Systems;

namespace UZSG.EOS
{
    public class EOSTransport : NetworkTransport
    {
        // EOS Data
        EOSTransportManager p2pManager;
        public const string DEFAULT_SOCKET_NAME = "EOSP2PTransport"; /// Socket names must only contain 1-32 alphanumeric characters.

        // True post-initialization and pre-shutdown, else false
        bool IsInitialized = false;

        // Our local EOS UserId (should always be valid after successful initialization)
        ProductUserId OurUserId { get => LocalUserIdOverride ?? Game.EOS?.GetProductUserId(); }

        /// <summary>
        /// Invalid ClientId
        /// </summary>
        /// <value><c>-1</c></value>
        public const ulong InvalidClientId = ulong.MaxValue;

        // Client ID Maps (locally persistent)
        ulong NextClientId = 1; // (ServerClientId is 0, so we start at 1)
        Dictionary<ulong, ProductUserId> _clientIdToUserId = new();
        Dictionary<ProductUserId, ulong> _userIdToClientId = new();


        // True if we're the Server, else we're a Client
        public bool IsServer { get; private set; } = false;
        public bool IsRunning { get; private set; } = false;
        /// <summary>
        /// UserID of the Server host to connect to on StartClient.
        /// This is required to be set by the user before calling StartClient
        /// </summary>
        public ProductUserId ServerUserIdToConnectTo = null;

        // Locked in after calling StartClient to avoid changing unexpectedly
        ProductUserId serverUserId = null;
        public ProductUserId ServerUserId => serverUserId;

        // Override local user id for testing multiple clients at once
        public ProductUserId LocalUserIdOverride = null;

// #if UNITY_EDITOR
//         //editor field to input a PUID to connect to(ease of access when using editor buttons in the network manager
//         public string ServerUserIdToConnectToInput = null;

//         //String of the current server PUID for copying purposes
//         public string ServerUserIDForCopying = null;
// #endif
        /// <summary>
        /// A constant `clientId` that represents the server.
        /// When this value is found in methods such as `Send`, it should be treated as a placeholder that means "the server".
        /// </summary>
        public override ulong ServerClientId => 0;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:NetworkTransport"/> is supported in the current runtime context.
        /// This is used by multiplex adapters.
        /// </summary>
        /// <value><c>true</c> if is supported; otherwise, <c>false</c>.</value>
        public override bool IsSupported => true; // TODO: Do an EOS SDK platform support check? (eg. Is PC?) Not super critical right now.

        // Cached User Connected/Disconnected Events
        // UserID, ClientID, True if Connected event else Disconnected event
        Queue<Tuple<ProductUserId, ulong, bool>> ConnectedDisconnectedUserEvents = new();

        [System.Diagnostics.Conditional("EOS_TRANSPORT_DEBUG")]
        void print(string msg)
        {
            Debug.Log(msg);
        }

        [System.Diagnostics.Conditional("EOS_TRANSPORT_DEBUG")]
        void printWarning(string msg)
        {
            Debug.LogWarning(msg);
        }

        [System.Diagnostics.Conditional("EOS_TRANSPORT_DEBUG")]
        void printError(string msg)
        {
            Debug.LogError(msg);
        }

        /// <summary>
        /// Send a payload to the specified clientId, data and channelName.
        /// </summary>
        /// <param name="clientId">The clientId to send to.</param>
        /// <param name="payload">The data to send.</param>
        /// <param name="networkDelivery">The delivery type (QoS) to send data with.</param>
        public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
        {
            if (!IsInitialized) return;

            if (!TryGetProductUserIdMapping(clientId, out ProductUserId userId))
            {
                printError($"EOSP2PTransport.Send: Unable to send payload - clientId [{clientId}] does not exist within the connection.");
                return;
            }

            print($"EOSP2PTransport.Send: [ClientId='{clientId}', UserId='{userId}', PayloadBytes='{payload.Count}', SendTimeSec='{Time.realtimeSinceStartup}']");

            Epic.OnlineServices.P2P.PacketReliability reliability = Epic.OnlineServices.P2P.PacketReliability.ReliableOrdered;
            if (networkDelivery == NetworkDelivery.Unreliable)
            {
                reliability = Epic.OnlineServices.P2P.PacketReliability.UnreliableUnordered;
            }
            else if (networkDelivery == NetworkDelivery.Reliable)
            {
                reliability = Epic.OnlineServices.P2P.PacketReliability.ReliableUnordered;
            }
            else if(networkDelivery == NetworkDelivery.ReliableFragmentedSequenced || networkDelivery == NetworkDelivery.ReliableSequenced)
            {
                reliability = Epic.OnlineServices.P2P.PacketReliability.ReliableOrdered;
            }

            if (payload.Count > EOSTransportManager.MaxPacketSize)
            {
                if (reliability != Epic.OnlineServices.P2P.PacketReliability.ReliableOrdered)
                {
                    printError($"EOSP2PTransport.Send: Unable to send payload - The payload size ({payload.Count} bytes) exceeds the maxmimum packet size supported by EOS P2P ({EOSTransportManager.MaxPacketSize} bytes).");
                    return;
                }
            }

            p2pManager.SendPacket(userId, DEFAULT_SOCKET_NAME, payload, 0, false, reliability);
        }

        public void Send(ulong clientId, ArraySegment<byte> payload, byte channel = 0, bool allowDelayedDelivery = false, NetworkDelivery networkDelivery = NetworkDelivery.Reliable)
        {
            if (!IsInitialized) return;

            if (!TryGetProductUserIdMapping(clientId, out ProductUserId userId))
            {
                printError($"EOSP2PTransport.Send: Unable to send payload - clientId [{clientId}] does not exist within the connection.");
                return;
            }

            print($"EOSP2PTransport.Send: [ClientId='{clientId}', UserId='{userId}', PayloadBytes='{payload.Count}', SendTimeSec='{Time.realtimeSinceStartup}']");

            Epic.OnlineServices.P2P.PacketReliability reliability = Epic.OnlineServices.P2P.PacketReliability.ReliableOrdered;
            if (networkDelivery == NetworkDelivery.Unreliable)
            {
                reliability = Epic.OnlineServices.P2P.PacketReliability.UnreliableUnordered;
            }
            else if (networkDelivery == NetworkDelivery.Reliable)
            {
                reliability = Epic.OnlineServices.P2P.PacketReliability.ReliableUnordered;
            }
            else if(networkDelivery == NetworkDelivery.ReliableFragmentedSequenced || networkDelivery == NetworkDelivery.ReliableSequenced)
            {
                reliability = Epic.OnlineServices.P2P.PacketReliability.ReliableOrdered;
            }

            if (payload.Count > EOSTransportManager.MaxPacketSize)
            {
                if (reliability != Epic.OnlineServices.P2P.PacketReliability.ReliableOrdered)
                {
                    printError($"EOSP2PTransport.Send: Unable to send payload - The payload size ({payload.Count} bytes) exceeds the maxmimum packet size supported by EOS P2P ({EOSTransportManager.MaxPacketSize} bytes).");
                    return;
                }
            }

            p2pManager.SendPacket(userId, DEFAULT_SOCKET_NAME, payload, channel, allowDelayedDelivery, reliability);
        }

        /// <summary>
        /// Polls for incoming events, with an extra output parameter to report the precise time the event was received.
        /// </summary>
        /// <param name="clientId">The clientId this event is for.</param>
        /// <param name="payload">The incoming data payload.</param>
        /// <param name="receiveTime">The time the event was received, as reported by Time.realtimeSinceStartup.</param>
        /// <returns>Returns the event type.</returns>
        public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
        {
            Debug.Assert(IsInitialized, "Transport is not initialized");

            // Any users connected or disconnected?
            if (ConnectedDisconnectedUserEvents.Count > 0)
            {
                Tuple<ProductUserId, ulong, bool> evnt = ConnectedDisconnectedUserEvents.Dequeue();
                ProductUserId evntUserId = evnt.Item1;
                ulong evntClientId = evnt.Item2;
                bool evntIsConnectionEvent = evnt.Item3;

                clientId = evntClientId;
                
                payload = new ArraySegment<byte>();
                receiveTime = Time.realtimeSinceStartup;
                NetworkEvent networkEventType = evntIsConnectionEvent ? NetworkEvent.Connect : NetworkEvent.Disconnect;
                print($"EOSP2PTransport.PollEvent: [{networkEventType}, ClientId='{clientId}', UserId='{evntUserId}', PayloadBytes='{payload.Count}', RecvTimeSec='{receiveTime}']");
                return networkEventType;
            }

            // Any packets to be received?
            if (p2pManager.TryReceivePacket(out ProductUserId userId, out string socketName, out byte channel, out byte[] packet))
            {
                Debug.Assert(socketName == DEFAULT_SOCKET_NAME);

                if (TryGetClientIdMapping(userId, out clientId))
                {
                    payload = new ArraySegment<byte>(packet);
                    receiveTime = Time.realtimeSinceStartup;
                    print($"EOSP2PTransport.PollEvent: [{NetworkEvent.Data}, ClientId='{clientId}', UserId='{userId}', PayloadBytes='{payload.Count}', RecvTimeSec='{receiveTime}']");
                    return NetworkEvent.Data;
                }
            }

            // Otherwise, nothing to report
            clientId = InvalidClientId;
            payload = new ArraySegment<byte>();
            receiveTime = 0;
            print("EOSP2PTransport.PollEvent: []");
            return NetworkEvent.Nothing;
        }

        /// <summary>
        /// Starts up in client mode and connects to the server.
        /// </summary>
        public override bool StartClient()
        {
// #if UNITY_EDITOR
//             if (!string.IsNullOrEmpty(ServerUserIdToConnectToInput))
//             {
//                 ServerUserIdToConnectTo = ProductUserId.FromString(ServerUserIdToConnectToInput);
//             }
// #endif
            if (ServerUserIdToConnectTo == null)
            {
                print("EOSP2PTransport.StartClient: No ServerUserIDToConnectTo set!");
                return false;
            }

            Debug.Assert(IsInitialized);

            bool result;

            // Set client mode
            IsServer = false;

            // User provided a valid ServerUserIdToConnectTo?
            if (result = (ServerUserIdToConnectTo != null && ServerUserIdToConnectTo.IsValid()))
            {
                // Store it in ServerUserId so it can't be changed after start up
                serverUserId = ServerUserIdToConnectTo;

                // Attempt to connect to the server hosted by ServerUserId - was the request successfully initiated?
                if (result = p2pManager.OpenConnection(serverUserId, DEFAULT_SOCKET_NAME))
                {
                    print($"EOSP2PTransport.StartClient: Successful Client start up - REQUESTED outgoing '{DEFAULT_SOCKET_NAME}' socket connection with Server UserId Server UserId='{serverUserId}'.");
                    result = true;
                }
                else
                {
                    printError($"EOSP2PTransport.StartClient: Failed Client start up - Unable to initiate a connect request with Server UserId='{serverUserId}'.");
                }
            }
            else
            {
                printError("EOSP2PTransport.StartClient: Failed Client start up - 'ServerUserIdToConnectTo' is null or invalid."
                    + " Please set a valid EOS ProductUserId of the Server host this Client should try connecting to in the 'ServerUserIdToConnectTo' property before calling StartClient"
                    + $" (ServerUserIdToConnectTo='{ServerUserIdToConnectTo}').");
            }

            IsRunning = result;
            return result;
        }

        /// <summary>
        /// Starts up in server mode and begins listening for incoming clients.
        /// </summary>
        public override bool StartServer()
        {
            Debug.Assert(IsInitialized);
            print($"EOSP2PTransport.StartServer: Entering Server mode with EOS UserId='{OurUserId}'.");
// #if UNITY_EDITOR
//             ServerUserIDForCopying = OurUserId.ToString();
// #endif
            serverUserId = OurUserId;
            // Set server mode
            IsServer = true;

            // Success
            IsRunning = true;
            return true;
        }

        /// <summary>
        /// Disconnects a client from the server.
        /// </summary>
        /// <param name="clientId">The clientId to disconnect.</param>
        public override void DisconnectRemoteClient(ulong clientId)
        {
            Debug.Assert(IsInitialized);
            Debug.Assert(IsServer);

            if (!TryGetProductUserIdMapping(clientId, out ProductUserId userId))
            {
                printError($"EOSP2PTransport.DisconnectRemoteClient: Unable to disconnect client - clientId [{clientId}] does not exist within the connection.");
                return;
            }

            print($"EOSP2PTransport.DisconnectRemoteClient: Disconnecting ClientId='{clientId}' (UserId='{userId}') from our Server.");
            p2pManager.CloseConnection(userId, DEFAULT_SOCKET_NAME, true);
        }

        /// <summary>
        /// Disconnects the local client from the server.
        /// </summary>
        public override void DisconnectLocalClient()
        {
            Debug.Assert(IsInitialized);
            Debug.Assert(IsServer == false);

            print($"EOSP2PTransport.DisconnectLocalClient: Disconnecting our Client from the Server (UserId='{serverUserId}').");
            p2pManager.CloseConnection(serverUserId, DEFAULT_SOCKET_NAME, true);
            IsRunning = false;
        }

        /// <summary>
        /// Gets the round trip time for a specific client.
        /// This method is optional, and not currently implemented in this case.
        /// </summary>
        /// <param name="clientId">The clientId to get the RTT from.</param>
        /// <returns><c>0</c></returns>
        public override ulong GetCurrentRtt(ulong clientId)
        {
            Debug.Assert(IsInitialized);
            /*
             * RTT can be calculated by subtracting the time at which
             * DateTime.Now is sent from the time at which it is returned from
             * the opponent.
             *
             * You can implement it by sending DateTime in your request, and
             * subsequently subtracting that value from DateTime.Now when the
             * response is received.
             *
             * It is not currently implemented due to the complexity it would
             * add to the samples.
             */
            return 0;
        }

        /// <summary>
        /// Shuts down the transport.
        /// </summary>
        public override void Shutdown()
        {
            Debug.Assert(IsInitialized);
            print("EOSP2PTransport.Shutdown: Shutting down Epic Online Services Peer-2-Peer NetworkTransport.");
            IsRunning = false;
            IsInitialized = false;

            // Shutdown EOS Peer-2-Peer Manager
            if (p2pManager != null)
            {
                p2pManager.Shutdown();
                p2pManager.OnIncomingConnectionRequestedCb = null;
                p2pManager.OnConnectionOpenedCb = null;
                p2pManager.OnConnectionClosedCb = null;
                p2pManager = null;
            }

            // Clear user connects/disconnects event cache
            ConnectedDisconnectedUserEvents = null;

            // Clear ID maps
            _clientIdToUserId = null;
            _userIdToClientId = null;

            // Clear Server UserId target
            serverUserId = null;
        }
        
        /// <summary>
        /// Initializes the transport.
        /// </summary>
        public override void Initialize(NetworkManager networkManager)
        {
            // Debug.Assert(IsInitialized == false);
            print("EOSP2PTransport.Initialize: Initializing Epic Online Services Peer-2-Peer NetworkTransport.");

            /// EOSManager should already be initialized and exist by this point
            if (Game.EOS == null)
            {
                printError("EOSP2PTransport.Initialize: Unable to initialize - EOSManager singleton is null (has the EOSManager component been added to an object in your initial scene?)");
                return;
            }

            /// Create ID maps
            NextClientId = 1; // Reset local client ID assignment counter
            _clientIdToUserId = new Dictionary<ulong, ProductUserId>();
            _userIdToClientId = new Dictionary<ProductUserId, ulong>();

            /// Create user connects/disconnects event cache
            ConnectedDisconnectedUserEvents = new Queue<Tuple<ProductUserId, ulong, bool>>();

            /// Initialize EOS Peer-2-Peer Manager
            Debug.Assert(p2pManager == null);

            if (LocalUserIdOverride?.IsValid() == true)
            {
                p2pManager = new EOSTransportManager(LocalUserIdOverride);
            }
            else
            {
                p2pManager = EOSSubManagers.Transport;
            }
            
            p2pManager.OnIncomingConnectionRequestedCb = OnIncomingConnectionRequestedCallback;
            p2pManager.OnConnectionOpenedCb = OnConnectionOpenedCallback;
            p2pManager.OnConnectionClosedCb = OnConnectionClosedCallback;
            if (p2pManager.Initialize() == false)
            {
                printError("EOSP2PTransport.Initialize: Unable to initialize - EOSP2PManager failed to initialize.");
                p2pManager.OnIncomingConnectionRequestedCb = null;
                p2pManager.OnConnectionOpenedCb = null;
                p2pManager.OnConnectionClosedCb = null;
                p2pManager = null;
                return;
            }

            IsInitialized = true;
        }

        /// <summary>
        /// Leaves any existing lobbies, shuts down the network manager, and logs the local user out of EOS.
        /// </summary>
        /*public void ForceLogout()
        {
            EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>().LeaveLobby(null);

            NetworkManager.Singleton.Shutdown();

            EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(), (LogoutCallbackInfo data) =>
            {
                if (data.ResultCode == Result.Success)
                {
                    print("Logout Successful. [" + data.ResultCode + "]");
                    GameStateManager.ChangeScene("MainMenu", false);
                }
            });
        }*/

        /// <summary>
        /// Called when a connection is originally requested by a remote peer (opened remotely but not yet locally).
        /// </summary>
        /// <param name="userId">The id of the remote peer requesting a connection.</param>
        /// <param name="socketName">The name of the socket the requested connection is on.</param>
        public void OnIncomingConnectionRequestedCallback(ProductUserId userId, string socketName)
        {
            Debug.Assert(IsInitialized);

            // For now if we're a server we'll just accept all incoming requests trying to establish an "EOSP2PTransport" socket connection.
            // TODO: Add more intelligent logic here (limiting the maximum number of players on a server, etc.)
            if (IsServer && socketName == DEFAULT_SOCKET_NAME)
            {
                // Accept connection request
                print($"EOSP2PTransport.OnIncomingConnectionRequestedCallback: ACCEPTING incoming '{socketName}' socket connection request from UserId='{userId}'.");
                p2pManager.OpenConnection(userId, socketName);
            }
            else
            {
                // Reject connection request
                print($"EOSP2PTransport.OnIncomingConnectionRequestedCallback: REJECTING incoming '{socketName}' socket connection request from UserId='{userId}'.");
                p2pManager.CloseConnection(userId, socketName);
            }
        }

        /// <summary>
        /// Called immediately after a remote peer connection becomes fully opened (opened both locally and remotely).
        /// </summary>
        /// <param name="userId">The id of the remote peer that has been connected to.</param>
        /// <param name="socketName">The name of the socket the newly opened connection is on.</param>
        public void OnConnectionOpenedCallback(ProductUserId userId, string socketName)
        {
            Debug.Assert(IsInitialized);

            print($"EOSP2PTransport.OnConnectionOpenedCallback: '{socketName}' socket connection OPENED with UserId='{userId}'.");
            if (socketName == DEFAULT_SOCKET_NAME)
            {
                if (IsServer)
                {
                    // We don't have this client in our map yet? (ie. We haven't seen them before)
                    if (_userIdToClientId.ContainsKey(userId) == false)
                    {
                        // Add client ID mapping
                        ulong newClientId = NextClientId++; // Generate new client ID (locally unique, incremental)
                        _clientIdToUserId.Add(newClientId, userId);
                        _userIdToClientId.Add(userId, newClientId);
                    }
                }

                // Get mapped client ID
                if (TryGetClientIdMapping(userId, out ulong clientId))
                {
                    // Cache user connection event, will be returned in a later PollEvent call
                    ConnectedDisconnectedUserEvents.Enqueue(new Tuple<ProductUserId, ulong, bool>(userId, clientId, true));
                }
            }
        }

        // Called immediately before a fully opened remote peer connection is closed (closed either locally or remotely).
        // Guaranteed to be called only if OnConnectionOpenedCallback was attempted to be called earlier for any given connection.

        /// <summary>
        /// Called immediately before a fully opened remote peer connection is closed (closed either locally or remotely).
        /// Guaranteed to be called only if OnConnectionOpenedCallback was attempted to be called earlier for any given connection.
        /// </summary>
        /// <param name="userId">The id of the remote peer that will be disconnected from.</param>
        /// <param name="socketName">The name of the socket to close the connection on.</param>
        public void OnConnectionClosedCallback(ProductUserId userId, string socketName)
        {
            Debug.Assert(IsInitialized);

            print($"EOSP2PTransport.OnConnectionClosedCallback: '{socketName}' socket connection CLOSED with UserId='{userId}'.");
            if (socketName == DEFAULT_SOCKET_NAME)
            {
                // We're the Server?
                if (IsServer)
                {
                    // We should have seen this client before in a prior call to OnConnectionOpenedCallback
                    Debug.Assert(_userIdToClientId.ContainsKey(userId) == true);
                }

                // Get mapped client ID
                if (TryGetClientIdMapping(userId, out ulong clientId))
                {
                    // NOTE: For simplicity of event processing order and ID lookups we will simply allow the client ID map
                    // to continually grow as we don't expect to receive an unreasonable number (>10k) of unique
                    // user connections during the lifetime of the host application.

                    // if (IsServer)
                    // {
                    //   // Remove client ID mapping
                    //   ulong clientId = UserIdToClientId[userId];
                    //   ClientIdToUserId.Remove(clientId);
                    //   UserIdToClientId.Remove(userId);
                    // }

                    // Cache user disconnection event, will be returned in a later PollEvent call
                    ConnectedDisconnectedUserEvents.Enqueue(new Tuple<ProductUserId, ulong, bool>(userId, clientId, false));
                }
            }
        }

        /// <summary>
        /// Set the server user id to connect to.
        /// </summary>
        public void SetConnection(ProductUserId userId)
        {
            if (userId == null || !userId.IsValid())
            {
                Game.Console.LogError("Invalid user to connect to!");
                return;
            }
            
            ServerUserIdToConnectTo = userId;
        }

        /// <summary>
        /// Returns the ProductUserId corresponding to a given ClientId
        /// </summary>
        public bool TryGetProductUserIdMapping(ulong clientId, out ProductUserId userId)
        {
            Debug.Assert(IsInitialized);

            if (clientId == 0uL)
            {
                userId = serverUserId;
                return true;
            }

            if (IsServer)
            {
                return _clientIdToUserId.TryGetValue(clientId, out userId);
            }
            else // we're a client
            {
                Debug.AssertFormat(clientId == ServerClientId, "EOSP2PTransport.GetUserId: Unexpected ClientId='{0}' given - We're a Client so we should only be dealing with the Server by definition (Server ClientId='{1}').",
                                   clientId, ServerClientId);
                userId = serverUserId;
                return true;
            }
        }

        /// <summary>
        /// Returns the ClientId corresponding to a given ProductUserId
        /// </summary>
        public bool TryGetClientIdMapping(ProductUserId userId, out ulong clientId)
        {
            Debug.Assert(IsInitialized);

            if (userId == serverUserId)
            {
                clientId = ServerClientId;
                return true;
            }

            if (IsServer)
            {
                return _userIdToClientId.TryGetValue(userId, out clientId);
            }
            else // we're a client
            {
                Debug.AssertFormat(userId == serverUserId, "EOSP2PTransport.GetClientId: Unexpected UserId='{0}' given - We're a Client so we should only be dealing with the Server by definition (Server UserId='{1}').",
                                   userId, serverUserId);
                clientId = ServerClientId;
                return true;
            }
        }
    }
}