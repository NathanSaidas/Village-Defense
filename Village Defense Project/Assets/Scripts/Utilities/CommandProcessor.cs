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

// -- System
using System.Collections.Generic;
// -- Unity
using UnityEngine;

#region CHANGE LOG
/* December, 7, 2014 - Nathan Hanlan - Added interface ICommandProcessor and class CommandProcessor
 * 
 */
#endregion

namespace Gem
{
    /// <summary>
    /// Main Interface for Command Processors
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>
        /// This method gets called to process a command entered from the debug console.
        /// </summary>
        /// <param name="aWords">All of the words entered as they were typed.</param>
        /// <param name="aLowerWords">All of the words entered but all lowercase.</param>
        void Process(List<string> aWords, List<string> aLowerWords);
    }

    /// <summary>
    /// An implementation example of ICommandProcessor
    /// </summary>
    public class CommandProcessor : ICommandProcessor
    {
#region COMMAND KEYWORDS
        //3
        public const string COMMAND_SET = "set";
        public const string COMMAND_LOG = "log";
        //4
        public const string COMMAND_SHOW = "show";
        public const string COMMAND_HIDE = "hide";
        public const string COMMAND_HELP = "help";
        public const string COMMAND_LOAD = "load";
        public const string COMMAND_QUIT = "quit";
        public const string COMMAND_KILL = "kill";
        //5
        public const string COMMAND_CLEAR = "clear";
        //6
        public const string COMMAND_RELOAD = "reload";
        //7
        public const string COMMAND_RESTART = "restart";
        public const string COMMAND_CONSOLE = "console";
#endregion
#region CONTEXT KEYWORDS
        //4
        public const string CONTEXT_UNIT = "unit";
        //5
        public const string CONTEXT_WORLD = "world";
        //6
        public const string CONTEXT_PLAYER = "player";
        //11 
        public const string CONTEXT_INTERACTIVE = "interactive";

#endregion

        private UIScrollArea m_ScrollArea = null;

        public virtual void Process(List<string> aWords, List<string> aLowerWords)
        {
                string firstWord = aWords[0];
                switch(firstWord.Length)
                {
                    case 3:
                        HandleCommand3(aWords,aLowerWords);
                        break;
                    case 4:
                        HandleCommand4(aWords,aLowerWords);
                        break;
                    case 5:
                        HandleCommand5(aWords,aLowerWords);
                        break;
                    case 6:
                        HandleCommand6(aWords, aLowerWords);
                        break;
                    case 7:
                        HandleCommand7(aWords, aLowerWords);
                        break;
                    case 8:
                        HandleCommand8(aWords, aLowerWords);
                        break;
                        
                }
        }

        #region HANDLE COMMANDS
        protected virtual void HandleCommand3(List<string> aWords, List<string> aLowerWords)
        {
            if(aLowerWords[0] == "add" && aLowerWords.Count > 2)
            {
                if(aLowerWords[1] == "item")
                {
                    if(m_ScrollArea == null)
                    {
                        UIUtils.CreateEventSystem();
                        GameObject canvas = UIUtils.CreateUICanvas("Canvas");
                        GameObject scrollArea = UIUtils.CreateScrollArea(canvas.transform, Vector2.zero, new Vector2(480.0f, 300.0f), 30.0f, false);
                        if(scrollArea != null)
                        {
                            m_ScrollArea = scrollArea.GetComponent<UIScrollArea>();
                        }
                    }

                    if(m_ScrollArea != null)
                    {
                        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                        for(int i = 2; i < aWords.Count; i++)
                        {
                            stringBuilder.Append(aWords[i]);
                            if(i != aWords.Count - 1)
                            {
                                stringBuilder.Append(" ");
                            }
                        }

                        GameObject text = UIUtils.CreateUIText(null, "Message", stringBuilder.ToString(), Vector2.zero, new Vector2(480.0f, 50.0f));
                        m_ScrollArea.AddContent(text);
                    }
                }
            }
        }
        protected virtual void HandleCommand4(List<string> aWords, List<string> aLowerWords)
        {
            if(aLowerWords[0] == "test")
            {
                UIUtils.CreateEventSystem();
                GameObject canvas = UIUtils.CreateUICanvas("Canvas");

                UIUtils.CreateScrollArea(canvas.transform, Vector2.zero, new Vector2(150.0f, 300.0f),30.0f,false);
            }
        }
        protected virtual void HandleCommand5(List<string> aWords, List<string> aLowerWords)
        {
            if(aLowerWords[0] == "error")
            {

            }

        }
        protected virtual void HandleCommand6(List<string> aWords, List<string> aLowerWords)
        {
            if(aLowerWords[0] == "create")
            {
                if(aLowerWords.Count > 2 && aLowerWords[1] == "canvas")
                {
                    UIUtils.CreateUICanvas(aWords[2]);
                }
                else if(aLowerWords.Count > 1 && aLowerWords[1] == "canvas")
                {
                    UIUtils.CreateUICanvas("Test");
                }
                
            }
        }
        protected virtual void HandleCommand7(List<string> aWords, List<string> aLowerWords)
        {

        }
        protected virtual void HandleCommand8(List<string> aWords, List<string> aLowerWords)
        {

        }
        #endregion
    }

}