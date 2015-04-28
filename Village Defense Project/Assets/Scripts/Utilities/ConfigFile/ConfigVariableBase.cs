namespace Gem
{
    namespace Tools
    {
        /// <summary>
        /// Base class for all config variable types.
        /// </summary>
        public abstract class ConfigVariableBase
        {
            // -- Constants for supported types.
            #region SUPPORTED TYPES
            public const string TYPE_INT = "int";
            public const string TYPE_UINT = "uint";
            public const string TYPE_FLOAT = "float";
            public const string TYPE_DOUBLE = "double";
            public const string TYPE_SHORT = "short";
            public const string TYPE_USHORT = "ushort";
            public const string TYPE_CHAR = "char";
            public const string TYPE_BYTE = "byte";
            public const string TYPE_STRING = "string";

            public const string TYPE_VECTOR2 = "Vector2";
            public const string TYPE_VECTOR3 = "Vector3";
            public const string TYPE_VECTOR4 = "Vector4";
            public const string TYPE_COLOR = "Color";

            

            public const string TYPE_INVALID = "InvalidType";
            public const string VALUE_INVALID = "@@BAD_VALUE@@";
            public const string PARSE_ERROR = "_PARSE_ERROR";
            #endregion

            public abstract bool ParseVariableData(string aVariable);
            public abstract string GetVariableData();
            public abstract string GetVariableName();
            public abstract string GetVariableTypename();
        }

    }
}

