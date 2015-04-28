using UnityEngine;
using System.Collections;

namespace Gem
{
    namespace Tools
    {
        public class ConfigStringStream
        {
            private string m_Content = string.Empty;
            private int m_CurrentLine = 1;
            private int m_Cursor = 0;
 
            /// <summary>
            /// Resets the cursor back to 0.
            /// </summary>
            public void ResetCursor()
            {
                m_Cursor = 0;
                m_CurrentLine = 1;
            }

            /// <summary>
            /// Clears the content and resets the cursor.
            /// </summary>
            public void Clear()
            {
                m_Content = string.Empty;
                ResetCursor();
            }

            /// <summary>
            /// Adds a line to the content.
            /// </summary>
            /// <param name="aContent"></param>
            public void AddLine(string aContent)
            {
                m_Content += aContent + "\n";
            }

            /// <summary>
            /// Reads a line advancing the cursors position.
            /// </summary>
            /// <returns>Returns the line read.</returns>
            public string ReadLine()
            {
                string line = string.Empty;
                if(string.IsNullOrEmpty(m_Content) || m_Cursor >= m_Content.Length)
                {
                    return line;
                }
                int start = m_Cursor;
                int end = 0;
                //Read for new line mark.

                for(; m_Cursor < m_Content.Length; m_Cursor++)
                {
                    if(m_Content[m_Cursor] == '\n')
                    {
                        end = m_Cursor;
                        break;
                    }
                }

                //Get Line

                if(end != 0)
                {
                    line = m_Content.Substring(start, (end - start));
                }
                else
                {
                    line = m_Content.Substring(start, m_Content.Length - start);
                }

                //Set Cursor
                m_Cursor = end + 1;
                m_CurrentLine++;

                return line;
            }

            /// <summary>
            /// Get / Set content.
            /// </summary>
            public string content
            {
                get { return m_Content; }
                set { m_Content = value; }
            }

            public int currentLine
            {
                get { return m_CurrentLine; }
                set { m_CurrentLine = value; }
            }
            
        }
    }
}

