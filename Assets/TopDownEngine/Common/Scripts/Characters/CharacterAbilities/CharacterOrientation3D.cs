using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Add this ability to a character, and it'll be able to rotate to face the movement's direction or the weapon's rotation
    /// </summary>
	public class CharacterOrientation3D : CharacterAbility 
	{
        /// the possible rotation modes
		public enum RotationModes { None, MovementDirection, WeaponDirection, Both }
        /// the possible rotation speeds
		public enum RotationSpeeds { Instant, Smooth, SmoothAbsolute }

		[Header("Rotation Mode")]
        /// whether the character should face movement direction, weapon direction, or both, or none
		public RotationModes RotationMode = RotationModes.None;

		[Header("Movement Direction")]
		/// If this is true, we'll rotate our model towards the direction
		public bool ShouldRotateToFaceMovementDirection = true;
		/// the current rotation mode
		public RotationSpeeds MovementRotationSpeed = RotationSpeeds.Instant;
		/// the object we want to rotate towards direction. If left empty, we'll use the Character's model
		public GameObject MovementRotatingModel;
		/// the speed at which to rotate towards direction (smooth and absolute only)
		public float RotateToFaceMovementDirectionSpeed = 10f;
		/// the threshold after which we start rotating (absolute mode only)
		public float AbsoluteThresholdMovement = 0.5f;
        [ReadOnly]
        /// the direction of the model
        public Vector3 ModelDirection;
        [ReadOnly]
        /// the direction of the model in angle values
        public Vector3 ModelAngles;

        [Header("Weapon Direction")]
		/// If this is true, we'll rotate our model towards the weapon's direction
		public bool ShouldRotateToFaceWeaponDirection = true;
		/// the current rotation mode
		public RotationSpeeds WeaponRotationSpeed = RotationSpeeds.Instant;
		/// the object we want to rotate towards direction. If left empty, we'll use the Character's model
		public GameObject WeaponRotatingModel;
		/// the speed at which to rotate towards direction (smooth and absolute only)
		public float RotateToFaceWeaponDirectionSpeed = 10f;
		/// the threshold after which we start rotating (absolute mode only)
		public float AbsoluteThresholdWeapon = 0.5f;

		protected CharacterHandleWeapon _characterHandleWeapon;
		protected Vector3 _lastRegisteredVelocity;
		protected Vector3 _rotationDirection;
		protected Vector3 _lastMovement = Vector3.zero;
		protected Vector3 _lastAim = Vector3.zero;

        protected Vector3 _relativeSpeed;
        protected Vector3 _relativeSpeedNormalized;
        protected bool _secondaryMovementTriggered = false;
        protected Vector3 _newMovementModelRotation;
        protected Vector3 _newWeaponModelRotation;

        /// <summary>
        /// On init we grab our model if necessary
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization ();
			if (MovementRotatingModel == null)
			{
				MovementRotatingModel = _model;
			}

			_characterHandleWeapon = GetComponent<CharacterHandleWeapon> ();
			if (WeaponRotatingModel == null)
			{
				WeaponRotatingModel = _model;
			}
		}

		/// <summary>
		/// Every frame we rotate towards the direction
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility ();
			RotateToFaceMovementDirection ();
			RotateToFaceWeaponDirection ();
            RotateModel();
		}


        protected virtual void LateUpdate()
        {
            ComputeRelativeSpeeds();
        }


		/// <summary>
		/// Rotates the player model to face the current direction
		/// </summary>
		protected virtual void RotateToFaceMovementDirection ()
		{
            // if we're not supposed to face our direction, we do nothing and exit
            if (!ShouldRotateToFaceMovementDirection) { return;	}
			if ((RotationMode != RotationModes.MovementDirection) && (RotationMode != RotationModes.Both)) { return; }

            
            // if the rotation mode is instant, we simply rotate to face our direction
            if (MovementRotationSpeed == RotationSpeeds.Instant)
			{
				if (_controller.CurrentDirection != Vector3.zero)
				{
                    _newMovementModelRotation = _controller.CurrentDirection;
				}	
			}

			// if the rotation mode is smooth, we lerp towards our direction
			if (MovementRotationSpeed == RotationSpeeds.Smooth)
			{
				if (_controller.CurrentDirection != Vector3.zero)
				{
                    _newMovementModelRotation = Vector3.Lerp (MovementRotatingModel.transform.forward, _controller.CurrentDirection, RotateToFaceMovementDirectionSpeed * Time.deltaTime);
				}
			}

			// if the rotation mode is smooth, we lerp towards our direction even if the input has been released
			if (MovementRotationSpeed == RotationSpeeds.SmoothAbsolute)
			{
				if (_controller.CurrentDirection.normalized.magnitude >= AbsoluteThresholdMovement)
				{
					_lastMovement = _controller.CurrentDirection;	
				}
				if (_lastMovement != Vector3.zero)
				{
                    _newMovementModelRotation = Vector3.Lerp (MovementRotatingModel.transform.forward, _lastMovement, RotateToFaceMovementDirectionSpeed * Time.deltaTime);	
				}
			}

            ModelDirection = MovementRotatingModel.transform.forward.normalized;
            ModelAngles = MovementRotatingModel.transform.eulerAngles;
        }

        /// <summary>
        /// Rotates the character so it faces the weapon's direction
        /// </summary>
		protected virtual void RotateToFaceWeaponDirection ()
		{
            _newWeaponModelRotation = Vector3.zero;

            // if we're not supposed to face our direction, we do nothing and exit
            if (!ShouldRotateToFaceWeaponDirection)	{	return;		}
			if ((RotationMode != RotationModes.WeaponDirection) && (RotationMode != RotationModes.Both)) { return; }

            if (_characterHandleWeapon == null) { return; }
            if (_characterHandleWeapon.WeaponAimComponent == null) { return;  }

            if (_characterHandleWeapon.WeaponAimComponent.AimControl == WeaponAim.AimControls.SecondaryMovement)
            {
                if ((_inputManager.SecondaryMovement == Vector2.zero))
                {
                    return;
                }
            }
            
            _rotationDirection = _characterHandleWeapon.WeaponAimComponent.CurrentAim.normalized;
            _newWeaponModelRotation = Vector3.zero;

            MMDebug.DebugDrawArrow(this.transform.position, _rotationDirection, Color.red);

            // if the rotation mode is instant, we simply rotate to face our direction
            if (WeaponRotationSpeed == RotationSpeeds.Instant)
			{
				if (_rotationDirection != Vector3.zero)
				{
					_newWeaponModelRotation = _rotationDirection;	
				}						
			}

			// if the rotation mode is smooth, we lerp towards our direction
			if (WeaponRotationSpeed == RotationSpeeds.Smooth)
			{
				if (_rotationDirection != Vector3.zero)
				{
                    _newWeaponModelRotation = Vector3.Lerp (WeaponRotatingModel.transform.forward, _rotationDirection, RotateToFaceWeaponDirectionSpeed * Time.deltaTime);
				}
			}

			// if the rotation mode is smooth, we lerp towards our direction even if the input has been released
			if (WeaponRotationSpeed == RotationSpeeds.SmoothAbsolute)
			{
				if (_rotationDirection.normalized.magnitude >= AbsoluteThresholdWeapon)
				{
					_lastMovement = _rotationDirection;	
				}
				if (_lastMovement != Vector3.zero)
				{
                    _newWeaponModelRotation = Vector3.Lerp (WeaponRotatingModel.transform.forward, _lastMovement, RotateToFaceWeaponDirectionSpeed * Time.deltaTime);	
				}
			}		
		}

        protected virtual void RotateModel()
        {
            if (_newMovementModelRotation != Vector3.zero)
            {
                MovementRotatingModel.transform.forward = _newMovementModelRotation;
            }
            if (_newWeaponModelRotation != Vector3.zero)
            {
                WeaponRotatingModel.transform.forward = _newWeaponModelRotation;
            }            
        }

        protected virtual void ComputeRelativeSpeeds()
        {
            if (_characterHandleWeapon == null)
            {
                _relativeSpeed = MovementRotatingModel.transform.InverseTransformVector(_controller.CurrentMovement);
            }
            else
            {
                _relativeSpeed = WeaponRotatingModel.transform.InverseTransformVector(_controller.CurrentMovement);
            }
            _relativeSpeedNormalized = _relativeSpeed.normalized;
        }

        /// <summary>
        /// Adds required animator parameters to the animator parameters list if they exist
        /// </summary>
        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter("RelativeForwardSpeed", AnimatorControllerParameterType.Float);
            RegisterAnimatorParameter("RelativeLateralSpeed", AnimatorControllerParameterType.Float);
            RegisterAnimatorParameter("RelativeForwardSpeedNormalized", AnimatorControllerParameterType.Float);
            RegisterAnimatorParameter("RelativeLateralSpeedNormalized", AnimatorControllerParameterType.Float);
        }

        /// <summary>
        /// Sends the current speed and the current value of the Walking state to the animator
        /// </summary>
        public override void UpdateAnimator()
        {
            MMAnimator.UpdateAnimatorFloat(_animator, "RelativeForwardSpeed", _relativeSpeed.z, _character._animatorParameters);
            MMAnimator.UpdateAnimatorFloat(_animator, "RelativeLateralSpeed", _relativeSpeed.x, _character._animatorParameters);
            MMAnimator.UpdateAnimatorFloat(_animator, "RelativeForwardSpeedNormalized", _relativeSpeedNormalized.z, _character._animatorParameters);
            MMAnimator.UpdateAnimatorFloat(_animator, "RelativeLateralSpeedNormalized", _relativeSpeedNormalized.x, _character._animatorParameters);
        }
    }
}