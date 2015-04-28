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
// -- December 1, 2014 - Nathan Hanlan - Adding Class DebugConstants and Enum ErrorCode
// -- April   11, 2015 - Nathan Hanlan - Adjusted constants to match Unity 5 Project.
#endregion

namespace Gem
{
    public static class DebugConstants
    {
        public static readonly string[] ERROR_STRINGS = new string[]
    {
        "General: Invalid Error Code.",
        "General: Starting the game from a scene which is not the " + Constants.SCENE_INIT + " may result in errors.",
#region COROUTINE
        "Coroutine: Coroutine has not been initialized. Call CoroutineEx.InitializeCoroutineExtensions",
#endregion

#region UI
        "UI: Invalid error window type",
        "UI: Invalid window close operation. Cannot close error window because its not the one on the top of the stack.",
        "UI: Missing event system",
#endregion

#region NETWORK
        "Network: Invalid username",
        "Network: Invalid password",
        "Network: User is already logged in",
        "Network: Connection already established. Disonnect from server before connecting to another",
        "Network: Invalid Connection",
        "Network: Invalid Network State",
        "Network: Invalid Request",
        "Network: IsServerHost is true but the peer is not a server host",
        "Network: Bad Packet",
        "Network: Invalid Packet Version",
        "Network: Invalid Packet Type",
#endregion

#region GAME_CACHE
        "Game Cache is missing. Possibly no instance of Game in the scene or the game is quitting.",
        "Game Cache cannot add another entry with the same key",
        "Game Cache cannot remove entry, entry does not exist",
        "Game Cache cannot get entry, entry does not exist",
#endregion

    };

        public static string GetError(int aCode)
        {
            if (aCode > ERROR_STRINGS.Length || aCode < 0)
            {
                return ERROR_STRINGS[0];
            }
            return ERROR_STRINGS[aCode];
        }
        public static string GetError(ErrorCode aCode)
        {
            int code = (int)aCode;
            if (code > ERROR_STRINGS.Length || code < 0)
            {
                return ERROR_STRINGS[0];
            }
            return ERROR_STRINGS[code];
        }
    }

    /// <summary>
    /// Must match DebugConstants ERROR_STRINGS
    /// </summary>
    public enum ErrorCode
    {
        INVALID_CODE,
        INVALID_START_GAME,
        #region COROUTINE
        COROUTINE_NOT_INITIALIZED,
        #endregion

        #region UI
        INVALID_ERROR_WINDOW_TYPE,
        INVALID_ERROR_WINDOW_CLOSE_OPERATION,
        MISSING_EVENT_SYSTEM,
        #endregion

        #region NETWORK
        INVALID_USERNAME,
        INVALID_PASSWORD,
        USER_ALREADY_LOGGED_IN,
        CONNECTION_ALREADY_ESTABLISHED,
        INVALID_CONNECTION,
        INVALID_NETWORK_STATE,
        INVALID_REQUEST,
        INVALID_SERVER_HOST_STATE,
        BAD_PACKET,
        INVALID_PACKET_VERSION,
        INVALID_PACKET_TYPE,
        #endregion

        #region GAME_CACHE
        GAME_CACHE_MISSING,
        GAME_CACHE_ADD_FAIL,
        GAME_CACHE_REMOVE_FAIL,
        GAME_CACHE_GET_FAIL,
        #endregion
    }
}