using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Gem
{
    namespace Tools
    {
        /// <summary>
        /// A section of variables
        /// </summary>
        public class ConfigFileSection
        {
            /// <summary>
            /// The name of the section
            /// </summary>
            private string m_SectionName = string.Empty;
            /// <summary>
            /// The list of variables within the section.
            /// </summary>
            private List<ConfigVariableBase> m_Variables = new List<ConfigVariableBase>();

            /// <summary>
            /// Write all the variables to the stream.
            /// </summary>
            /// <param name="aStream">The stream to write to.</param>
            public void Write(ConfigStringStream aStream)
            {
                if(string.IsNullOrEmpty(m_SectionName))
                {
                    DebugUtils.LogWarning("Section name is empty");
                }
                aStream.AddLine("[" + m_SectionName + "]");
                aStream.AddLine("$Variables=" + m_Variables.Count);
                foreach(ConfigVariableBase variable in m_Variables)
                {
                    aStream.AddLine(variable.GetVariableData());
                }
            }

            /// <summary>
            /// Read all the variables from the stream.
            /// </summary>
            /// <param name="aStream">The stream to write to.</param>
            public void Read(ConfigStringStream aStream)
            {
                //Get Section Name.
                int currentLine = aStream.currentLine;
                string line = aStream.ReadLine();
                if(line.Length < 2                                          //Bad Line
                    || (line[0] != '[' || line[line.Length-1] != ']'))      //Bad Line, Does not contain section name.
                {
                    DebugUtils.LogWarning("Failed to parse stream, Missing Section Header\n{Line: " + currentLine + "}");
                    return;
                }
                m_SectionName = line.Substring(1, line.Length - 2);
                //Get Variable Count
                currentLine = aStream.currentLine;
                line = aStream.ReadLine();
                if(!line.Contains("$Variables=") || line.Length < 12)
                {
                    DebugUtils.LogWarning("Failed to parse stream, Missing Variables Count\n{Line: " + currentLine + "}");
                    return;
                }
                //Create all the variables.
                int count = 0;
                if(int.TryParse(line.Substring(11, line.Length - 11), out count))
                {
                    for(int i = 0; i < count; i++)
                    {
                        line = aStream.ReadLine();
                        CreateVariable(line);
                    }
                }
                else
                {
                    DebugUtils.LogWarning("Failed to parse line, Value is not a number");
                }
            }

            /// <summary>
            /// Log out all of the variables to the debug console.
            /// </summary>
            public void LogSection()
            {
                DebugUtils.Log("[" + m_SectionName + "]");
                foreach(ConfigVariableBase variable in m_Variables)
                {
                    DebugUtils.Log(variable.GetVariableData());
                }
            }

            /// <summary>
            /// Add a variable to the section. Variables of the same name cannot be added.
            /// </summary>
            /// <param name="aVariable">The variable to add.</param>
            /// <returns>Returns true if successful, returns false otherwise.</returns>
            public bool AddVariable(ConfigVariableBase aVariable)
            {
                //Don't add null variables.
                if(aVariable == null)
                {
                    DebugUtils.ArgumentNull("aVariable");
                    return false;
                }

                //Don't add the variable, one with the same name already exists.
                if(Exists(aVariable.GetVariableName()))
                {   
                    return false;
                }
                m_Variables.Add(aVariable);
                return true;
            }
            
            /// <summary>
            /// Add a variable to the section. Variables of the same name cannot be added.
            /// </summary>
            /// <typeparam name="T">The type of variable to add.</typeparam>
            /// <param name="aVariableName">The name of the variable.</param>
            /// <param name="aValue">The value of the variable.</param>
            /// <returns>Returns true if successful, returns false otherwise.</returns>
            public bool AddVariable<T>(string aVariableName, T aValue)
            {
                if(string.IsNullOrEmpty(aVariableName))
                {
                    DebugUtils.InvalidArgument("aVariableName");
                    return false;
                }
                if(!ConfigVariable<T>.IsSupported())
                {
                    DebugUtils.InvalidArgument("T", "Argument Type " + typeof(T).Name + " is not an accepted type.");
                    return false;
                }
                ConfigVariable<T> variable = new ConfigVariable<T>(aVariableName, aValue);
                m_Variables.Add(variable);
                return true;
            }

            /// <summary>
            /// Removes a variable from the section.
            /// </summary>
            /// <param name="aVariable">The variable to remove.</param>
            /// <returns>Returns true if the variable existed. Returns false otherwise.</returns>
            public bool RemoveVariable(ConfigVariableBase aVariable)
            {
                if(aVariable == null)
                {
                    DebugUtils.ArgumentNull("aVariable");
                    return false;
                }
                return m_Variables.Remove(aVariable);
            }
            
            /// <summary>
            /// Removes a variable from the section.
            /// </summary>
            /// <param name="aVariableName">The name of the variable to find and remove.</param>
            /// <returns>Returns true if the variable existed. Returns false otherwise.</returns>
            public bool RemoveVariable(string aVariableName)
            {
                if(string.IsNullOrEmpty(aVariableName))
                {
                    DebugUtils.InvalidArgument("aVariableName");
                    return false;
                }
                ConfigVariableBase variable = GetVariable(aVariableName);
                if(variable != null)
                {
                    return RemoveVariable(variable);
                }
                return false;
            }

            /// <summary>
            /// Gets a variable from the section by name.
            /// </summary>
            /// <param name="aVariableName">The name of the variable to get.</param>
            /// <returns>Returns the found variable or null if the variable is not found.</returns>
            public ConfigVariableBase GetVariable(string aVariableName)
            {
                if (string.IsNullOrEmpty(aVariableName))
                {
                    DebugUtils.InvalidArgument("aVariableName");
                    return null;
                }
                return m_Variables.FirstOrDefault<ConfigVariableBase>(Element => Element.GetVariableName() == aVariableName);
            }

            /// <summary>
            /// Gets a variable from the section by name and type.
            /// </summary>
            /// <param name="aVariableName">The name of the variable to get.</param>
            /// <param name="aTypename">The type name of the variable.</param>
            /// <returns>Returns the found variable or null if the variable is not found.</returns>
            public ConfigVariableBase GetVariable(string aVariableName, string aTypename)
            {
                if (string.IsNullOrEmpty(aVariableName))
                {
                    DebugUtils.InvalidArgument("aVariableName");
                    return null;
                }
                if (string.IsNullOrEmpty(aTypename))
                {
                    DebugUtils.InvalidArgument("aTypename");
                    return null;
                }
                return m_Variables.FirstOrDefault<ConfigVariableBase>(Element => Element.GetVariableName() == aVariableName && Element.GetVariableTypename() == aTypename);
            }

            /// <summary>
            /// Gets a variable from the section by name and type.
            /// </summary>
            /// <param name="aVariableName">The name of the variable to get.</param>
            /// <param name="aType">The type of the variable.</param>
            /// <returns>Returns the found variable or null if the variable is not found.</returns>
            public ConfigVariableBase GetVariable(string aVariableName, Type aType)
            {
                return GetVariable(aVariableName, aType.Name);
            }

            /// <summary>
            /// Determines if the variable exists by name comparison.
            /// </summary>
            /// <param name="aVariableName">The name of the variable to search for.</param>
            /// <returns>Returns whether or not the variable exists.</returns>
            public bool Exists(string aVariableName)
            {
                return m_Variables.Count > 0 && m_Variables.Any<ConfigVariableBase>(Element => Element.GetVariableName() == aVariableName);
            }

            /// <summary>
            /// Determines if the variable exists by name and type comparison.
            /// </summary>
            /// <param name="aVariableName">The name of the variable to search for.</param>
            /// <param name="aTypename">The type name of the variable.</param>
            /// <returns>Returns whether or not the variable exists.</returns>
            public bool Exists(string aVariableName, string aTypename)
            {
                return m_Variables.Count > 0 && m_Variables.Any<ConfigVariableBase>(Element => Element.GetVariableName() == aVariableName && Element.GetVariableTypename() == aTypename);
            }

            /// <summary>
            /// Determines if the variable exists by name and type comparison.
            /// </summary>
            /// <param name="aVariableName">The name of the variable to search for.</param>
            /// <param name="aType">The type name of the variable.</param>
            /// <returns>Returns whether or not the variable exists.</returns>
            public bool Exists(string aVariableName, Type aType)
            {
                return Exists(aVariableName, aType.Name);
            }

            /// <summary>
            /// Removes all variables from the section.
            /// </summary>
            public void Clear()
            {
                m_Variables.Clear();
            }

            /// <summary>
            /// Used for parsing to create a variable with a single line of data.
            /// </summary>
            /// <param name="aVariableData">A formatted line of variable data to parse.</param>
            void CreateVariable(string aVariableData)
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
                List<string> words = Utilities.ParseToWords(aVariableData, false);

                //If the word count is not valid flag the variable as an error type.
                if (words.Count < 4)
                {
                    return;
                }

                //Get the typename and variable name.
                string variableTypename = words[0];
                string variableName = words[1];

                bool error = false;

                ///Parse the value based on supported types.
                switch (variableTypename)
                {
                    case ConfigVariableBase.TYPE_INT:
                        {
                            int rawValue = 0;
                            if (int.TryParse(words[3], out rawValue))
                            {
                                ConfigVariable<int> variable = new ConfigVariable<int>(variableName, rawValue);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<int> variable = new ConfigVariable<int>(variableName + ConfigVariableBase.PARSE_ERROR, default(int));
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_UINT:
                        {
                            uint rawValue = 0;
                            if (uint.TryParse(words[3], out rawValue))
                            {
                                ConfigVariable<uint> variable = new ConfigVariable<uint>(variableName, rawValue);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<uint> variable = new ConfigVariable<uint>(variableName + ConfigVariableBase.PARSE_ERROR, default(uint));
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_FLOAT:
                        {
                            float rawValue = 0;
                            if (float.TryParse(words[3], out rawValue))
                            {
                                ConfigVariable<float> variable = new ConfigVariable<float>(variableName, rawValue);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<float> variable = new ConfigVariable<float>(variableName + ConfigVariableBase.PARSE_ERROR, default(float));
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_USHORT:
                        {
                            ushort rawValue = 0;
                            if (ushort.TryParse(words[3], out rawValue))
                            {
                                ConfigVariable<ushort> variable = new ConfigVariable<ushort>(variableName, rawValue);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<ushort> variable = new ConfigVariable<ushort>(variableName + ConfigVariableBase.PARSE_ERROR, default(ushort));
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_SHORT:
                        {
                            short rawValue = 0;
                            if (short.TryParse(words[3], out rawValue))
                            {
                                ConfigVariable<short> variable = new ConfigVariable<short>(variableName, rawValue);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<short> variable = new ConfigVariable<short>(variableName + ConfigVariableBase.PARSE_ERROR, default(short));
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_CHAR:
                        {
                            char rawValue = '0';
                            if (char.TryParse(words[3], out rawValue))
                            {
                                ConfigVariable<char> variable = new ConfigVariable<char>(variableName, rawValue);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<char> variable = new ConfigVariable<char>(variableName + ConfigVariableBase.PARSE_ERROR, default(char));
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_BYTE:
                        {
                            byte rawValue = 0;
                            if (byte.TryParse(words[3], out rawValue))
                            {
                                ConfigVariable<byte> variable = new ConfigVariable<byte>(variableName, rawValue);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<byte> variable = new ConfigVariable<byte>(variableName + ConfigVariableBase.PARSE_ERROR, default(byte));
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_DOUBLE:
                        {
                            double rawValue = 0;
                            if (double.TryParse(words[3], out rawValue))
                            {
                                ConfigVariable<double> variable = new ConfigVariable<double>(variableName, rawValue);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<double> variable = new ConfigVariable<double>(variableName + ConfigVariableBase.PARSE_ERROR, default(double));
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_STRING:
                        {
                            string rawValue = words[3];
                            ConfigVariable<string> variable = new ConfigVariable<string>(variableName, rawValue);
                            m_Variables.Add(variable);
                        }
                        break;
                    case ConfigVariableBase.TYPE_VECTOR2:
                        {
                            Vector2 vector = Vector2.zero;
                            if (words.Count > 4 && !float.TryParse(words[4], out vector.x))
                            {
                                error = true;
                            }
                            if (words.Count > 6 && !float.TryParse(words[6], out vector.y))
                            {
                                error = true;
                            }

                            if (!error)
                            {
                                ConfigVariable<Vector2> variable = new ConfigVariable<Vector2>(variableName, vector);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<Vector2> variable = new ConfigVariable<Vector2>(variableName + ConfigVariableBase.PARSE_ERROR, vector);
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_VECTOR3:
                        {
                            Vector3 vector = Vector3.zero;
                            if (words.Count > 4 && !float.TryParse(words[4], out vector.x))
                            {
                                error = true;
                            }
                            if (words.Count > 6 && !float.TryParse(words[6], out vector.y))
                            {
                                error = true;
                            }
                            if (words.Count > 8 && !float.TryParse(words[8], out vector.z))
                            {
                                error = true;
                            }
                            if (!error)
                            {
                                ConfigVariable<Vector3> variable = new ConfigVariable<Vector3>(variableName, vector);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<Vector3> variable = new ConfigVariable<Vector3>(variableName + ConfigVariableBase.PARSE_ERROR, vector);
                                m_Variables.Add(variable);
                            }
                        }

                        break;
                    case ConfigVariableBase.TYPE_VECTOR4:
                        {
                            Vector4 vector = Vector4.zero;
                            if (words.Count > 4 && !float.TryParse(words[4], out vector.x))
                            {
                                error = true;
                            }
                            if (words.Count > 6 && !float.TryParse(words[6], out vector.y))
                            {
                                error = true;
                            }
                            if (words.Count > 8 && !float.TryParse(words[8], out vector.z))
                            {
                                error = true;
                            }
                            if (words.Count > 10 && !float.TryParse(words[10], out vector.w))
                            {
                                error = true;
                            }
                            if(!error)
                            {
                                ConfigVariable<Vector4> variable = new ConfigVariable<Vector4>(variableName, vector);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<Vector4> variable = new ConfigVariable<Vector4>(variableName + ConfigVariableBase.PARSE_ERROR, vector);
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_COLOR:
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

                            if (!error)
                            {
                                ConfigVariable<Color> variable = new ConfigVariable<Color>(variableName, color);
                                m_Variables.Add(variable);
                            }
                            else
                            {
                                ConfigVariable<Color> variable = new ConfigVariable<Color>(variableName + ConfigVariableBase.PARSE_ERROR, color);
                                m_Variables.Add(variable);
                            }
                        }
                        break;
                    case ConfigVariableBase.TYPE_INVALID:
                        DebugUtils.LogError("Type is not supported.\nType = " + variableTypename + "\nName = " + variableName);
                        break;
                    default:
                        DebugUtils.LogError("Type is not supported.\nType = " + variableTypename + "\nName = " + variableName);
                        break;
                }
            }

            /// <summary>
            /// The name of the section.
            /// </summary>
            public string sectionName
            {
                get { return m_SectionName; }
                set { m_SectionName = value; }
            }

        }
    }
}


