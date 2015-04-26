//Copyright (c) 2015 Nathan Hanlan
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

#region CHANGE LOG
/// -- April    15, 2015 - Nathan Hanlan - Added NetworkRPC file with base constants for RPC calls.
#endregion

using UnityEngine;
using System.Collections;

namespace Gem
{

    public static class NetworkRPC
    {
        //Constants prefixed with the name "MANAGER" are from NetworkManager.
        public const string MANAGER_ON_REQUEST_AUTHENTICATION = "OnRequestAuthentication";
        public const string MANAGER_ON_REQUEST_CONNECTION_LIST = "OnRequestConnectionList";
        public const string MANAGER_ON_REQUEST_CONNECTION = "OnRequestConnection";
        public const string MANAGER_ON_REQUEST_KICK = "OnRequestKick";
        public const string MANAGER_ON_REQUEST_DISCONNECT = "OnRequestDisconnect";
        public const string MANAGER_ON_REQUEST_SELECT_HERO = "OnRequestSelectHero";
        public const string MANAGER_ON_REQUEST_START_GAME = "OnRequestStartGame";
        public const string MANAGER_ON_REQUEST_SPAWN = "OnRequestSpawn";
        public const string MANAGER_ON_REQUEST_OWNERSHIP = "OnRequestOwnership";
        public const string MANAGER_ON_REQUEST_DESTROY = "OnRequestDestroy";
        public const string MANAGER_ON_REQUEST_ISSUE_ABILITY = "OnRequestIssueAbility";


        public const string MANAGER_ON_RECEIVE_AUTHENTICATION_STATUS = "OnReceiveAuthenticationStatus";
        public const string MANAGER_ON_RECEIVE_CONNECTIONS_STATUS = "OnReceiveConnectionStatus";
        public const string MANAGER_ON_RECEIVE_DISCONNECT_STATUS = "OnReceiveDisconnectStatus";
        public const string MANAGER_ON_ESTABLISH_NEW_HOST = "OnEstablishNewHost";
        public const string MANAGER_ON_RECEIVE_CONNECTION_LIST = "OnReceiveConnectionList";
        public const string MANAGER_ON_PLAYER_DISCONNECT = "OnPlayerDisconnect";
    }
}

