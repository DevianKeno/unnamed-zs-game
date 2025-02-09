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

using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.P2P;

namespace UZSG.EOS
{
    public partial class EOSTransportManager : IEOSSubManager
    {
        /// <summary>
        /// Connection data, socket name must be unique within an individual remote peer.
        /// </summary>
        public class Connection : IEquatable<Connection>
        {
            /// <summary>
            /// The ID of the socket for this Connection.
            /// </summary>
            public SocketId SocketId = new SocketId();

            /// <summary>
            /// The name of the socket used by this Connection.
            /// </summary>
            public string SocketName { get => SocketId.SocketName; set => SocketId.SocketName = value; }

            /// <summary>
            /// If the outgoing (local) side of the connection has been opened.
            /// </summary>
            public bool OpenedOutgoing = false;

            /// <summary>
            /// If the incoming (remote) side of the connection has been opened.
            /// </summary>
            public bool OpenedIncoming = false;

            /// <summary>
            /// If we are waiting on the remote side of the connection to confirm.
            /// </summary>
            public bool IsPendingOutgoing { get => IsValid && (OpenedOutgoing && !OpenedIncoming); }
            /// <summary>
            /// If the remote side of the connection is awaiting a connection accept response.
            /// </summary>
            public bool IsPendingIncoming { get => IsValid && (!OpenedOutgoing && OpenedIncoming); }

            /// <summary>
            /// If the connection has been opened on at least one end (local or remote).
            /// </summary>
            public bool IsHalfOpened { get => IsValid && (OpenedOutgoing || OpenedIncoming); }
            /// <summary>
            /// If the connection has been opened on both the local and remote ends.
            /// </summary>
            public bool IsFullyOpened { get => IsValid && (OpenedOutgoing && OpenedIncoming); }

            /// <summary>
            /// Has the user been given the chance to handle the connection open event?
            /// </summary>
            public bool ConnectionOpenedHandled = false;

            /// <summary>
            /// Has the user been given the chance to handle the connection closed event?
            /// </summary>
            public bool ConnectionClosedHandled = false;

            ushort CurrentPacketIndex = 0;

            /// <summary>
            /// By design we don't re-use this Connection data structure after the connection lifecycle is complete.
            /// </summary>
            public bool IsValid = false;

            /// <summary>
            /// Creates a Connection with no initial information.
            /// </summary>
            public Connection() { }

            /// <summary>
            /// Creates a Connection with a given named socket.
            /// </summary>
            /// <param name="socketName">The name of the socket to use.</param>
            public Connection(string socketName) { SocketName = socketName; }

            /// <summary>
            /// Gets the ID to use for the next outgoing message on this connection.
            /// Note: The index is updated after each call, and it is expected that the value given will be used.
            /// </summary>
            /// <returns>The index to use for the next message.</returns>
            public ushort GetNextMessageIndex() { return CurrentPacketIndex++; }

            /// <summary>
            /// Gets the hash code for the socket.
            /// </summary>
            /// <returns>The hash code for this Connection's socket.</returns>
            public override int GetHashCode()
            {
                return SocketName.GetHashCode();
            }

            /// <summary>
            /// Checks if a given object is the same as this Connection.
            /// </summary>
            /// <param name="obj">The object to compare to.</param>
            /// <returns><c>true</c> if the objects match, <c>false</c> if not.</returns>
            public override bool Equals(object obj)
            {
                return Equals(obj as Connection);
            }

            /// <summary>
            /// Checks if the given socket name matches this Connection.
            /// </summary>
            /// <param name="socketName">The name of the socket to check.</param>
            /// <returns><c>true</c> if the socket name matches, <c>false</c> if not.</returns>
            public bool Equals(string socketName)
            {
                return SocketName == socketName;
            }

            /// <summary>
            /// Checks if a given Connection object is the same as this Connection.
            /// </summary>
            /// <param name="connection">The Connection to compare to.</param>
            /// <returns><c>true</c> if the Connections match, <c>false</c> if not.</returns>
            public bool Equals(Connection connection)
            {
                return SocketName == connection.SocketName;
            }

            /// <summary>
            /// Provides a JSON formatted debug string for this connection.
            /// </summary>
            /// <returns>The JSON formatted debug string containing the socket ID and socket name.</returns>
            public string DebugStringJSON()
            {
                return string.Format("{{\"SocketName\": {1}\"}}", SocketName);
            }
        }
    }
}
