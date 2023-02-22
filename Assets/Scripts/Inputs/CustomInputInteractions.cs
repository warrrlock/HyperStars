
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem.Editor;
#endif

//TODOTODOTODO: TAP THEN TAP SOMETHING ELSE CANCELS

////TODO: add ability to respond to any of the taps in the sequence (e.g. one response for single tap, another for double tap)

////TODO: add ability to perform on final press rather than on release

////TODO: change this so that the interaction stays performed when the tap count is reached until the button is released

namespace UnityEngine.InputSystem.Interactions
{
    //#if UNITY_EDITOR
    //[InitializeOnLoad]
    //#endif
    public class InterruptableInteraction : IInputInteraction<float>
    {
        public void Process(ref InputInteractionContext context)
        {
        }

        /// <inheritdoc />
        public void Reset()
        {
        }
    }




    ////REVIEW: Why is this deriving from IInputInteraction<float>? It goes by actuation just like Hold etc.
    /// <summary>
    /// Interaction that requires multiple taps (press and release within <see cref="tapTime"/>) spaced no more
    /// than <see cref="tapDelay"/> seconds apart. This equates to a chain of <see cref="TapInteraction"/> with
    /// a maximum delay between each tap.
    /// </summary>
    /// <remarks>
    /// The interaction goes into <see cref="InputActionPhase.Started"/> on the first press and then will not
    /// trigger again until either the full tap sequence is performed (in which case the interaction triggers
    /// <see cref="InputActionPhase.Performed"/>) or the multi-tap is aborted by a timeout being hit (in which
    /// case the interaction will trigger <see cref="InputActionPhase.Canceled"/>).
    /// </remarks>
    ////#if UNITY_EDITOR
    //[InitializeOnLoad]
    ////#endif
    public class MultiTapDownInteraction : IInputInteraction<System.Single>
    {
        #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        #endif
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {
            InputSystem.RegisterInteraction<MultiTapDownInteraction>();
        }

        //static MultiTapDownInteraction()
        //{
        //    InputSystem.RegisterInteraction<MultiTapDownInteraction>();
        //}

        /// <summary>
        /// The time in seconds within which the control needs to be pressed and released to perform the interaction.
        /// </summary>
        /// <remarks>
        /// If this value is equal to or smaller than zero, the input system will use (<see cref="InputSettings.defaultTapTime"/>) instead.
        /// </remarks>
        [Tooltip("The maximum time (in seconds) allowed to elapse between pressing and releasing a control for it to register as a tap.")]
        public float tapTime;

        /// <summary>
        /// The time in seconds which is allowed to pass between taps.
        /// </summary>
        /// <remarks>
        /// If this time is exceeded, the multi-tap interaction is canceled.
        /// If this value is equal to or smaller than zero, the input system will use the duplicate value of <see cref="tapTime"/> instead.
        /// </remarks>
        [Tooltip("The maximum delay (in seconds) allowed between each tap. If this time is exceeded, the multi-tap is canceled.")]
        public float tapDelay;

        /// <summary>
        /// The number of taps required to perform the interaction.
        /// </summary>
        /// <remarks>
        /// How many taps need to be performed in succession. Two means double-tap, three means triple-tap, and so on.
        /// </remarks>
        [Tooltip("How many taps need to be performed in succession. Two means double-tap, three means triple-tap, and so on.")]
        public int tapCount = 2;

        /// <summary>
        /// Magnitude threshold that must be crossed by an actuated control for the control to
        /// be considered pressed.
        /// </summary>
        /// <remarks>
        /// If this is less than or equal to 0 (the default), <see cref="InputSettings.defaultButtonPressPoint"/> is used instead.
        /// </remarks>
        /// <seealso cref="InputControl.EvaluateMagnitude()"/>
        public float pressPoint;


        public bool isInterruptionAllowed;

        private float tapTimeOrDefault => tapTime > 0.0 ? tapTime : InputSystem.settings.defaultTapTime;
        internal float tapDelayOrDefault => tapDelay > 0.0 ? tapDelay : InputSystem.settings.multiTapDelayTime;
        //private float pressPointOrDefault => pressPoint > 0 ? pressPoint : ButtonControl.s_GlobalDefaultButtonPressPoint;
        //private float releasePointOrDefault => pressPointOrDefault * ButtonControl.s_GlobalDefaultButtonReleaseThreshold;
        private float pressPointOrDefault => pressPoint;
        private float releasePointOrDefault => pressPointOrDefault;


        private bool _isInterrupted;

        private enum InterruptedState { Base, Uninterrupted, Interrupted }
        private InterruptedState _interruptedState = InterruptedState.Base;

        private InputControl _lastControl = null;

        //public MultiTapDownInteraction()
        //{
        //    InputSystem.onActionChange += (action, change) =>
        //    {
        //        if (action.GetType() != typeof(InputAction))
        //        {
        //            return;
        //        }
        //        InputAction act = action as InputAction;
        //        if (change != InputActionChange.ActionPerformed)
        //        {
        //            return;
        //        }

        //        if (_lastControl == null)
        //        {
        //            Debug.Log("charles");
        //            _lastControl = act.activeControl;
        //        }
        //        else
        //        {
        //            Debug.Log("pratt");
        //            Debug.Log("active control: " + act.activeControl);
        //            if (act.activeControl == _lastControl)
        //            {
        //                _interruptedState = InterruptedState.Uninterrupted;
        //                return;
        //            }
        //            else
        //            {
        //                _interruptedState = InterruptedState.Interrupted;
        //                //_lastControl = act.activeControl;
        //            }
        //        }

        //        //if (act.interactions.Contains("Down"))
        //        //{
        //        //    Debug.Log("nis");
        //        //    _interruptedState = InterruptedState.Uninterrupted;
        //        //    return;
        //        //}
        //        //_interruptedState = InterruptedState.Interrupted;
        //        ////if (_interruptedState == InterruptedState.Base)
        //        ////{

        //        ////}
        //    };
        //}


        /// <inheritdoc />
        public void Process(ref InputInteractionContext context)
        {
            if (context.timerHasExpired)
            {
                // We use timers multiple times but no matter what, if they expire it means
                // that we didn't get input in time.
                _interruptedState = InterruptedState.Base;
                _lastControl = null;
                context.Canceled();
                return;
            }

            switch (m_CurrentTapPhase)
            {
                case TapPhase.None:
                    if (context.ControlIsActuated(pressPointOrDefault))
                    {
                        m_CurrentTapPhase = TapPhase.WaitingForNextRelease;
                        m_CurrentTapStartTime = context.time;
                        context.Started();
                        ++m_CurrentTapCount;
                        m_LastTapTime = context.time;

                        var maxTapTime = tapTimeOrDefault;
                        var maxDelayInBetween = tapDelayOrDefault;
                        context.SetTimeout(maxTapTime);

                        // We'll be using multiple timeouts so set a total completion time that
                        // effects the result of InputAction.GetTimeoutCompletionPercentage()
                        // such that it accounts for the total time we allocate for the interaction
                        // rather than only the time of one single timeout.
                        context.SetTotalTimeoutCompletionTime(maxTapTime * tapCount + (tapCount - 1) * maxDelayInBetween);
                    }
                    break;

                //case TapPhase.WaitingForNextRelease:
                //    if (!context.ControlIsActuated(releasePointOrDefault))
                //    {
                //        if (context.time - m_CurrentTapStartTime <= tapTimeOrDefault)
                //        {
                //            ++m_CurrentTapCount;
                //            if (m_CurrentTapCount >= tapCount)
                //            {
                //                context.Performed();
                //            }
                //            else
                //            {
                //                m_CurrentTapPhase = TapPhase.WaitingForNextPress;
                //                m_LastTapReleaseTime = context.time;
                //                context.SetTimeout(tapDelayOrDefault);
                //            }
                //        }
                //        else
                //        {
                //            context.Canceled();
                //        }
                //    }
                //    break;

                //case TapPhase.WaitingForNextPress:
                //    if (context.ControlIsActuated(pressPointOrDefault))
                //    {
                //        if (context.time - m_LastTapReleaseTime <= tapDelayOrDefault)
                //        {
                //            m_CurrentTapPhase = TapPhase.WaitingForNextRelease;
                //            m_CurrentTapStartTime = context.time;
                //            context.SetTimeout(tapTimeOrDefault);
                //        }
                //        else
                //        {
                //            context.Canceled();
                //        }
                //    }
                //    break;

                case TapPhase.WaitingForNextRelease:
                    if (!context.ControlIsActuated(releasePointOrDefault))
                    {
                        if (context.time - m_LastTapTime <= tapDelay)
                        {
                            m_CurrentTapPhase = TapPhase.WaitingForNextPress;
                            context.SetTimeout(tapDelayOrDefault);
                        }
                        else
                        {
                            _lastControl = null;
                            context.Canceled();
                        }
                    }
                    break;

                case TapPhase.WaitingForNextPress:
                    if (_interruptedState == InterruptedState.Interrupted)
                    {
                        //Debug.Log("peen");
                        _interruptedState = InterruptedState.Base;
                        _lastControl = null;
                        context.Canceled();
                    }

                    //if (_interruptedState == InterruptedState.Uninterrupted)
                    //{

                    //}


                    if (context.ControlIsActuated(pressPointOrDefault))
                    {
                        if (context.time - m_LastTapTime <= tapDelay)
                        {
                            ++m_CurrentTapCount;
                            if (m_CurrentTapCount >= tapCount)
                            {
                                context.Performed();
                            }
                            else
                            {
                                m_CurrentTapPhase = TapPhase.WaitingForNextRelease;
                                m_LastTapTime = context.time;
                                context.SetTimeout(tapDelayOrDefault);
                            }
                        }
                        else
                        {
                            context.Canceled();
                        }
                    }
                    break;
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            m_CurrentTapPhase = TapPhase.None;
            m_CurrentTapCount = 0;
            m_CurrentTapStartTime = 0;
            m_LastTapTime = 0;

            _isInterrupted = false;
            _interruptedState = InterruptedState.Base;
            _lastControl = null;
        }

        private TapPhase m_CurrentTapPhase;
        private int m_CurrentTapCount;
        private double m_CurrentTapStartTime;
        private double m_LastTapTime;

        private enum TapPhase
        {
            None,
            WaitingForNextRelease,
            WaitingForNextPress,
        }
    }

    ////REVIEW: Why is this deriving from IInputInteraction<float>? It goes by actuation just like Hold etc.
    /// <summary>
    /// Interaction that requires multiple taps (press and release within <see cref="tapTime"/>) spaced no more
    /// than <see cref="tapDelay"/> seconds apart. This equates to a chain of <see cref="TapInteraction"/> with
    /// a maximum delay between each tap.
    /// </summary>
    /// <remarks>
    /// The interaction goes into <see cref="InputActionPhase.Started"/> on the first press and then will not
    /// trigger again until either the full tap sequence is performed (in which case the interaction triggers
    /// <see cref="InputActionPhase.Performed"/>) or the multi-tap is aborted by a timeout being hit (in which
    /// case the interaction will trigger <see cref="InputActionPhase.Canceled"/>).
    /// </remarks>
    //#if UNITY_EDITOR
    //[InitializeOnLoad]
    //#endif
    public class Joystick : IInputInteraction
    {
        #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        #endif
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {
            InputSystem.RegisterInteraction<Joystick>();
        }

        //static Joystick()
        //{
        //    InputSystem.RegisterInteraction<Joystick>();
        //}

        /// <summary>
        /// Magnitude threshold that must be crossed by an actuated control for the control to
        /// be considered pressed.
        /// </summary>
        /// <remarks>
        /// If this is less than or equal to 0 (the default), <see cref="InputSettings.defaultButtonPressPoint"/> is used instead.
        /// </remarks>
        /// <seealso cref="InputControl.EvaluateMagnitude()"/>
        public float pressPoint;


        //public bool isInterruptionAllowed;

        //private float tapTimeOrDefault => tapTime > 0.0 ? tapTime : InputSystem.settings.defaultTapTime;
        //internal float tapDelayOrDefault => tapDelay > 0.0 ? tapDelay : InputSystem.settings.multiTapDelayTime;
        ////private float pressPointOrDefault => pressPoint > 0 ? pressPoint : ButtonControl.s_GlobalDefaultButtonPressPoint;
        ////private float releasePointOrDefault => pressPointOrDefault * ButtonControl.s_GlobalDefaultButtonReleaseThreshold;
        private float pressPointOrDefault => pressPoint;
        //private float releasePointOrDefault => pressPointOrDefault;

        /// <inheritdoc />
        public void Process(ref InputInteractionContext context)
        {
            //if (context.ControlIsActuated(pressPointOrDefault))
            //{
            //    context.PerformedAndStayPerformed();
            //}
            //else
            //{
            //    context.Canceled();
            //}

            if (context.ControlIsActuated(pressPointOrDefault))
            {
                context.PerformedAndStayPerformed();
                m_CurrentTapPhase = TapPhase.WaitingForNextRelease;
            }
            else if (m_CurrentTapPhase == TapPhase.WaitingForNextRelease)
            {
                context.Canceled();
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            m_CurrentTapPhase = TapPhase.None;
            //m_CurrentTapCount = 0;
            //m_CurrentTapStartTime = 0;
            //m_LastTapTime = 0;

            //_isInterrupted = false;
            //_interruptedState = InterruptedState.Base;
        }

        private TapPhase m_CurrentTapPhase;
        //private int m_CurrentTapCount;
        //private double m_CurrentTapStartTime;
        //private double m_LastTapTime;

        private enum TapPhase
        {
            None,
            WaitingForNextRelease
        }
    }
}