using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using PlayEveryWare.EpicOnlineServices;

using UZSG.EOS.P2P;
using UZSG.Systems;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Linq;
using UZSG.Saves;

namespace UZSG.EOS
{
    /// <summary>
    /// Simplified wrapper for EOS [P2P Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/P2P/index.html).
    /// </summary>
    public partial class EOSPeer2PeerManager : IEOSSubManager
    {
        const int WORLD_DATA_CHUNK_SIZE_BYTES = 4096;
        
        P2PInterface P2PHandle => Game.EOS.GetEOSP2PInterface();

        ulong ConnectionNotificationId;
        Dictionary<ProductUserId, ChatWithFriendData> ChatDataCache;
        bool ChatDataCacheDirty;

        public UIPeer2PeerParticleController ParticleController;
        public Transform parent;

        [Header("Debugging")]
        [SerializeField] bool enableDebugging = false;

#if UNITY_EDITOR
        void OnPlayModeChanged(UnityEditor.PlayModeStateChange modeChange)
        {
            if (modeChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                //prevent attempts to call native EOS code while exiting play mode, which crashes the editor
                // P2PHandle = null;
            }
        }
#endif

        public EOSPeer2PeerManager()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif

            // P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();

            ChatDataCache = new Dictionary<ProductUserId, ChatWithFriendData>();
            ChatDataCacheDirty = true;
        }

#if UNITY_EDITOR
        ~EOSPeer2PeerManager()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }
#endif

        public bool GetChatDataCache(out Dictionary<ProductUserId, ChatWithFriendData> ChatDataCache)
        {
            ChatDataCache = this.ChatDataCache;
            return ChatDataCacheDirty;
        }

        void RefreshNATType()
        {
            var options = new QueryNATTypeOptions();
            P2PHandle.QueryNATType(ref options, null, OnRefreshNATTypeFinished);
        }

        public NATType GetNATType()
        {
            var options = new GetNATTypeOptions();
            Result result = P2PHandle.GetNATType(ref options, out NATType natType);

            if (result == Result.NotFound)
            {
                return NATType.Unknown;
            }

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("EOS P2PNAT GetNatType: error while retrieving NAT Type: {0}", result);
                return NATType.Unknown;
            }

            return natType;
        }

        internal void Update()
        {
            HandleReceivedPackets();
        }


        #region Event callbacks

        void OnLoggedIn()
        {
            RefreshNATType();

            SubscribeToConnectionRequest();
        }

        void OnLoggedOut()
        {
            UnsubscribeFromConnectionRequests();
        }

        void OnRefreshNATTypeFinished(ref OnQueryNATTypeCompleteInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("P2P (OnRefreshNATTypeFinished): data is null");
            //    return;
            //}

            if (data.ResultCode != Result.Success)
            {
                Debug.LogErrorFormat("P2p (OnRefreshNATTypeFinished): RefreshNATType error: {0}", data.ResultCode);
                return;
            }

            Debug.Log("P2p (OnRefreshNATTypeFinished): RefreshNATType Completed");
        }

        #endregion


        const int MAX_RECEIVE_PACKET_SIZE_BYTES = 8192;
        public void HandleReceivedPackets()
        {
            if (!Game.Main.IsOnline) return;

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

            P2PHandle.GetNextReceivedPacketSize(ref receivedPacketSizeOptions, out uint nextPacketSizeBytes);
            if (nextPacketSizeBytes == 0) return;

            var dataBytes = new byte[nextPacketSizeBytes];
            var dataSegment = new ArraySegment<byte>(dataBytes);
            ProductUserId peerId = null;
            SocketId socketId = default;
            Result result = P2PHandle.ReceivePacket(ref receivePacketOptions, ref peerId, ref socketId, out byte outChannel, dataSegment, out uint bytesWritten);

            if (result == Result.Success)
            {
                if (enableDebugging) Debug.Log($"Received packet from: peerId={peerId}, socketId={socketId}");

                if (!peerId.IsValid())
                {
                    if (enableDebugging) Debug.LogErrorFormat("EOS P2PNAT HandleReceivedMessages: ProductUserId peerId is not valid!");
                    return;
                }

                Packet deserializedPacket;
                try
                {
                    deserializedPacket = JsonConvert.DeserializeObject<Packet>(Encoding.UTF8.GetString(dataBytes));
                }
                catch
                {
                    Debug.LogError($"Encountered an error when deserializing packet from: peerId={peerId}, socketId={socketId}");
                    return;
                }
                
                switch (deserializedPacket.Type)
                {
                    case PacketType.ChatMessage:
                    {
                        HandleChatPacket(peerId, deserializedPacket);
                        break;
                    }
                    case PacketType.WorldDataRequest:
                    {
                        HandleWorldDataRequestPacket(peerId, deserializedPacket);
                        break;
                    }
                    case PacketType.WorldDataHeading:
                    {
                        HandleWorldSendDataHeadingPacket(peerId, deserializedPacket);
                        break;
                    }
                    case PacketType.WorldDataChunk:
                    {
                        HandleWorldDataChunkPacket(peerId, deserializedPacket);
                        break;
                    }
                    case PacketType.WorldDataFooter:
                    {
                        HandleWorldDataFooterPacket(peerId, deserializedPacket);
                        break;
                    }
                }
            }
            else if (result == Result.NotFound)
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


        #region World data transfer

        event Action<string> onRequestWorldDataCompleted;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="onCompleted"><c>string</c> is filepath of received "level.dat"</param>
        public void RequestWorldData(ProductUserId fromUserId, Action<string> onCompleted = null)
        {
            onRequestWorldDataCompleted += onCompleted;

            var packet = new Packet()
            {
                Type = PacketType.WorldDataHeading,
                Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject("world please")),
            };
            var socketId = new SocketId() { SocketName = "WORLD_DATA" };
            var sendPacketOptions = new SendPacketOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                RemoteUserId = fromUserId,
                SocketId = socketId,
                AllowDelayedDelivery = true,
                Channel = 0,
                Reliability = PacketReliability.ReliableOrdered,
                Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(packet))
            };

            Debug.Log("Requesting world data...");
            Result result = P2PHandle.SendPacket(ref sendPacketOptions);
            if (result != Result.Success) return;
        }

        void SendWorldData(string filepath, ProductUserId targetId)
        {
            string worldDataBytes = File.ReadAllText(filepath);
            int packetCount = Mathf.CeilToInt(worldDataBytes.Length / (float) WORLD_DATA_CHUNK_SIZE_BYTES);
            
            /// Send heading first with metadata
            var headingPacket = new Packet()
            {
                Type = PacketType.WorldDataHeading,
                Data = Encoding.UTF8.GetBytes($"world_{packetCount}"),
            };
            var socketId = new SocketId() { SocketName = "WORLD_DATA" };
            var headerPacketOptions = new SendPacketOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                RemoteUserId = targetId,
                SocketId = socketId,
                AllowDelayedDelivery = true,
                Channel = 0,
                Reliability = PacketReliability.ReliableOrdered,
                Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(headingPacket))
            };

            Result result = P2PHandle.SendPacket(ref headerPacketOptions);
            if (result != Result.Success) return;

            /// Send world data chunks
            using var memoryStream = new MemoryStream();

            for (int i = 0; i < packetCount && i < 65536; i++)
            {
                int offset = i * WORLD_DATA_CHUNK_SIZE_BYTES;
                int chunkSize = Mathf.Min(WORLD_DATA_CHUNK_SIZE_BYTES, worldDataBytes.Length - offset);
                string data = worldDataBytes[offset..chunkSize];
             
                var packet = new Packet()
                {
                    Type = PacketType.WorldDataChunk,
                    Data = Encoding.UTF8.GetBytes(data),
                };
                byte[] packetBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(packet));
                
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

                result = P2PHandle.SendPacket(ref worldDataPacketOptions);

                if (result == Result.Success)
                {
                    Debug.Log("EOS P2PNAT SendMessage: Message successfully sent to user.");
                }
                else
                {
                    Debug.LogError($"EOS P2PNAT SendMessage: error while sending data: {result}");
                    return;
                }
            }
            
            /// Send world data footer
            var footerPacket = new Packet()
            {
                Type = PacketType.WorldDataFooter,
                Data = Encoding.UTF8.GetBytes($"TotalPackets:{packetCount}")
            };
            var footerPacketOptions = new SendPacketOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                RemoteUserId = targetId,
                SocketId = socketId,
                AllowDelayedDelivery = true,
                Channel = 0,
                Reliability = PacketReliability.ReliableOrdered,
                Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(footerPacket))
            };
            result = P2PHandle.SendPacket(ref footerPacketOptions);
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

        #region Messages transfer

        public void SendMessage(ProductUserId friendId, MessageData message)
        {
            if (!friendId.IsValid())
            {
                Debug.LogError("EOS P2PNAT SendMessage: bad input data: account id is wrong.");
                return;
            }
            if (message.Type == MessageType.TextMessage)
            {
                if (string.IsNullOrEmpty(message.TextData))
                {
                    Debug.LogError("EOS P2PNAT SendMessage: bad input data message is empty.");
                    return;
                }

                // Update Cache
                ChatEntry chatEntry = new()
                {
                    IsOwnEntry = true,
                    Message = message.TextData
                };

                if (ChatDataCache.TryGetValue(friendId, out ChatWithFriendData chatData))
                {
                    chatData.ChatLines.Enqueue(chatEntry);
                    ChatDataCacheDirty = true;
                }
                else
                {
                    var newChatData = new ChatWithFriendData()
                    {
                        FriendId = friendId
                    };
                    newChatData.ChatLines.Enqueue(chatEntry);

                    ChatDataCache.Add(friendId, newChatData);
                    ChatDataCacheDirty = true;
                }

                // Send Message
                SocketId socketId = new SocketId()
                {
                    SocketName = "CHAT"
                };

                SendPacketOptions options = new SendPacketOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = friendId,
                    SocketId = socketId,
                    AllowDelayedDelivery = true,
                    Channel = 0,
                    Reliability = PacketReliability.ReliableOrdered,
                    Data = new ArraySegment<byte>(Encoding.UTF8.GetBytes("t" + message.TextData))
                };

                Result result = P2PHandle.SendPacket(ref options);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("EOS P2PNAT SendMessage: error while sending data, code: {0}", result);
                    return;
                }

                Debug.Log("EOS P2PNAT SendMessage: Message successfully sent to user.");
            }

            else if (message.Type == MessageType.CoordinatesMessage)
            {

                string rawData = ("m" + message.PositionX.ToString() + "," + message.PositionY.ToString());
                // Send Message
                SocketId socketId = new SocketId()
                {
                    SocketName = "CHAT"
                };

                SendPacketOptions options = new SendPacketOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = friendId,
                    SocketId = socketId,
                    AllowDelayedDelivery = true,
                    Channel = 0,
                    Reliability = PacketReliability.ReliableOrdered,
                    Data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(rawData))
                };

                Result result = P2PHandle.SendPacket(ref options);

                if (result != Result.Success)
                {
                    Debug.LogErrorFormat("EOS P2PNAT SendMessage: error while sending data, code: {0}", result);
                    return;
                }
            }

            else
            {
                Debug.Log("EOS P2PNAT SendMessage: Message content was not valid.");
            }
        }

        #endregion

        ProductUserId HandleChatPacket(ProductUserId peerId, Packet packet)
        {
            var message = Convert.ToBase64String(packet.Data);
            if (message.StartsWith("t"))
            {
                ChatEntry newMessage = new()
                {
                    IsOwnEntry = false,
                    Message = message[1..]
                };

                if (ChatDataCache.TryGetValue(peerId, out ChatWithFriendData chatData))
                {
                    // Update existing chat
                    chatData.ChatLines.Enqueue(newMessage);

                    ChatDataCacheDirty = true;
                    return peerId;
                }
                else
                {
                    var newChat = new ChatWithFriendData()
                    {
                        FriendId = peerId
                    };
                    newChat.ChatLines.Enqueue(newMessage);
                    /// New Chat Request
                    ChatDataCache.Add(peerId, newChat);
                    return peerId;
                }
            }
            else if (message.StartsWith("m"))
            {
                message = message[1..];

                string[] coords = message.Split(',');
                int xPos = int.Parse(coords[0]);
                int yPos = int.Parse(coords[1]);
                Debug.Log("EOS P2PNAT HandleReceivedMessages:  Mouse position Recieved at " + xPos + ", " + yPos);

                ParticleController.SpawnParticles(xPos, yPos, parent);

                return peerId;
            }
            else
            {
                // Debug.LogErrorFormat("EOS P2PNAT HandleReceivedMessages: error while reading data, code: {0}", result);
                return null;
            }
        }

        void HandleWorldDataRequestPacket(ProductUserId userId, Packet packet)
        {
            if (!Game.World.IsInWorld) return;
            
            var savepath = Game.World.CurrentWorld.GetPath();
            SendWorldData(savepath, userId);
        }

        List<Packet> _receivedWorldDataChunkPackets = new();
        void HandleWorldSendDataHeadingPacket(ProductUserId userId, Packet packet)
        {
            if (packet.Type != PacketType.WorldDataHeading) return;
            Debug.Log($"Received WorldSendDataHeading from: {userId}, world size: {packet.Data.Length} bytes");
            _receivedWorldDataChunkPackets = new();
        }


        void HandleWorldDataChunkPacket(ProductUserId userId, Packet packet)
        {
            if (packet.Type != PacketType.WorldDataChunk) return;
            Debug.Log($"Received World Data Chunk from: {userId}, chunk size: {packet.Data.Length} bytes");
        
            switch (packet.Type)
            {
                case PacketType.WorldDataHeading:
                {
                    break;
                }
                case PacketType.WorldDataChunk:
                {
                    _receivedWorldDataChunkPackets.Add(packet);
                    break;
                }
                case PacketType.WorldDataFooter:
                {
                    break;
                }
            }
        }

        void SubscribeToConnectionRequest()
        {
            if (ConnectionNotificationId == 0)
            {
                SocketId socketId = new SocketId()
                {
                    SocketName = "CHAT"
                };

                AddNotifyPeerConnectionRequestOptions options = new AddNotifyPeerConnectionRequestOptions()
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    SocketId = socketId
                };

                ConnectionNotificationId = P2PHandle.AddNotifyPeerConnectionRequest(ref options, null, OnIncomingConnectionRequest);
                if (ConnectionNotificationId == 0)
                {
                    Debug.Log("EOS P2PNAT SubscribeToConnectionRequests: could not subscribe, bad notification id returned.");
                }
            }
        }

        void UnsubscribeFromConnectionRequests()
        {
            if (ConnectionNotificationId != 0)//check to prevent warnings when done unnecessarily during p2p startup
            {
                P2PHandle.RemoveNotifyPeerConnectionRequest(ConnectionNotificationId);
                ConnectionNotificationId = 0;
            }
        }

        void OnIncomingConnectionRequest(ref OnIncomingConnectionRequestInfo data)
        {
            //if (data == null)
            //{
            //    Debug.LogError("P2P (OnIncomingConnectionRequest): data is null");
            //    return;
            //}

            if (!(bool)data.SocketId?.SocketName.Equals("CHAT"))
            {
                Debug.LogError("P2p (OnIncomingConnectionRequest): bad socket id");
                return;
            }

            SocketId socketId = new SocketId()
            {
                SocketName = "CHAT"
            };

            AcceptConnectionOptions options = new AcceptConnectionOptions()
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = data.RemoteUserId,
                SocketId = socketId
            };

            Result result = P2PHandle.AcceptConnection(ref options);

            if (result != Result.Success)
            {
                Debug.LogErrorFormat("P2p (OnIncomingConnectionRequest): error while accepting connection, code: {0}", result);
            }
        }
    }
}