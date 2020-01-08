﻿using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Switches can be used to trigger actions based on their current state (on or off). Useful to open doors, chests, portals, bridges...
    /// </summary>
    public class Switch : MonoBehaviour
    {
        [Header("Bindings")]
        /// a SpriteReplace to represent the switch knob when it's on
        public Animator SwitchAnimator;
     
        /// the possible states of the switch
        public enum SwitchStates { On, Off }
        /// the current state of the switch
        public SwitchStates CurrentSwitchState { get; set; }
        [Header("Switch")]
        /// the state the switch should start in
        public SwitchStates InitialState = SwitchStates.Off;

        [Header("Events")]
        /// the methods to call when the switch is turned on
        public UnityEvent SwitchOn;
        /// the methods to call when the switch is turned off
        public UnityEvent SwitchOff;
        /// the methods to call when the switch is toggled
        public UnityEvent SwitchToggle;

        [Header("Feedbacks")]
        /// a feedback to play when the switch is toggled on
        public MMFeedbackLegacy SwitchOnFeedback;
        /// a feedback to play when the switch is turned off
        public MMFeedbackLegacy SwitchOffFeedback;
        /// a feedback to play when the switch changes state
        public MMFeedbackLegacy ToggleFeedback;

        [InspectorButton("TurnSwitchOn")]
        /// a test button to turn the switch on
        public bool SwitchOnButton;
        [InspectorButton("TurnSwitchOff")]
        /// a test button to turn the switch off
        public bool SwitchOffButton;
        [InspectorButton("ToggleSwitch")]
        /// a test button to change the switch's state
        public bool ToggleSwitchButton;

        /// <summary>
        /// On init, we set our current switch state
        /// </summary>
        protected virtual void Start()
        {
            CurrentSwitchState = InitialState;
            SwitchOffFeedback.Initialization(this.gameObject);
            SwitchOnFeedback.Initialization(this.gameObject);
            ToggleFeedback.Initialization(this.gameObject);
        }

        /// <summary>
        /// Turns the switch on
        /// </summary>
        public virtual void TurnSwitchOn()
        {
            CurrentSwitchState = SwitchStates.On;
            if (SwitchOn != null) { SwitchOn.Invoke(); }
            if (SwitchToggle != null) { SwitchToggle.Invoke(); }
            SwitchOnFeedback.Play(this.transform.position, this);
        }

        /// <summary>
        /// Turns the switch off
        /// </summary>
        public virtual void TurnSwitchOff()
        {
            CurrentSwitchState = SwitchStates.Off;
            if (SwitchOff != null) { SwitchOff.Invoke(); }
            if (SwitchToggle != null) { SwitchToggle.Invoke(); }
            SwitchOffFeedback.Play(this.transform.position, this);
        }
        
        /// <summary>
        /// Use this method to go from one state to the other
        /// </summary>
        public virtual void ToggleSwitch()
        {
            if (CurrentSwitchState == SwitchStates.Off)
            {
                CurrentSwitchState = SwitchStates.On;
                if (SwitchOn != null) { SwitchOn.Invoke(); }
                if (SwitchToggle != null) { SwitchToggle.Invoke(); }
                SwitchOnFeedback.Play(this.transform.position, this);
            }
            else
            {
                CurrentSwitchState = SwitchStates.Off;
                if (SwitchOff != null) { SwitchOff.Invoke(); }
                if (SwitchToggle != null) { SwitchToggle.Invoke(); }
                SwitchOffFeedback.Play(this.transform.position, this);
            }
            ToggleFeedback.Play(this.transform.position, this);
        }

        /// <summary>
        /// On Update, we update our switch's animator
        /// </summary>
        protected virtual void Update()
        {
            if (SwitchAnimator != null)
            {
                SwitchAnimator.SetBool("SwitchOn", (CurrentSwitchState == SwitchStates.On));
            }            
        }
    }
}