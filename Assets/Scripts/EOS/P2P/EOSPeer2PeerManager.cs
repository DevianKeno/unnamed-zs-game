using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Newtonsoft.Json;

using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using PlayEveryWare.EpicOnlineServices;

using UZSG.EOS.P2P;
using UZSG.Systems;
using UZSG.Saves;

namespace UZSG.EOS
{
    /// <summary>
    /// Simplified wrapper for EOS [P2P Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/P2P/index.html).
    /// </summary>
    public partial class EOSPeer2PeerManager : IEOSSubManager, IAuthInterfaceEventListener, IConnectInterfaceEventListener
    {
        const int MAX_PACKET_SIZE_BYTES = 1170;
        const int WORLD_DATA_CHUNK_SIZE_BYTES = 1024;
        const string DEFAULT_SOCKET_NAME = "UZSG_P2P";
        
        P2PInterface p2pInterface;
        bool _isActive;
        ulong _connectionNotificationId;
        Dictionary<ProductUserId, ChatWithFriendData> ChatDataCache;
        bool ChatDataCacheDirty;

        public UIPeer2PeerParticleController ParticleController;
        public Transform parent;

        [Header("Debugging")]
        [SerializeField] bool enableDebugging = false;

        public EOSPeer2PeerManager()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
            p2pInterface = Game.EOS.GetEOSP2PInterface();
            ChatDataCache = new Dictionary<ProductUserId, ChatWithFriendData>();
            ChatDataCacheDirty = true;
        }

#if UNITY_EDITOR
        ~EOSPeer2PeerManager()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }
#endif

#if UNITY_EDITOR
        void OnPlayModeChanged(UnityEditor.PlayModeStateChange modeChange)
        {
            _isActive = false;
            if (modeChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                /// Prevent attempts to call native EOS code while exiting play mode, which crashes the editor
                p2pInterface = null;
            }
        }
#endif

        public bool GetChatDataCache(out Dictionary<ProductUserId, ChatWithFriendData> ChatDataCache)
        {
            ChatDataCache = this.ChatDataCache;
            return ChatDataCacheDirty;
        }

        public NATType GetNATType()
        {
            var options = new GetNATTypeOptions();
            Epic.OnlineServices.Result result = p2pInterface.GetNATType(ref options, out NATType natType);

            if (result == Epic.OnlineServices.Result.NotFound)
            {
                return NATType.Unknown;
            }

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("EOS P2PNAT GetNatType: error while retrieving NAT Type: {0}", result);
                return NATType.Unknown;
            }

            return natType;
        }

        public void Update()
        {
            if (!_isActive) return;

            HandleReceivedPackets();
        }

        public void OnAuthLogin(Epic.OnlineServices.Auth.LoginCallbackInfo info)
        {
            /// NOTE: Idk really know which is used when logging if its either Auth or Connect
            /// but I'm team connect for now
            
            // if (info.ResultCode == Result.Success)
            // {
            //      Game.Main.AddUpdateCoListener((Game.IUpdateCoListener) this);
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
            RefreshNATType();
            SubscribeToConnectionRequest();
            _isActive = true;
        }

        void DeinitializeLoggedOut()
        {
            UnsubscribeFromConnectionRequests();
        }

        void RefreshNATType()
        {
            var options = new QueryNATTypeOptions();
            p2pInterface.QueryNATType(ref options, null, OnRefreshNATTypeFinished);
        }

        void OnRefreshNATTypeFinished(ref OnQueryNATTypeCompleteInfo info)
        {
            if (info.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Game.Console.LogDebug($"P2p (OnRefreshNATTypeFinished): RefreshNATType error: {info.ResultCode}");
                return;
            }

            Game.Console.LogDebug($"P2p (OnRefreshNATTypeFinished): RefreshNATType Completed");
        }
        
        const int MAX_RECEIVE_PACKET_SIZE_BYTES = 8192;
        public void HandleReceivedPackets()
        {
            var receivePacketOptions = new ReceivePacketOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                MaxDataSizeBytes = MAX_RECEIVE_PACKET_SIZE_BYTES,
                RequestedChannel = null
            };
            var receivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                RequestedChannel = null
            };

            p2pInterface.GetNextReceivedPacketSize(ref receivedPacketSizeOptions, out uint nextPacketSizeBytes);
            if (nextPacketSizeBytes == 0) return;

            var dataBytes = new byte[nextPacketSizeBytes];
            var dataSegment = new ArraySegment<byte>(dataBytes);
            ProductUserId peerId = null;
            SocketId socketId = default;
            Epic.OnlineServices.Result result = p2pInterface.ReceivePacket(ref receivePacketOptions, ref peerId, ref socketId, out byte outChannel, dataSegment, out uint bytesWritten);

            if (result == Epic.OnlineServices.Result.Success)
            {
                if (enableDebugging) Debug.Log($"Received packet from: peerId={peerId}, socketId={socketId}");

                if (!peerId.IsValid())
                {
                    if (enableDebugging) Debug.LogErrorFormat("EOS P2PNAT HandleReceivedMessages: ProductUserId peerId is not valid!");
                    return;
                }

                var packet = new Packet()
                {
                    Type = dataBytes[0],
                    Data = dataBytes[1..],
                };
                var packetType = (PacketType) packet.Type;
                switch (packetType)
                {
                    case PacketType.ChatMessage:
                    {
                        // HandleChatPacket(peerId, deserializedPacket);
                        break;
                    }
                    case PacketType.WorldDataRequest:
                    {
                        HandleWorldDataRequestPacket(peerId, packet);
                        break;
                    }
                    case PacketType.WorldDataHeading:
                    {
                        HandleWorldDataHeadingPacket(peerId, packet);
                        break;
                    }
                    case PacketType.WorldDataChunk:
                    {
                        HandleWorldDataChunkPacket(peerId, packet);
                        break;
                    }
                    case PacketType.WorldDataFooter:
                    {
                        HandleWorldDataFooterPacket(peerId, packet);
                        break;
                    }
                    case PacketType.PlayerSaveDataRequest:
                    {
                        HandlePlayerDataRequestPacket(peerId, packet);
                        break;
                    }
                    case PacketType.PlayerSaveDataReceived:
                    {
                        HandlePlayerSaveDataReceivedPacket(peerId, packet);
                        break;
                    }
                }
            }
            else if (result == Epic.OnlineServices.Result.NotFound)
            {
                return;
            }
        }
        void HandleWorldDataFooterPacket(ProductUserId userId, Packet packet)
        {
            var dataBytes = AssembleWorldDataFromPackets(_receivedWorldDataChunkPackets);
            var worldSaveData = JsonConvert.DeserializeObject<WorldSaveData>(Encoding.UTF8.GetString(dataBytes));
            var filepath = Game.World.ConstructWorldFromExternal(worldSaveData);
            _receivedWorldDataChunkPackets.Clear();
            onRequestWorldDataCompleted?.Invoke(filepath);
            onRequestWorldDataCompleted = null;
        }

        void HandlePlayerDataRequestPacket(ProductUserId userId, Packet deserializedPacket)
        {
            Game.Console.LogDebug($"Received player data request from: user[{userId}]");

            var playerData = Game.World.CurrentWorld.GetPlayerDataFromUID(userId.ToString());
            var packet = new Packet()
            {
                Type = (byte) PacketType.PlayerSaveDataRequest,
                Data = playerData,
            };
            var socketId = new SocketId() { SocketName = DEFAULT_SOCKET_NAME };
            var sendPacketOptions = new SendPacketOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                RemoteUserId = userId,
                SocketId = socketId,
                AllowDelayedDelivery = true,
                Channel = 0,
                Reliability = PacketReliability.ReliableOrdered,
                Data = packet.ToBytes()
            };

            Epic.OnlineServices.Result result = p2pInterface.SendPacket(ref sendPacketOptions);
            if (result == Epic.OnlineServices.Result.Success)
            {
                Game.Console.LogDebug($"Sending player data to user[{userId}]...");
            }
            else
            {
                Game.Console.LogDebug($"Failed to send player data to user[{userId}]: [{result}]");
            }
        }

        void HandlePlayerSaveDataReceivedPacket(ProductUserId userId, Packet packet)
        {
            Game.Console.LogDebug($"Received player data from: user[{userId}]");

            onRequestPlayerSaveDataCompleted?.Invoke(packet.Data, Epic.OnlineServices.Result.Success);
            onRequestPlayerSaveDataCompleted = null;
        }


        #region World data transfer

        event Action<string> onRequestWorldDataCompleted;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="onCompleted"><c>string</c> is filepath of received world dat file</param>
        public void RequestWorldData(ProductUserId fromUserId, Action<string> onCompleted = null)
        {
            onRequestWorldDataCompleted += onCompleted;

            var packet = new Packet()
            {
                Type = (byte) PacketType.WorldDataRequest,
                Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject("world please")),
            };
            var socketId = new SocketId() { SocketName = DEFAULT_SOCKET_NAME };
            var sendPacketOptions = new SendPacketOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                RemoteUserId = fromUserId,
                SocketId = socketId,
                AllowDelayedDelivery = true,
                Channel = 0,
                Reliability = PacketReliability.ReliableOrdered,
                Data = packet.ToBytes()
            };

            Epic.OnlineServices.Result result = p2pInterface.SendPacket(ref sendPacketOptions);
            if (result == Epic.OnlineServices.Result.Success)
            {
                Game.Console.LogDebug("Requesting world data...");
            }
        }

        void SendWorldData(string filepath, ProductUserId targetId)
        {
            var worldDataBytes = File.ReadAllBytes(filepath);
            int packetCount = Mathf.CeilToInt(worldDataBytes.Length / (float) WORLD_DATA_CHUNK_SIZE_BYTES);
            
            /// Send heading first with metadata
            var headingPacket = new Packet()
            {
                Type = (int) PacketType.WorldDataHeading,
                Data = Encoding.UTF8.GetBytes($"size:{worldDataBytes.Length}"),
            };
            var socketId = new SocketId() { SocketName = DEFAULT_SOCKET_NAME };
            var headerPacketOptions = new SendPacketOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                RemoteUserId = targetId,
                SocketId = socketId,
                AllowDelayedDelivery = true,
                Channel = 0,
                Reliability = PacketReliability.ReliableOrdered,
                Data = headingPacket.ToBytes()
            };

            Epic.OnlineServices.Result result = p2pInterface.SendPacket(ref headerPacketOptions);
            if (result != Epic.OnlineServices.Result.Success)
            {
                Game.Console.LogDebug($"Error sending packet: [{result}]");
                return;
            }
            Game.Console.LogDebug($"Sent world data header to user: [{targetId}]");
            Game.Console.LogDebug($"Begin send world data...");

            /// Send world data chunks
            for (int i = 0; i < packetCount; i++)
            {
                int offset = i * WORLD_DATA_CHUNK_SIZE_BYTES;
                int chunkSize = Mathf.Min(WORLD_DATA_CHUNK_SIZE_BYTES, worldDataBytes.Length - offset);

                using (var stream = new MemoryStream())
                {
                    stream.WriteByte((byte) PacketType.WorldDataChunk);
                    stream.Write(worldDataBytes, offset, chunkSize);
                    byte[] packetBytes = stream.ToArray();

                    var worldDataPacketOptions = new SendPacketOptions()
                    {
                        LocalUserId = Game.EOS.GetProductUserId(),
                        RemoteUserId = targetId,
                        SocketId = socketId,
                        AllowDelayedDelivery = true,
                        Channel = 0,
                        Reliability = PacketReliability.ReliableOrdered,
                        Data = packetBytes
                    };
                    result = p2pInterface.SendPacket(ref worldDataPacketOptions);
                }

                if (result == Epic.OnlineServices.Result.Success)
                {
                    Game.Console.LogDebug($"Sending world data chunk: [{i}/{packetCount}]");
                }
                else
                {
                    Game.Console.LogDebug($"Error sending packet: [{result}]");
                    return;
                }
            }
            Game.Console.LogDebug($"Send chunks complete");
            Game.Console.LogDebug($"Sending footer");

            /// Send world data footer
            var footerPacket = new Packet()
            {
                Type = (byte) PacketType.WorldDataFooter,
                Data = new byte[0]
            };
            var footerPacketOptions = new SendPacketOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                RemoteUserId = targetId,
                SocketId = socketId,
                AllowDelayedDelivery = true,
                Channel = 0,
                Reliability = PacketReliability.ReliableOrdered,
                Data = footerPacket.ToBytes()
            };
            result = p2pInterface.SendPacket(ref footerPacketOptions);
            if (result == Epic.OnlineServices.Result.Success)
            {
                Game.Console.LogDebug($"Sent footer to user: [{targetId}]");
            }
            Game.Console.LogDebug($"World data send completed");
        }

        byte[] AssembleWorldDataFromPackets(List<Packet> packets)
        {
            int totalSize = packets.Sum(p => p.Data.Length);
            byte[] result = new byte[totalSize];
            int offset = 0;

            foreach (Packet packet in packets)
            {
                Buffer.BlockCopy(packet.Data, 0, result, offset, packet.Data.Length);
                offset += packet.Data.Length;
            }

            return result;
        }
        
        #endregion


        #region PlayerSaveData transfer

        public delegate void OnRequestData(byte[] data, Epic.OnlineServices.Result result);
        public delegate void OnRequestPlayerSaveData(byte[] data, Epic.OnlineServices.Result result);
        event OnRequestPlayerSaveData onRequestPlayerSaveDataCompleted;
        public void RequestPlayerSaveData(ProductUserId userId, OnRequestPlayerSaveData onCompleted = null)
        {
            onRequestPlayerSaveDataCompleted += onCompleted;
            
            var packet = new Packet()
            {
                Type = (byte) PacketType.PlayerSaveDataRequest,
                Data = new byte[1],
            };
            var socketId = new SocketId() { SocketName = DEFAULT_SOCKET_NAME };
            var sendPacketOptions = new SendPacketOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                RemoteUserId = userId,
                SocketId = socketId,
                AllowDelayedDelivery = true,
                Channel = 0,
                Reliability = PacketReliability.ReliableOrdered,
                Data = packet.ToBytes()
            };

            Epic.OnlineServices.Result result = p2pInterface.SendPacket(ref sendPacketOptions);
            if (result == Epic.OnlineServices.Result.Success)
            {
                Game.Console.LogDebug($"Requesting player data...");
            }
        }
        
        #endregion


        #region Messages transfer

        // public void SendMessage(ProductUserId friendId, MessageData message)
        // {
        //     if (!friendId.IsValid())
        //     {
        //         Debug.LogError("EOS P2PNAT SendMessage: bad input data: account id is wrong.");
        //         return;
        //     }
        //     if (message.Type == MessageType.TextMessage)
        //     {
        //         if (string.IsNullOrEmpty(message.TextData))
        //         {
        //             Debug.LogError("EOS P2PNAT SendMessage: bad input data message is empty.");
        //             return;
        //         }

        //         // Update Cache
        //         ChatEntry chatEntry = new()
        //         {
        //             IsOwnEntry = true,
        //             Message = message.TextData
        //         };

        //         if (ChatDataCache.TryGetValue(friendId, out ChatWithFriendData chatData))
        //         {
        //             chatData.ChatLines.Enqueue(chatEntry);
        //             ChatDataCacheDirty = true;
        //         }
        //         else
        //         {
        //             var newChatData = new ChatWithFriendData()
        //             {
        //                 FriendId = friendId
        //             };
        //             newChatData.ChatLines.Enqueue(chatEntry);

        //             ChatDataCache.Add(friendId, newChatData);
        //             ChatDataCacheDirty = true;
        //         }

        //         // Send Message
        //         SocketId socketId = new SocketId()
        //         {
        //             SocketName = "CHAT"
        //         };

        //         SendPacketOptions options = new SendPacketOptions()
        //         {
        //             LocalUserId = EOSManager.Instance.GetProductUserId(),
        //             RemoteUserId = friendId,
        //             SocketId = socketId,
        //             AllowDelayedDelivery = true,
        //             Channel = 0,
        //             Reliability = PacketReliability.ReliableOrdered,
        //             Data = new ArraySegment<byte>(Encoding.UTF8.GetBytes("t" + message.TextData))
        //         };

        //         Epic.OnlineServices.Result result = p2pInterface.SendPacket(ref options);

        //         if (result != Epic.OnlineServices.Result.Success)
        //         {
        //             Debug.LogErrorFormat("EOS P2PNAT SendMessage: error while sending data, code: {0}", result);
        //             return;
        //         }

        //         Debug.Log("EOS P2PNAT SendMessage: Message successfully sent to user.");
        //     }

        //     else if (message.Type == MessageType.CoordinatesMessage)
        //     {

        //         string rawData = ("m" + message.PositionX.ToString() + "," + message.PositionY.ToString());
        //         // Send Message
        //         SocketId socketId = new SocketId()
        //         {
        //             SocketName = "CHAT"
        //         };

        //         SendPacketOptions options = new SendPacketOptions()
        //         {
        //             LocalUserId = EOSManager.Instance.GetProductUserId(),
        //             RemoteUserId = friendId,
        //             SocketId = socketId,
        //             AllowDelayedDelivery = true,
        //             Channel = 0,
        //             Reliability = PacketReliability.ReliableOrdered,
        //             Data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(rawData))
        //         };

        //         Epic.OnlineServices.Result result = p2pInterface.SendPacket(ref options);

        //         if (result != Epic.OnlineServices.Result.Success)
        //         {
        //             Debug.LogErrorFormat("EOS P2PNAT SendMessage: error while sending data, code: {0}", result);
        //             return;
        //         }
        //     }

        //     else
        //     {
        //         Debug.Log("EOS P2PNAT SendMessage: Message content was not valid.");
        //     }
        // }

        #endregion

        // ProductUserId HandleChatPacket(ProductUserId peerId, Packet packet)
        // {
        //     var message = Convert.ToBase64String(packet.Data);
        //     if (message.StartsWith("t"))
        //     {
        //         ChatEntry newMessage = new()
        //         {
        //             IsOwnEntry = false,
        //             Message = message[1..]
        //         };

        //         if (ChatDataCache.TryGetValue(peerId, out ChatWithFriendData chatData))
        //         {
        //             // Update existing chat
        //             chatData.ChatLines.Enqueue(newMessage);

        //             ChatDataCacheDirty = true;
        //             return peerId;
        //         }
        //         else
        //         {
        //             var newChat = new ChatWithFriendData()
        //             {
        //                 FriendId = peerId
        //             };
        //             newChat.ChatLines.Enqueue(newMessage);
        //             /// New Chat Request
        //             ChatDataCache.Add(peerId, newChat);
        //             return peerId;
        //         }
        //     }
        //     else if (message.StartsWith("m"))
        //     {
        //         message = message[1..];

        //         string[] coords = message.Split(',');
        //         int xPos = int.Parse(coords[0]);
        //         int yPos = int.Parse(coords[1]);
        //         Debug.Log("EOS P2PNAT HandleReceivedMessages:  Mouse position Recieved at " + xPos + ", " + yPos);

        //         ParticleController.SpawnParticles(xPos, yPos, parent);

        //         return peerId;
        //     }
        //     else
        //     {
        //         // Debug.LogErrorFormat("EOS P2PNAT HandleReceivedMessages: error while reading data, code: {0}", result);
        //         return null;
        //     }
        // }

        List<Packet> _receivedWorldDataChunkPackets = new();
        void HandleWorldDataRequestPacket(ProductUserId userId, Packet packet)
        {
            Game.Console.LogDebug($"Received world data request from user: [{userId}]");
            /// NOTE: testing absolute path 
            var testpath = Path.Combine(Application.persistentDataPath, "SavedWorlds", "alpha", "level.dat");
            SendWorldData(testpath, userId);
        }

        void HandleWorldDataHeadingPacket(ProductUserId userId, Packet packet)
        {
            var message = Encoding.UTF8.GetString(packet.Data).Split(":");

            Game.Console.LogDebug($"Received world data heading from user: [{userId}], world size: {message[1]} bytes");
            _receivedWorldDataChunkPackets = new();
        }

        void HandleWorldDataChunkPacket(ProductUserId userId, Packet packet)
        {
            Game.Console.LogDebug($"Received world data chunk from: {userId}, chunk size: {packet.Data.Length} bytes");
            _receivedWorldDataChunkPackets.Add(packet);
        }

        void SubscribeToConnectionRequest()
        {
            if (this._connectionNotificationId == 0)
            {
                var socketId = new SocketId()
                {
                    SocketName = DEFAULT_SOCKET_NAME
                };

                var options = new AddNotifyPeerConnectionRequestOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    SocketId = socketId
                };
                this._connectionNotificationId = p2pInterface.AddNotifyPeerConnectionRequest(ref options, null, OnIncomingConnectionRequest);
                
                if (_connectionNotificationId == 0)
                {
                    Debug.Log("EOS P2PNAT SubscribeToConnectionRequests: could not subscribe, bad notification id returned.");
                }
            }
        }

        void UnsubscribeFromConnectionRequests()
        {
            if (_connectionNotificationId != 0)//check to prevent warnings when done unnecessarily during p2p startup
            {
                p2pInterface.RemoveNotifyPeerConnectionRequest(_connectionNotificationId);
                _connectionNotificationId = 0;
            }
        }

        void OnIncomingConnectionRequest(ref OnIncomingConnectionRequestInfo data)
        {
            if (!(bool)data.SocketId?.SocketName.Equals(DEFAULT_SOCKET_NAME))
            {
                Game.Console.LogDebug($"[P2P/OnIncomingConnectionRequest()]: Bad socket id");
                return;
            }

            var socketId = new SocketId()
            {
                SocketName = DEFAULT_SOCKET_NAME
            };
            var options = new AcceptConnectionOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                RemoteUserId = data.RemoteUserId,
                SocketId = socketId
            };
            Epic.OnlineServices.Result result = p2pInterface.AcceptConnection(ref options);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Game.Console.LogDebug($"[P2P/OnIncomingConnectionRequest()]: Error while accepting connection, code: {result}");
            }
        }
    }
}