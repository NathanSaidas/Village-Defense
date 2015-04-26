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
// -- April     17, 2015 - Nathan Hanlan - Added class/file
#endregion
using UnityEngine;
using System.Collections.Generic;

namespace Gem
{

    public enum UIMenuType
    {
        MainMenu,
        LoginWindow,
        LobbyBrowser,
        Options,
        MatchMaking,
        HeroSelection
    }

    public enum SpeedMode
    {
        Fixed,
        Add,
        Multiply,
    }

    /*
    * Suggested setup:
    * StartingSpeed = 1.0, SpeedMode = Fixed - This keeps a 1:1 ratio with the duration. Eg. Duration = 5 second, it'll take 5 seconds for the transition to execute.
    * SpeedMode = Multiple - Very fast increase in speed.
    * SpeedMode = Add - Gradual increase in speed.
    */
    public class UIMenuSystem : MonoBehaviour
    {
        private enum State
        {
            None,
            Previous,
            Next
        }
        public static UIMenuSystem current
        {
            get;
            set;
        }

        /// <summary>
        /// The menu to start with.
        /// </summary>
        [SerializeField]
        private UIMenu m_StartMenu = null;
        /// <summary>
        /// How long should the transition be.
        /// </summary>
        [SerializeField]
        private float m_Duration = 1.0f;
        /// <summary>
        /// The speed to start at.
        /// </summary>
        [SerializeField]
        private float m_StartingSpeed = 1.0f;
        /// <summary>
        /// The type of speed acceleration to user. 
        /// </summary>
        [SerializeField]
        private SpeedMode m_SpeedMode = SpeedMode.Fixed;
        /// <summary>
        /// The modifier used in speed calculations.
        /// </summary>
        [SerializeField]
        private float m_SpeedModifier = 3.0f;



        private State m_CurrentState = State.None;
        private UIMenu m_CurrentMenu = null;
        private UIMenu m_TargetMenu = null;
        private float m_CurrentTime = 0.0f;
        private float m_CurrentSpeed = 0.0f;
        
        private void Awake()
        {
            if (current != null)
            {
                DebugUtils.LogWarning("Multiple UIMenuSystems is not supported.");
            }
            else
            {
                current = this;
            }

        }

        private void Start()
        {
            
            m_CurrentMenu = m_StartMenu;

            if(m_CurrentMenu == null)
            {
                DebugUtils.MissingProperty<RectTransform>("m_StartMenu", gameObject);
            }
        }

        private void OnDestroy()
        {
            if(current == this)
            {
                current = null;
            }
        }

        private void Update()
        {
            switch(m_CurrentState)
            {
                case State.Next:
                    {
                        RectTransform currentTransform = m_CurrentMenu.GetComponent<RectTransform>();
                        RectTransform targetTransform = m_TargetMenu.GetComponent<RectTransform>();

                        float factor = m_CurrentTime / m_Duration;
                        currentTransform.anchoredPosition = Vector2.Lerp(Center(), Left(), factor);
                        targetTransform.anchoredPosition = Vector2.Lerp(Right(), Center(), factor);
                        UpdateSpeed();
                        if (factor >= 1.0f)
                        {
                            OnTransitionComplete();
                        }
                    }
                    break;
                case State.Previous:
                    {
                        RectTransform currentTransform = m_CurrentMenu.GetComponent<RectTransform>();
                        RectTransform targetTransform = m_TargetMenu.GetComponent<RectTransform>();

                        float factor = m_CurrentTime / m_Duration;
                        currentTransform.anchoredPosition = Vector2.Lerp(Center(), Right(), factor);
                        targetTransform.anchoredPosition = Vector2.Lerp(Left(), Center(), factor);
                        UpdateSpeed();

                        if(factor >= 1.0f)
                        {
                            OnTransitionComplete();
                        }
                    }
                    break;
                case State.None:

                    break;
                default:
                    DebugUtils.LogError("Invalid Menu Transition State");
                    m_CurrentState = State.None;
                    break;
            }
        }

        private void UpdateSpeed()
        {
            m_CurrentTime += Time.deltaTime * m_CurrentSpeed;
            switch (m_SpeedMode)
            {
                case SpeedMode.Multiply:
                    m_CurrentSpeed += (m_CurrentSpeed + m_SpeedModifier) * Time.deltaTime;
                    break;
                case SpeedMode.Add:
                    m_CurrentSpeed += (m_CurrentSpeed + m_SpeedModifier) * Time.deltaTime;
                    break;
                case SpeedMode.Fixed:

                    break;
                default:
                    DebugUtils.LogError("Invalid Speed Mode");
                    m_SpeedMode = SpeedMode.Multiply;
                    break;

            }
        }


        public void TransitionNext(UIMenu aMenu)
        {
            if (m_CurrentMenu == null)
            {
                return;
            }

            if(inTransition)
            {
                DebugUtils.LogError("Cannot make a transition while in a transition");
                return;
            }


            //Set Position
            m_TargetMenu = aMenu;
            if(m_TargetMenu != null)
            {
                RectTransform rectTransform = m_TargetMenu.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Right();
                }
                else
                {
                    DebugUtils.LogError("Cannot make a transition, missing RectTransform");
                    return;
                }
                m_TargetMenu.gameObject.SetActive(true);
                m_TargetMenu.OnTransitionBegin();
            }
            //Setup State
            m_CurrentState = State.Next;
            m_CurrentTime = 0.0f;
            m_CurrentSpeed = m_StartingSpeed;
        }
        public void TransitionPrevious(UIMenu aMenu)
        {
            if(m_CurrentMenu == null)
            {
                return;
            }

            if (inTransition)
            {
                DebugUtils.LogError("Cannot make a transition while in a transition");
                return;
            }
            //Set position
            m_TargetMenu = aMenu;
            if (m_TargetMenu != null)
            {
                RectTransform rectTransform = m_TargetMenu.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Left();
                }
                else
                {
                    DebugUtils.LogError("Cannot make a transition, missing RectTransform");
                    return;
                } 
                m_TargetMenu.gameObject.SetActive(true);
                m_TargetMenu.OnTransitionBegin();
            }

            //Set the state
            m_CurrentState = State.Previous;
            m_CurrentTime = 0.0f;
            m_CurrentSpeed = m_StartingSpeed;
        }

        private void OnTransitionComplete()
        {
            //TODO(Nathan): Invoke delegates.

            m_CurrentState = State.None;
            m_CurrentMenu.gameObject.SetActive(false);
            m_CurrentMenu = m_TargetMenu;
            m_CurrentMenu.OnTransitionComplete();
            m_TargetMenu = null;

        }

        private static Vector2 Center()
        {
            return Vector2.zero;
        }

        private static Vector2 Left()
        {
            return new Vector2(-Screen.width * 1.25f, 0.0f);
        }
        private static Vector2 Right()
        {
            return new Vector2(Screen.width * 1.25f, 0.0f);
        }

        public bool inTransition
        {
            get { return m_CurrentState != State.None; }
        }
        public float percentageComplete
        {
            get { return m_CurrentTime / m_Duration; }
        }
        public UIMenu currentMenu
        {
            get { return m_CurrentMenu; }
        }

        public float duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }
        public float startingSpeed
        {
            get { return m_StartingSpeed; }
            set { m_StartingSpeed = value; }
        }
        public SpeedMode speedMode
        {
            get { return m_SpeedMode; }
            set { m_SpeedMode = value; }
        }
        public float speedModifier
        {
            get { return m_SpeedModifier; }
            set { m_SpeedModifier = value; }
        }

    }
}


