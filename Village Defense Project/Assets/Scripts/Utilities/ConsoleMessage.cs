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
/* December, 7, 2014 - Nathan Hanlan - Added struct ConsoleMessage and Enum LogLevel
 * 
 */ 
#endregion

namespace Gem
{
    /// <summary>
    /// Describes the kind of console message the console should display.
    /// </summary>
    public enum LogLevel
    {
        Log,
        Warning,
        Error,
        User
    }

    /// <summary>
    /// Defines a message in the console.
    /// </summary>
    public struct ConsoleMessage
    {
        /// <summary>
        /// The contents of the message
        /// </summary>
        private string m_Message;
        /// <summary>
        /// The type of message it is.
        /// </summary>
        private LogLevel m_LogLevel;
        public ConsoleMessage(string aMessage)
        {
            m_Message = aMessage;
            m_LogLevel = LogLevel.Log;
        }
        public ConsoleMessage(string aMessage, LogLevel aLogLevel)
        {
            m_Message = aMessage;
            m_LogLevel = aLogLevel;
        }
        /// <summary>
        /// An accessor to the contents of the message
        /// </summary>
        public string message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }
        /// <summary>
        /// An accessor to the type of message it is.
        /// </summary>
        public LogLevel logLevel
        {
            get { return m_LogLevel; }
            set { m_LogLevel = value; }
        }
    }
}