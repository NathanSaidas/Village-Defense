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
// -- April     11, 2015 - Nathan Hanlan - Added GameObject constants.
#endregion
namespace Gem
{
    public static class Constants
    {
        #region SCENE NAMES
        public const string SCENE_INIT = "init_scene";
        public const string SCENE_FINAL = "empty_scene";
        public const string SCENE_EMPTY = "empty_scene";
        #endregion

        #region LAYER CONSTANTS
        public const string LAYER_UI = "UI";
        #endregion


        #region GAME OBJECT CONSTANTS
        public const string GAME_OBJECT_PERSISTENT = "_Persistent";
        public const string GAME_OBJECT_EVENT_SYSTEM = "UI_EventSystem";
        #endregion

        #region NETWORK_CONSTANTS
        public const int DEFAULT_AUTHENTICATION_PORT = 25070;
        public const int DEFAULT_GAME_PORT = 25071;
        public const float DEFAULT_TIME_OUT = -1.0f;

        public const string SERVER_TYPE_AUTHENTICATION = "GEM-VD-Authentication";
        public const string SERVER_TYPE_GAME = "GEM-VD-Game";
        public const int SERVER_MAX_USERS = 6;


        #region NETWORK EVENT PROPERTIES

        public const string NETWORK_EVENT_PROPERTY_CONNECTING_USERS = "connectingUsers";
        public const string NETWORK_EVENT_PROPERTY_DISCONNECTING_USERS = "disconnectingUsers";

        #endregion

        #endregion

    }


}

