
using UnityEngine;

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
        public const string NETWORK_EVENT_PROPERTY_KICKED_PLAYER = "kickedPlayer";
        public const string NETWORK_EVENT_PROPERTY_KICKED_REASON = "kickedReason";
        public const string NETWORK_EVENT_PROPERTY_CREATED_OBJECT = "createdObject";
        public const string NETWORK_EVENT_PROPERTY_DESTROYED_OBJECT = "destroyedObject";
        public const string NETWORK_EVENT_PROPERTY_SENDER = "sender";

        #endregion

        #endregion

        public static KeyCode INPUT_CAMERA_LEFT_KEY = KeyCode.A;
        public static KeyCode INPUT_CAMERA_RIGHT_KEY = KeyCode.D;
        public static KeyCode INPUT_CAMERA_UP_KEY = KeyCode.W;
        public static KeyCode INPUT_CAMERA_DOWN_KEY = KeyCode.S;
        public static KeyCode INPUT_CAMERA_FOCUS_PLAYER = KeyCode.F1;

    }


}

