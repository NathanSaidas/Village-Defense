using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Gem
{
    namespace Tools
    {
        public class ConfigFile
        {
            private List<ConfigFileSection> m_Sections = new List<ConfigFileSection>();

            public void Save(string aFilename)
            {
                if(!string.IsNullOrEmpty(aFilename))
                {
                    ConfigStringStream stream = new ConfigStringStream();
                    stream.Clear();

                    foreach(ConfigFileSection section in m_Sections)
                    {
                        section.Write(stream);
                    }

                    File.WriteAllText(aFilename, stream.content);
                }
            }

            public void Load(string aFilename)
            {
                if(!string.IsNullOrEmpty(aFilename))
                {
                    //Clear Section
                    Clear();

                    //Create stream
                    ConfigStringStream stream = new ConfigStringStream();
                    stream.Clear();

                    //Read text
                    stream.content = File.ReadAllText(aFilename);

                    //Get all section names
                    string line = stream.ReadLine();
                    while(!string.IsNullOrEmpty(line))
                    {
                        //If section line, add new section.
                        if(line[0] == '[' && line[line.Length-1] == ']')
                        {
                            ConfigFileSection section = new ConfigFileSection();
                            section.sectionName = m_Sections.Count.ToString();
                            AddSection(section);
                        }
                        line = stream.ReadLine();
                    }

                    stream.ResetCursor();
                    //Read the stream.
                    foreach(ConfigFileSection section in m_Sections)
                    {
                        section.Read(stream);
                    }

                }
            }

            public string SaveToString()
            {
                ConfigStringStream stream = new ConfigStringStream();
                stream.Clear();

                foreach (ConfigFileSection section in m_Sections)
                {
                    section.Write(stream);
                }

                return stream.content;
            }

            public void LoadFromString(string aContent)
            {
                //Create stream
                ConfigStringStream stream = new ConfigStringStream();
                stream.Clear();

                //Read text
                stream.content = aContent;

                //Get all section names
                string line = stream.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    //If section line, add new section.
                    if (line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        ConfigFileSection section = new ConfigFileSection();
                        section.sectionName = m_Sections.Count.ToString();
                        AddSection(section);
                    }
                    line = stream.ReadLine();
                }

                stream.ResetCursor();
                //Read the stream.
                foreach (ConfigFileSection section in m_Sections)
                {
                    section.Read(stream);
                }
            }

            public void Clear()
            {
                m_Sections.Clear();
            }

            public bool AddSection(ConfigFileSection aSection)
            {
                if(aSection == null)
                {
                    DebugUtils.ArgumentNull("aSection");
                    return false;
                }

                if(m_Sections.Count > 0 && m_Sections.Any<ConfigFileSection>(Element => Element.sectionName == aSection.sectionName))
                {
                    return false;
                }

                m_Sections.Add(aSection);
                return true;
            }
            public bool RemoveSection(ConfigFileSection aSection)
            {
                return m_Sections.Remove(aSection);
            }
            public bool RemoveSection(string aSectionName)
            {
                foreach(ConfigFileSection section in m_Sections)
                {
                    if (section.sectionName == aSectionName)
                    {
                        m_Sections.Remove(section);
                        return true;
                    }
                }
                return false;
            }
            public ConfigFileSection GetSection(string aSectionName)
            {
                return m_Sections.FirstOrDefault<ConfigFileSection>(Element => Element.sectionName == aSectionName);
            }
            public ConfigFileSection GetSection(int aIndex)
            {
                if(aIndex >= 0 && aIndex < m_Sections.Count)
                {
                    return m_Sections[aIndex];
                }
                return null;
            }
        }

    }
}

