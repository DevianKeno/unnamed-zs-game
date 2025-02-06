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
using System.IO;
using System.Linq;
using System.Text;

using Unity.Netcode;
using UnityEngine;

using Newtonsoft.Json;

using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using PlayEveryWare.EpicOnlineServices;

using UZSG.EOS.P2P;
using UZSG.EOS.Lobbies;

using UZSG.Saves;

namespace UZSG.EOS
{
    /// <summary>
    /// Simplified wrapper for EOS [P2P Interface](https://dev.epicgames.com/docs/services/en-US/Interfaces/P2P/index.html).
    /// </summary>
    public partial class EOSPeer2PeerManager : IEOSSubManager
    {
        const bool OnlyAcceptPacketsFromServer = true;
        EOSTransport transport;
        P2PInterface p2pInterface;
        bool _isActive;
        bool _isChatDataCacheDirty;
        Dictionary<ProductUserId, ChatWithFriendData> _chatDataCache;

        ProductUserId serverUserId;

        [Header("Debugging")]
        [SerializeField] bool enableDebugging = false;

        public EOSPeer2PeerManager()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
            p2pInterface = Game.EOS.GetEOSP2PInterface();
            transport = NetworkManager.Singleton.GetComponent<EOSTransport>();
            _chatDataCache = new Dictionary<ProductUserId, ChatWithFriendData>();
            _isChatDataCacheDirty = true;
        }

#if UNITY_EDITOR
        ~EOSPeer2PeerManager()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            onRequestWorldDataCompleted = null;
            onRequestPlayerSaveDataCompleted = null;
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

        public bool GetChatDataCache(out Dictionary<ProductUserId, ChatWithFriendData> chatDataCache)
        {
            chatDataCache = this._chatDataCache;
            return _isChatDataCacheDirty;
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

        /// <summary>
        /// Custom packet handling logic.
        /// </summary>
        /// <param name="userId">Sender</param>
        /// <param name="payload"></param>
        internal void HandlePacket(ProductUserId userId, byte[] payload)
        {
            if (!transport.IsServer &&
                OnlyAcceptPacketsFromServer &&
                transport.TryGetClientIdMapping(userId, out var clientId) &&
                clientId != transport.ServerClientId)
            {
                return;
            }
            
            Packet packet;
            try
            {
                packet = Packet.FromBytes(payload);
            }
            catch (Exception ex)
            {
                Game.Console.LogDebug($"Error deserializing packet from user:[{userId}]. Discarding...");
                Debug.LogException(ex);
                return;
            }

            switch (packet.Type)
            {
                case PacketType.Request:
                {
                    HandleRequestPackets(userId, packet);
                    break;
                }
                case PacketType.ChatMessage:
                {
                    ReceiveChatMessage(userId, packet);
                    break;
                }
            }
        }

        void HandleRequestPackets(ProductUserId senderId, Packet packet)
        {
            switch (packet.Subtype)
            {
                case PacketSubtype.RequestWorldData:
                {
                    HandleWorldDataRequestPacket(senderId, packet);
                    break;
                }
                case PacketSubtype.ReceiveWorldDataHeading:
                {
                    HandleWorldDataHeadingPacket(senderId, packet);
                    break;
                }
                case PacketSubtype.ReceiveWorldDataChunk:
                {
                    HandleWorldDataChunkPacket(senderId, packet);
                    break;
                }
                case PacketSubtype.ReceiveWorldDataFooter:
                {
                    HandleWorldDataFooterPacket(senderId, packet);
                    break;
                }
                case PacketSubtype.RequestPlayerSaveData:
                {
                    HandlePlayerSaveDataRequestPacket(senderId, packet);
                    break;
                }
                case PacketSubtype.ReceivePlayerSaveData:
                {
                    HandlePlayerSaveDataReceivedPacket(senderId, packet);
                    break;
                }
                case PacketSubtype.SpawnMyPlayer:
                {
                    HandleSpawnMyPlayerRequestPacket(senderId, packet);
                    break;
                }
                case PacketSubtype.SpawnMyPlayerCompleted:
                {
                    HandleSpawnMyPlayerCompletedPacket(senderId, packet);
                    break;
                }
            }
        }

        void HandleSpawnMyPlayerRequestPacket(ProductUserId senderId, Packet packet)
        {
            Game.World.CurrentWorld.SpawnPlayer_ServerMethod(senderId);

            var packetToSend = new Packet()
            {
                Type = PacketType.Request,
                Subtype = PacketSubtype.SpawnMyPlayerCompleted,
                Data = new byte[]{ 1 }, /// one represents success
            };
            EOSSubManagers.Transport.SendPacket(
                senderId,
                EOSTransport.DEFAULT_SOCKET_NAME,
                packetToSend.ToBytes(),
                allowDelayedDelivery: true,
                reliability: PacketReliability.ReliableUnordered);
        }

        void HandleSpawnMyPlayerCompletedPacket(ProductUserId senderId, Packet packet)
        {
            onRequestSpawnPlayerCompleted?.Invoke();
            onRequestSpawnPlayerCompleted = null;
        }

        List<Packet> _receivedWorldDataChunkPackets = new();
        void HandleWorldDataRequestPacket(ProductUserId userId, Packet packet)
        {
            Game.Console.LogDebug($"Received world data request from user: [{userId}]");
            /// NOTE: testing absolute path 
            var testpath = Path.Combine(Application.persistentDataPath, WorldManager.WORLDS_FOLDER, "alpha", "level.dat");

            if (File.Exists(testpath))
            {
                SendWorldData(testpath, userId);
            }
            else
            {
                Game.Console.LogDebug($"User[{userId}] tried to request world data at filepath: '{testpath}', but world does not exists! Ignoring request...");
            }
        }

        void HandleWorldDataHeadingPacket(ProductUserId userId, Packet packet)
        {
            try
            {
                var content = Encoding.UTF8.GetString(packet.Data);
                var tokens = content.Split(",");
                string worldName = tokens[0].Split(":")[1]; 
                string filesize = tokens[1].Split(":")[1];
                
                Game.Console.LogDebug($"Received world from user: [{userId}], [{worldName}],  size: {filesize} bytes");
                _receivedWorldDataChunkPackets = new();
            }
            catch
            {
                Game.Console.LogDebug($"Received invalid world data heading from user: [{userId}]");
            }
        }

        void HandleWorldDataChunkPacket(ProductUserId userId, Packet packet)
        {
            Game.Console.LogDebug($"Received world data chunk from: {userId}, chunk size: {packet.Data.Count} bytes");
            _receivedWorldDataChunkPackets.Add(packet);
        }

        void HandleWorldDataFooterPacket(ProductUserId userId, Packet packet)
        {
            var worldDataBytes = AssembleWorldDataFromPackets(_receivedWorldDataChunkPackets);
            var worldSaveData = JsonConvert.DeserializeObject<WorldSaveData>(Encoding.UTF8.GetString(worldDataBytes));
            var filepath = Game.World.ConstructWorldFromExternal(worldSaveData);
            _receivedWorldDataChunkPackets.Clear();
            
            onRequestWorldDataCompleted?.Invoke(filepath);
            onRequestWorldDataCompleted = null;
        }

        void HandlePlayerSaveDataRequestPacket(ProductUserId userId, Packet deserializedPacket)
        {
            Game.Console.LogDebug($"Received player data request from user[{userId}]");

            var currentWorld = Game.World.CurrentWorld;
            byte[] playerData;
            if (currentWorld.CheckIfPlayerHasSave(userId))
            {
                playerData = Game.World.CurrentWorld.GetPlayerDataBytesFromUID(userId.ToString());
                Game.Console.LogDebug($"Sending player data to user[{userId}]");
            }
            else
            {
                Game.Console.LogDebug($"No save data found for user[{userId}]");
                playerData = Encoding.UTF8.GetBytes("empty");
            }

            var packet = new Packet()
            {
                Type = PacketType.Request,
                Subtype = PacketSubtype.ReceivePlayerSaveData,
                Data = playerData,
            };
            EOSSubManagers.Transport.SendPacket(
                userId,
                EOSTransport.DEFAULT_SOCKET_NAME,
                packet.ToBytes(),
                allowDelayedDelivery: true,
                reliability: PacketReliability.ReliableUnordered);
        }

        void HandlePlayerSaveDataReceivedPacket(ProductUserId userId, Packet packet)
        {
            Game.Console.LogDebug($"Received player data from host: user[{userId}]");

            var contents = Encoding.UTF8.GetString(packet.Data);
            PlayerSaveData psd = PlayerSaveData.Empty;
            if (!contents.Equals("empty"))
            {
                psd = JsonConvert.DeserializeObject<PlayerSaveData>(contents);
            }

            onRequestPlayerSaveDataCompleted?.Invoke(psd, Epic.OnlineServices.Result.Success);
            onRequestPlayerSaveDataCompleted = null;
        }


        #region Chat messages

        public void SendChatMessage(string username, string message)
        {
            if (EOSSubManagers.Lobbies.FindMemberByDisplayName(username, out LobbyMember member))
            {
                var packet = new Packet()
                {
                    Type = PacketType.ChatMessage,
                    Data = Encoding.UTF8.GetBytes(message),
                };
                EOSSubManagers.Transport.SendPacket(
                    member.ProductUserId,
                    EOSTransport.DEFAULT_SOCKET_NAME,
                    packet.ToBytes(),
                    allowDelayedDelivery: true,
                    reliability: PacketReliability.ReliableUnordered);
            }
            else
            {
                Game.Console.LogInfo($"Unable to deliver message to target user.");
            }
        }

        public void ReceiveChatMessage(ProductUserId userId, Packet packet)
        {
            if (EOSSubManagers.UserInfo.TryGetUserInfoByProductId(userId, out var userInfo))
            {
                Game.Console.LogInfo($"Received new message from {userInfo.DisplayName}: ");
                var message = Encoding.UTF8.GetString(packet.Data);
                Game.Console.LogInfo($"\t{message}");
            }
            else
            {
                // EOSSubManagers.UserInfo.Query
            }
        }

        #endregion


        #region World data transfer

        event OnRequestWorldDataCompleted onRequestWorldDataCompleted;
        public delegate void OnRequestWorldDataCompleted(string filepath);

        /// <summary>
        /// Requests world save data from target player (the host).
        /// </summary>
        /// <param name="userId">Target user to request data from</param>
        /// <param name="onCompleted"><c>string</c>: the resulting filepath of received world dat file on where it is saved.</param>
        public void RequestWorldSaveData(ProductUserId userId, OnRequestWorldDataCompleted onCompleted = null)
        {
            onRequestWorldDataCompleted += onCompleted;

            var packet = new Packet()
            {
                Type = PacketType.Request,
                Subtype = PacketSubtype.RequestWorldData,
                Data = Encoding.UTF8.GetBytes("world please"),
            };
            EOSSubManagers.Transport.SendPacket(
                userId,
                EOSTransport.DEFAULT_SOCKET_NAME,
                packet.ToBytes(),
                allowDelayedDelivery: true,
                reliability: PacketReliability.ReliableUnordered);
            Game.Console.LogDebug("Requesting world data...");
        }

        /// <summary>
        /// Send the world 'level.dat' to target user.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="targetId"></param>
        void SendWorldData(string filepath, ProductUserId targetId)
        {
            var worldDataBytes = File.ReadAllBytes(filepath);
            var saveData = Game.World.Deserialize(filepath);

            /// Send heading first with metadata
            var headingPacket = new Packet()
            {
                Type = PacketType.Request,
                Subtype = PacketSubtype.ReceiveWorldDataHeading,
                Data = Encoding.UTF8.GetBytes($"worldname:{saveData.WorldName},size:{worldDataBytes.Length}"),
            };
            EOSSubManagers.Transport.SendPacket(
                targetId,
                EOSTransport.DEFAULT_SOCKET_NAME,
                headingPacket.ToBytes(),
                allowDelayedDelivery: true,
                reliability: PacketReliability.ReliableOrdered);

            var worldDataPacket = new Packet()
            {
                Type = PacketType.Request,
                Subtype = PacketSubtype.ReceiveWorldDataChunk,
                Data = worldDataBytes,
            };
            Game.Console.LogDebug($"Sending world data to user: [{targetId}]");
            EOSSubManagers.Transport.SendPacket(
                targetId,
                EOSTransport.DEFAULT_SOCKET_NAME,
                worldDataPacket.ToBytes(),
                allowDelayedDelivery: true,
                reliability: PacketReliability.ReliableOrdered);
            Game.Console.LogDebug($"Sent world data to user: [{targetId}]");

            Game.Console.LogDebug($"Sending world data footer...");
            /// Send world data footer
            var footerPacket = new Packet()
            {
                Type = PacketType.Request,
                Subtype = PacketSubtype.ReceiveWorldDataFooter,
                Data = Encoding.UTF8.GetBytes("footer")
            };
            EOSSubManagers.Transport.SendPacket(
                targetId,
                EOSTransport.DEFAULT_SOCKET_NAME,
                footerPacket.ToBytes(),
                allowDelayedDelivery: true,
                reliability: PacketReliability.ReliableOrdered);
            Game.Console.LogDebug($"World data send finally completed...");
        }

        byte[] AssembleWorldDataFromPackets(List<Packet> packets)
        {
            int totalSize = packets.Sum(p => p.Data.Count);
            byte[] result = new byte[totalSize];
            int offset = 0;

            foreach (Packet packet in packets)
            {
                Buffer.BlockCopy(packet.Data.ToArray(), 0, result, offset, packet.Data.Count);
                offset += packet.Data.Count;
            }

            return result;
        }
        
        #endregion


        #region PlayerSaveData transfer

        event OnRequestPlayerSaveDataCallback onRequestPlayerSaveDataCompleted;
        public delegate void OnRequestPlayerSaveDataCallback(PlayerSaveData playerSaveData, Epic.OnlineServices.Result result);

        /// <summary>
        /// TODO: add a timeout
        /// </summary>
        /// <param name="targetUserId">Target user to request from. Usually is the lobby owner.</param>
        /// <param name="onCompleted"></param>
        public void RequestPlayerSaveData(ProductUserId targetUserId, OnRequestPlayerSaveDataCallback onCompleted = null)
        {
            onRequestPlayerSaveDataCompleted += onCompleted;
            
            var packet = new Packet()
            {
                Type = PacketType.Request,
                Subtype = PacketSubtype.RequestPlayerSaveData,
                Data = new byte[1],
            };
            EOSSubManagers.Transport.SendPacket(
                targetUserId,
                EOSTransport.DEFAULT_SOCKET_NAME,
                packet.ToBytes(),
                allowDelayedDelivery: true,
                reliability: PacketReliability.ReliableUnordered);
        }

        event OnRequestSpawnPlayerCallback onRequestSpawnPlayerCompleted;
        public delegate void OnRequestSpawnPlayerCallback();
        /// <summary>
        /// Request for our player to be spawned in the server.
        /// </summary>
        /// <param name="userId">Target user id to request to</param>
        /// <param name="onCompleted">Callback</param>
        public void RequestSpawnPlayer(ProductUserId userId, OnRequestSpawnPlayerCallback onCompleted)
        {
            onRequestSpawnPlayerCompleted += onCompleted;

            var packet = new Packet()
            {
                Type = PacketType.Request,
                Subtype = PacketSubtype.SpawnMyPlayer,
                Data = new byte[1],
            };
            EOSSubManagers.Transport.SendPacket(
                userId,
                EOSTransport.DEFAULT_SOCKET_NAME,
                packet.ToBytes(),
                allowDelayedDelivery: true,
                reliability: PacketReliability.ReliableUnordered);
        }

        #endregion
    }
}