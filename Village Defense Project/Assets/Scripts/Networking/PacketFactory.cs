using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using System.Runtime.Serialization;

#region CHANGE LOG
// -- April     28, 2015 - Nathan Hanlan - Added the packet factory classs.
#endregion

namespace Gem
{
    namespace Networking
    {
        

        /// <summary>
        /// This class creates packets and their data memebrs and provide functions for getting data out of packets safetly.
        /// 
        /// naming convention:
        /// 
        /// Create [TYPE] Packet - Creates a packet of TYPE
        /// Get [TYPE] PacketData - Gets the data members of the packet of a TYPE
        /// </summary>
        public static class PacketFactory
        {
            public const int SERIALIZATION_VERSION = 1;

            /// <summary>
            /// Call this with each Create function to create a header.
            /// </summary>
            /// <param name="aType"></param>
            /// <param name="aFormatter"></param>
            /// <param name="aStream"></param>
            private static void CreateHeader(PacketType aType, BinaryFormatter aFormatter, MemoryStream aStream)
            {
                //Version/Type
                aFormatter.Serialize(aStream, SERIALIZATION_VERSION);
                aFormatter.Serialize(aStream, aType);
            }
            /// <summary>
            /// Call this with each Get function to check the header.
            /// </summary>
            /// <param name="aType"></param>
            /// <param name="aFormatter"></param>
            /// <param name="aStream"></param>
            /// <returns>Returns false if the header was bad data.</returns>
            private static bool CheckHeader(PacketType aType, BinaryFormatter aFormatter, MemoryStream aStream)
            {
                try
                {
                    //Version/Type
                    int version = (int)aFormatter.Deserialize(aStream);
                    PacketType type = (PacketType)aFormatter.Deserialize(aStream);

                    if (version != SERIALIZATION_VERSION)
                    {
                        DebugUtils.LogError(ErrorCode.INVALID_PACKET_VERSION);
                        return false;
                    }
                    if (type != aType)
                    {
                        DebugUtils.LogError(ErrorCode.INVALID_PACKET_TYPE);
                        return false;
                    }
                    return true;
                }
                catch(Exception aException)
                {
                    DebugUtils.LogException(aException);
                    return false;
                }
                    
                
            }

            //Authentication
            
            /// <summary>
            /// Creates a packet of the authentication type.
            /// </summary>
            /// <param name="aRequest">The request to serialize</param>
            /// <param name="aUsername">The username to serialize</param>
            /// <param name="aPassword">The password to serialize</param>
            /// <returns>Returns a null packet if any of the arguments are null.</returns>
            public static Packet CreateAuthenticationPacket(Request aRequest, string aUsername, string aPassword)
            {
                if(aRequest == null || string.IsNullOrEmpty(aUsername) || string.IsNullOrEmpty(aPassword))
                {
                    return null;
                }
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();

                CreateHeader(PacketType.Authentication,formatter,stream);
                //Request
                formatter.Serialize(stream, typeof(Request).Name);
                aRequest.Serialize(stream, formatter);
                //Username
                formatter.Serialize(stream, typeof(string).Name);
                formatter.Serialize(stream, aUsername);
                //Password
                formatter.Serialize(stream, typeof(string).Name);
                formatter.Serialize(stream, aPassword);

                return new Packet(stream.ToArray());
            }

            /// <summary>
            /// Gets authentication data from the packet data. 
            /// Fails and returns false under the following conditions.
            /// -InvalidVersion
            /// -InvalidType
            /// -Deserialization Exception thrown.
            /// </summary>
            /// <param name="aRequest">The request deserialized.</param>
            /// <param name="aUsername">The username deserialized.</param>
            /// <param name="aPassword">The password deserialized.</param>
            /// <returns>Returns true if successful, false otherwise.</returns>
            public static bool GetAuthenticationPacketData(Packet aPacket, out Request aRequest, out string aUsername, out string aPassword)
            {
                aRequest = Request.EMPTY;
                aUsername = string.Empty;
                aPassword = string.Empty;

                if(aPacket == null || aPacket.bytes == null || aPacket.bytes.Length == 0)
                {
                    DebugUtils.LogError(ErrorCode.BAD_PACKET);
                    return false;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(aPacket.bytes);
                string typename = string.Empty;

                try
                {
                    //Check Header
                    if(!CheckHeader(PacketType.Authentication,formatter,stream))
                    {
                        return false;
                    }

                    //Request
                    typename = (string)formatter.Deserialize(stream);
                    aRequest.Deserialize(stream, formatter);
                    //Username
                    typename = (string)formatter.Deserialize(stream);
                    aUsername = (string)formatter.Deserialize(stream);
                    //Password.
                    typename = (string)formatter.Deserialize(stream);
                    aPassword = (string)formatter.Deserialize(stream);
                    return true;
                }
                catch(Exception aException)
                {
                    DebugUtils.LogError("Failed to deserialize type " + typename);
                    DebugUtils.LogException(aException);
                    return false;
                }

            }

            //Authentication Status

            /// <summary>
            /// Creates an Authentication Status packet.
            /// </summary>
            /// <param name="aRequest"></param>
            /// <param name="aStatus"></param>
            /// <returns>Returns an Authentication Status Packet</returns>
            public static Packet CreateAuthenticationStatusPacket(Request aRequest, int aStatus)
            {
                if(aRequest == null)
                {
                    DebugUtils.ArgumentNull("aRequest");
                    return null;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();

                CreateHeader(PacketType.AuthenticationStatus, formatter, stream);

                //Request
                formatter.Serialize(stream, typeof(Request).Name);
                aRequest.Serialize(stream, formatter);
                //Status
                formatter.Serialize(stream, typeof(int).Name);
                formatter.Serialize(stream, aStatus);

                return new Packet(stream.ToArray());
            }

            /// <summary>
            /// Gets the data from a AuthenticationStatus Packet.
            /// </summary>
            /// <param name="aPacket"></param>
            /// <param name="aRequest"></param>
            /// <param name="aStatus"></param>
            /// <returns>Returns true if successful at getting the data, false otherwise.</returns>
            public static bool GetAuthenticationStatusPacketData(Packet aPacket, out Request aRequest, out int aStatus)
            {
                aRequest = Request.EMPTY;
                aStatus = NetworkStatus.ERROR;

                if (aPacket == null || aPacket.bytes == null || aPacket.bytes.Length == 0)
                {
                    DebugUtils.LogError(ErrorCode.BAD_PACKET);
                    return false;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(aPacket.bytes);
                string typename = string.Empty;

                try
                {
                    if(!CheckHeader(PacketType.AuthenticationStatus,formatter,stream))
                    {
                        return false;
                    }

                    //Request
                    typename = (string)formatter.Deserialize(stream);
                    aRequest.Deserialize(stream, formatter);
                    //Status
                    typename = (string)formatter.Deserialize(stream);
                    aStatus = (int)formatter.Deserialize(stream);

                    return true;
                }
                catch(Exception aException)
                {
                    DebugUtils.LogError("Failed to deserialize type: " + typename);
                    DebugUtils.LogException(aException);
                    return false;
                }

            }

            //ConnectionRequest

            public static Packet CreateConnectionRequestPacket(Request aRequest, NetworkUser aUser)
            {
                if(aRequest == null)
                {
                    DebugUtils.ArgumentNull("aRequest");
                    return null;
                }

                if(aUser == NetworkUser.BAD_USER)
                {
                    DebugUtils.InvalidArgument("aUser");
                    return null;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();

                //Version/Type
                CreateHeader(PacketType.ConnectionRequest, formatter, stream);
                //Request
                formatter.Serialize(stream, typeof(Request).Name);
                aRequest.Serialize(stream, formatter);
                //User
                formatter.Serialize(stream, typeof(NetworkUser).Name);
                aUser.Serialize(stream, formatter);
                return new Packet(stream.ToArray());
            }

            public static bool GetConnectionRequestPacketData(Packet aPacket, out Request aRequest, out NetworkUser aUser)
            {
                aRequest = Request.EMPTY;
                aUser = NetworkUser.BAD_USER;

                if(aPacket == null || aPacket.bytes == null || aPacket.bytes.Length == 0)
                {
                    DebugUtils.LogError(ErrorCode.BAD_PACKET);
                    return false;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(aPacket.bytes);
                string typename = string.Empty;

                try
                {
                    if(!CheckHeader(PacketType.ConnectionRequest, formatter,stream))
                    {
                        return false;
                    }

                    //Request
                    typename = (string)formatter.Deserialize(stream);
                    aRequest.Deserialize(stream, formatter);
                    //User
                    typename = (string)formatter.Deserialize(stream);
                    aUser.Deserialize(stream, formatter);

                    return true;
                }
                catch(Exception aException)
                {
                    if(!string.IsNullOrEmpty(typename))
                    {
                        DebugUtils.LogError("Error deserializing packet type, " + typename);
                    }
                    DebugUtils.LogException(aException);
                    return false;
                }
                
            }

            //Connection Status

            public static Packet CreateConnectionStatusPacket(Request aRequest, int aStatus)
            {
                if(aRequest == null)
                {
                    DebugUtils.ArgumentNull("aRequest");
                    return null;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();

                //Version/Type
                CreateHeader(PacketType.ConnectionStatus,formatter,stream);

                //Request
                formatter.Serialize(stream, typeof(Request).Name);
                aRequest.Serialize(stream, formatter);

                //Status
                formatter.Serialize(stream, typeof(int).Name);
                formatter.Serialize(stream, aStatus);
                return new Packet(stream.ToArray());
            }

            public static bool GetConnectionStatusPacketData(Packet aPacket, out Request aRequest, out int aStatus)
            {
                aRequest = Request.EMPTY;
                aStatus = NetworkStatus.ERROR;

                if(aPacket == null || aPacket.bytes == null)
                {
                    DebugUtils.LogError(ErrorCode.BAD_PACKET);
                    return false;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(aPacket.bytes);
                string typename = string.Empty;

                try
                {
                    //Check Header
                    if (!CheckHeader(PacketType.ConnectionStatus, formatter, stream))
                    {
                        return false;
                    }
                    //Request
                    typename = (string)formatter.Deserialize(stream);
                    aRequest.Deserialize(stream, formatter);
                    //User
                    typename = (string)formatter.Deserialize(stream);
                    aStatus = (int)formatter.Deserialize(stream);
                    return true;
                }
                catch (Exception aException)
                {
                    if (!string.IsNullOrEmpty(typename))
                    {
                        DebugUtils.LogError("Error deserializing packet type, " + typename);
                    }
                    DebugUtils.LogException(aException);
                    return false;
                }
            }

            //Connection List

            public static Packet CreateConnectionListPacket(NetworkUser[] aConnectionList)
            {
                if(aConnectionList == null || aConnectionList.Length == 0)
                {
                    DebugUtils.ArgumentNull("aConnectionList");
                    return null;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();

                CreateHeader(PacketType.ConnectionList, formatter, stream);

                formatter.Serialize(stream,typeof(int).Name);
                formatter.Serialize(stream, aConnectionList.Length);

                foreach (NetworkUser user in aConnectionList)
                {
                    formatter.Serialize(stream, typeof(NetworkUser).Name);
                    user.Serialize(stream, formatter);
                }

                return new Packet(stream.ToArray());
            }

            public static bool GetConnectionListPacketData(Packet aPacket, out NetworkUser[] aConnectionList)
            {
                aConnectionList = null;

                if(aPacket == null || aPacket.bytes == null || aPacket.bytes.Length == 0)
                {
                    DebugUtils.LogError(ErrorCode.BAD_PACKET);
                    return false;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(aPacket.bytes);
                string typename = string.Empty;

                try
                {
                    if (!CheckHeader(PacketType.ConnectionList, formatter, stream))
                    {
                        return false;
                    }

                    typename = (string)formatter.Deserialize(stream);
                    int count = (int)formatter.Deserialize(stream);

                    aConnectionList = new NetworkUser[count];

                    for (int i = 0; i < count; i++)
                    {
                        typename = (string)formatter.Deserialize(stream);
                        NetworkUser user = NetworkUser.BAD_USER;
                        user.Deserialize(stream, formatter);
                        aConnectionList[i] = user;
                    }

                    return true;
                }
                catch(Exception aException)
                {
                    if (!string.IsNullOrEmpty(typename))
                    {
                        DebugUtils.LogError("Error deserializing packet type, " + typename);
                    }
                    DebugUtils.LogException(aException);
                    return false;
                }

            }
        }

    }
}

