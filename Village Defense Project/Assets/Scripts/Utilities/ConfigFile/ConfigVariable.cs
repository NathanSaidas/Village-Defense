using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Gem
{
    namespace Tools
    {

        /// <summary>
        /// Config variable definition type.
        /// See SUPPORTED TYPES region for list of supported types.
        /// </summary>
        /// <typeparam name="T">The type of variable.</typeparam>
        public class ConfigVariable<T> : ConfigVariableBase
        {



            private static readonly Type[] SUPPORTED_TYPES = new Type[]
            {
                typeof(int), 
                typeof(uint),
                typeof(short),
                typeof(ushort),
                typeof(char),
                typeof(byte),
                typeof(string),
                typeof(double),
                typeof(float),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Color)
            };

            /// <summary>
            /// The name of the variable.
            /// </summary>
            private string m_VariableName = string.Empty;
            /// <summary>
            /// The value of the variable.
            /// </summary>
            private T m_Value = default(T);

            //Constructors
            /// <summary>
            /// Default Constructor.
            /// </summary>
            public ConfigVariable() : base()
            {
                m_VariableName = string.Empty;
                m_Value = default(T);
            }

            /// <summary>
            /// Initializer constructor, initializes the class with the given values.
            /// </summary>
            /// <param name="aVariableName">The name of the variable.</param>
            /// <param name="aValue">The value of the variable..</param>
            public ConfigVariable(string aVariableName, T aValue) : base()
            {
                m_VariableName = aVariableName;
                m_Value = aValue;
            }

            /// <summary>
            /// Returns whether or not a type is a supported variable type.
            /// </summary>
            /// <typeparam name="TYPE">The type to check.</typeparam>
            /// <returns>Returns whether or not a type is a supported variable type.</returns>
            public static bool IsSupported()
            {
                return IsSupported(typeof(T));
            }

            /// <summary>
            /// Returns whether or not a type is a supported variable type.
            /// </summary>
            /// <param name="aTypename">The name of the type. Refer to https://msdn.microsoft.com/en-us/library/w3f99sx1(v=vs.110).aspx</param>
            /// <returns>Returns whether or not a type is a supported variable type.</returns>
            public static bool IsSupported(string aTypename)
            {
                return IsSupported(Type.GetType(aTypename));
            }

            /// <summary>
            /// Returns whether or not a type is a supported variable type.
            /// </summary>
            /// <param name="aType">The type to check.</param>
            /// <returns>Returns whether or not a type is a supported variable type.</returns>
            public static bool IsSupported(Type aType)
            {
                return SUPPORTED_TYPES.Any<Type>(Element => Element == aType);
            }

            /// <summary>
            /// Parses variable data from a string.
            /// </summary>
            /// <param name="aVariable">A formatted string containing variable data. See GetVariableData method for format.</param>
            /// <returns>Returns true if parse successful or false if unsuccessful.</returns>
            public override bool ParseVariableData(string aVariable)
            {
                
                //words[0] = type
                //words[1] = variable name
                //words[2] = '='
                //words[3] = value
                //words[5] =  [element 0] (x,r)
                //words[7] =  [element 1] (y,g)
                //words[9] =  [element 2] (z,b)
                //words[11] = [element 3] (w,a)
                
                //Parse the string to words.
                List<string> words = Utilities.ParseToWords(aVariable, false);

                //If the word count is not valid flag the variable as an error type.
                if(words.Count < 4)
                {
                    m_VariableName = PARSE_ERROR;
                    m_Value = default(T);
                    return false;
                }

                //Get the typename and variable name.
                string variableTypename = words[0];
                variableName = words[1];

                bool error = false;

                ///Parse the value based on supported types.
                switch(variableTypename)
                {
                    case TYPE_INT:
                        {
                            int rawValue = 0;
                            if (int.TryParse(words[3], out rawValue))
                            {
                                m_Value = (T)(object)rawValue;
                            }
                            else
                            {
                                m_Value = default(T);
                                error = true;
                            }
                        }
                        break;
                    case TYPE_UINT:
                        {
                            uint rawValue = 0;
                            if (uint.TryParse(words[3], out rawValue))
                            {
                                m_Value = (T)(object)rawValue;
                            }
                            else
                            {
                                m_Value = default(T);
                            }
                        }
                        break;
                    case TYPE_FLOAT:
                        {
                            float rawValue = 0;
                            if (float.TryParse(words[3], out rawValue))
                            {
                                m_Value = (T)(object)rawValue;
                            }
                            else
                            {
                                m_Value = default(T);
                                error = true;
                            }
                        }
                        break;
                    case TYPE_USHORT:
                        {
                            ushort rawValue = 0;
                            if (ushort.TryParse(words[3], out rawValue))
                            {
                                m_Value = (T)(object)rawValue;
                            }
                            else
                            {
                                m_Value = default(T);
                                error = true;
                            }
                        }
                        break;
                    case TYPE_SHORT:
                        {
                            short rawValue = 0;
                            if (short.TryParse(words[3], out rawValue))
                            {
                                m_Value = (T)(object)rawValue;
                            }
                            else
                            {
                                m_Value = default(T);
                                error = true;
                            }
                        }
                        break;
                    case TYPE_CHAR:
                        {
                            char rawValue = '0';
                            if (char.TryParse(words[3], out rawValue))
                            {
                                m_Value = (T)(object)rawValue;
                            }
                            else
                            {
                                m_Value = default(T);
                                error = true;
                            }
                        }
                        break;
                    case TYPE_BYTE:
                        {
                            byte rawValue = 0;
                            if (byte.TryParse(words[3], out rawValue))
                            {
                                m_Value = (T)(object)rawValue;
                            }
                            else
                            {
                                m_Value = default(T);
                                error = true;
                            }
                        }
                        break;
                    case TYPE_DOUBLE:
                        {
                            double rawValue = 0;
                            if (double.TryParse(words[3], out rawValue))
                            {
                                m_Value = (T)(object)rawValue;
                            }
                            else
                            {
                                m_Value = default(T);
                                error = true;
                            }
                        }
                        break;
                    case TYPE_STRING:
                        {
                            string rawValue = words[3];
                            m_Value = (T)(object)rawValue;
                        }
                        break;
                    case TYPE_VECTOR2:
                        {
                            Vector2 vector = Vector2.zero;
                            if (words.Count > 4 && !float.TryParse(words[5], out vector.x))
                            {
                                error = true;
                            }
                            if (words.Count > 6 && !float.TryParse(words[7], out vector.y))
                            {
                                error = true;
                            }

                            m_Value = (T)(object)vector;
                        }
                        break;
                    case TYPE_VECTOR3:
                        {
                            Vector3 vector = Vector3.zero;
                            if (words.Count > 4 && !float.TryParse(words[5], out vector.x))
                            {
                                error = true;
                            }
                            if (words.Count > 6 && !float.TryParse(words[7], out vector.y))
                            {
                                error = true;
                            }
                            if (words.Count > 8 && !float.TryParse(words[9], out vector.z))
                            {
                                error = true;
                            }
     
                            m_Value = (T)(object)vector;
                        }
                        
                        break;
                    case TYPE_VECTOR4:
                        {
                            Vector4 vector = Vector4.zero;
                            if (words.Count > 4 && !float.TryParse(words[5], out vector.x))
                            {
                                error = true;
                            }
                            if (words.Count > 6 && !float.TryParse(words[7], out vector.y))
                            {
                                error = true;
                            }
                            if (words.Count > 8 && !float.TryParse(words[9], out vector.z))
                            {
                                error = true;
                            }
                            if (words.Count > 10 && !float.TryParse(words[11], out vector.w))
                            {
                                error = true;
                            }

                            m_Value = (T)(object)vector;
                        }
                        break;
                    case TYPE_COLOR:
                        {
                            
                            Color color = Color.black;
                            if (words.Count > 4 && !float.TryParse(words[5], out color.r))
                            {
                                error = true;
                            }
                            if (words.Count > 6 && !float.TryParse(words[7], out color.g))
                            {
                                error = true;
                            }
                            if (words.Count > 8 && !float.TryParse(words[9], out color.b))
                            {
                                error = true;
                            }
                            if (words.Count > 10 && !float.TryParse(words[11], out color.a))
                            {
                                error = true;
                            }

                            m_Value = (T)(object)color;
                        }
                        break;
                    case TYPE_INVALID:
                        DebugUtils.LogError("Type is not supported.\nType = " + variableTypename + "\nName = " + variableName);
                        break;
                    default:
                        DebugUtils.LogError("Type is not supported.\nType = " + variableTypename + "\nName = " + variableName);
                        break;
                }

                if(error)
                {
                    m_VariableName = m_VariableName + PARSE_ERROR;
                }

                return error;
            }

            /// <summary>
            /// Creates a string thats a readable line for a variable.
            /// Format = [Typename] [Variable Name] = [Value]
            /// </summary>
            /// <returns>Returns a string in the specficied format.</returns>
            public override string GetVariableData()
            {
                Type variableType = type;
                string variableTypename = variableType.Name;
                string variableValue = string.Empty;

                if (variableTypename == SUPPORTED_TYPES[0].Name)
                {
                    variableTypename = TYPE_INT;
                    variableValue = value.ToString();
                }
                else if (variableTypename == SUPPORTED_TYPES[1].Name)
                {
                    variableTypename = TYPE_UINT;
                    variableValue = value.ToString();
                }
                else if (variableTypename == SUPPORTED_TYPES[2].Name)
                {
                    variableTypename = TYPE_SHORT;
                    variableValue = value.ToString();
                }
                else if (variableTypename == SUPPORTED_TYPES[3].Name)
                {
                    variableTypename = TYPE_USHORT;
                    variableValue = value.ToString();
                }
                else if (variableTypename == SUPPORTED_TYPES[4].Name)
                {
                    variableTypename = TYPE_CHAR;
                    variableValue = value.ToString();
                }
                else if (variableTypename == SUPPORTED_TYPES[5].Name)
                {
                    variableTypename = TYPE_BYTE;
                    variableValue = value.ToString();
                }
                else if (variableTypename == SUPPORTED_TYPES[6].Name)
                {
                    variableTypename = TYPE_STRING;
                    variableValue = value.ToString();
                }
                else if (variableTypename == SUPPORTED_TYPES[7].Name)
                {
                    variableTypename = TYPE_DOUBLE;
                    variableValue = value.ToString();
                }
                else if (variableTypename == SUPPORTED_TYPES[8].Name)
                {
                    variableTypename = TYPE_FLOAT;
                    variableValue = value.ToString();
                }
                else if (variableTypename == SUPPORTED_TYPES[9].Name)
                {
                    Vector2 vector = (Vector2)(object)m_Value;
                    variableTypename = TYPE_VECTOR2;
                    variableValue = "[x] " + vector.x + " [y] " + vector.y;  
                }
                else if (variableTypename == SUPPORTED_TYPES[10].Name)
                {
                    Vector3 vector = (Vector3)(object)m_Value;
                    variableTypename = TYPE_VECTOR3;
                    variableValue = "[x] " + vector.x + " [y] " + vector.y + " [z] " + vector.z;
                }
                else if (variableTypename == SUPPORTED_TYPES[11].Name)
                {
                    Vector4 vector = (Vector4)(object)m_Value;
                    variableTypename = TYPE_VECTOR4;
                    variableValue = "[x] " + vector.x + " [y] " + vector.y + " [z] " + vector.z + " [w] " + vector.w;
                }
                else if (variableTypename == SUPPORTED_TYPES[12].Name)
                {
                    Color color = (Color)(object)m_Value;
                    variableTypename = TYPE_COLOR;
                    variableValue = "[r] " + color.r + " [g] " + color.g + " [b] " + color.b + " [a] " + color.a;
                }
                else
                {
                    variableTypename = TYPE_INVALID;
                    variableValue = VALUE_INVALID;
                }

                return variableTypename + " " + variableName + " = " + variableValue;
            }

            /// <summary>
            /// Calls and returns GetVariableData
            /// </summary>
            /// <returns>A string in the format of readable data.</returns>
            public override string ToString()
            {
                return GetVariableData();
            }

            public override string GetVariableName()
            {
                return variableName;
            }

            public override string GetVariableTypename()
            {
                return type.Name;
            }

            /// <summary>
            /// The name of the variable.
            /// </summary>
            public string variableName
            {
                get { return m_VariableName; }
                set { m_VariableName = value; }
            }

            /// <summary>
            /// The value of the variable.
            /// </summary>
            public T value
            {
                get { return m_Value; }
                set { m_Value = value; }
            }

            /// <summary>
            /// Gets the type of config variable
            /// </summary>
            public Type type
            {
                get { return typeof(T); }
            }
        }
    }

}


