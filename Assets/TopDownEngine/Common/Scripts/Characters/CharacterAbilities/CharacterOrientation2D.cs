using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Add this ability to a character and it'll rotate or flip to face the direction of movement or the weapon's, or both, or none
    /// Only add this ability to a 2D character
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Orientation 2D")]
    public class CharacterOrientation2D : CharacterAbility
    {
        /// the possible facing modes
        public enum FacingModes { None, MovementDirection, WeaponDirection, Both }
        /// the facing mode for this character
        public FacingModes FacingMode = FacingModes.None;
        
        [Information("You can also decide if the character must automatically flip when going backwards or not. Additionnally, if you're not using sprites, you can define here how the character's model's localscale will be affected by flipping. By default it flips on the x axis, but you can change that to fit your model.", MoreMountains.Tools.InformationAttribute.InformationType.Info, false)]
        /// whether we should flip the model's scale when the character changes direction or not		
        public bool ModelShouldFlip = false;
        [Condition("ModelShouldFlip", true)]
        /// the scale value to apply to the model when facing left
        public Vector3 ModelFlipValueLeft = new Vector3(-1, 1, 1);
        [Condition("ModelShouldFlip", true)]
        /// the scale value to apply to the model when facing right
        public Vector3 ModelFlipValueRight = new Vector3(1, 1, 1);
        
        /// whether we should rotate the model on direction change or not		
        public bool ModelShouldRotate;
        [Condition("ModelShouldRotate", true)]
        /// the rotation to apply to the model when it changes direction		
        public Vector3 ModelRotationValueLeft = new Vector3(0f, 180f, 0f);
        [Condition("ModelShouldRotate", true)]
        /// the rotation to apply to the model when it changes direction		
        public Vector3 ModelRotationValueRight = new Vector3(0f, 0f, 0f);
        [Condition("ModelShouldRotate", true)]
        /// the speed at which to rotate the model when changing direction, 0f means instant rotation		
        public float ModelRotationSpeed = 0f;

        [Header("Direction")]
        /// true if the player is facing right
        [Information("It's usually good practice to build all your characters facing right. If that's not the case of this character, select Left instead.", MoreMountains.Tools.InformationAttribute.InformationType.Info, false)]
        public Character.FacingDirections InitialFacingDirection = Character.FacingDirections.Right;
        /// the threshold at which movement is considered
        public float AbsoluteThresholdMovement = 0.5f;
        /// the threshold at which weapon gets considered
        public float AbsoluteThresholdWeapon = 0.5f;

        [ReadOnly]
        /// whether or not this character is facing right
        public bool IsFacingRight = true;

        protected Vector3 _targetModelRotation;
        protected CharacterHandleWeapon _characterHandleWeapon;
        protected Vector3 _lastRegisteredVelocity;
        protected Vector3 _rotationDirection;
        protected Vector3 _lastMovement = Vector3.zero;
        protected Vector3 _lastAim = Vector3.zero;
        protected float _lastNonNullXMovement;        
        protected int _direction;
        protected int _directionLastFrame = 0;

        /// <summary>
        /// On awake we init our facing direction and grab components
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if (InitialFacingDirection == Character.FacingDirections.Left)
            {
                IsFacingRight = false;
                _direction = -1;
            }
            else
            {
                IsFacingRight = true;
                _direction = 1;
            }
            _directionLastFrame = 0;
            _characterHandleWeapon = this.gameObject.GetComponent<CharacterHandleWeapon>();
        }

        /// <summary>
        /// On process ability, we flip to face the direction set in settings
        /// </summary>
        public override void ProcessAbility()
        {
            base.ProcessAbility();

            if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
            {
                return;
            }

            FlipToFaceMovementDirection();
            FlipToFaceWeaponDirection();
            ApplyModelRotation();
            FlipAbilities();

            _directionLastFrame = _direction;
            _lastNonNullXMovement = (Mathf.Abs(_controller.CurrentMovement.x) > 0) ? _controller.CurrentMovement.x : _lastNonNullXMovement;
        }

        /// <summary>
        /// If the model should rotate, we modify its rotation 
        /// </summary>
        protected virtual void ApplyModelRotation()
        {
            if (!ModelShouldRotate)
            {
                return;
            }

            if (ModelRotationSpeed > 0f)
            {
                _character.CharacterModel.transform.localEulerAngles = Vector3.Lerp(_character.CharacterModel.transform.localEulerAngles, _targetModelRotation, Time.deltaTime * ModelRotationSpeed);
            }
            else
            {
                _character.CharacterModel.transform.localEulerAngles = _targetModelRotation;
            }
        }

        /// <summary>
        /// Flips the object to face direction
        /// </summary>
        protected virtual void FlipToFaceMovementDirection()
        {
            // if we're not supposed to face our direction, we do nothing and exit
			if ((FacingMode != FacingModes.MovementDirection) && (FacingMode != FacingModes.Both)) { return; }

            if (_controller.CurrentMovement.normalized.magnitude >= AbsoluteThresholdMovement)
            {
                float checkedDirection = (Mathf.Abs(_controller.CurrentMovement.normalized.x) > 0) ? _controller.CurrentMovement.normalized.x : _lastNonNullXMovement;
                
                if (checkedDirection >= 0)
                {
                    FaceDirection(1);
                }
                else
                {
                    FaceDirection(-1);
                }
            }                
        }

        /// <summary>
        /// Flips the character to face the current weapon direction
        /// </summary>
        protected virtual void FlipToFaceWeaponDirection()
        {
            if (_characterHandleWeapon == null)
            {
                return;
            }
            // if we're not supposed to face our direction, we do nothing and exit
            if ((FacingMode != FacingModes.WeaponDirection) && (FacingMode != FacingModes.Both)) { return; }
            
            if (_characterHandleWeapon.WeaponAimComponent != null)
            {
                float weaponAngle = _characterHandleWeapon.WeaponAimComponent.CurrentAngleAbsolute;
                weaponAngle = IsFacingRight ? weaponAngle : -weaponAngle;
                
                if ((weaponAngle > 90) || (weaponAngle < -90))
                {
                    FaceDirection(-1);
                }
                else
                {
                    FaceDirection(1);
                }
            }            
        }

        /// <summary>
		/// Flips the character and its dependencies (jetpack for example) horizontally
		/// </summary>
		public virtual void FaceDirection(int direction)
        {
            if (ModelShouldFlip)
            {
                FlipModel(direction);
            }

            if (ModelShouldRotate)
            {
                RotateModel(direction);
            }

            _direction = direction;
            IsFacingRight = _direction == 1;
        }

        protected virtual void RotateModel(int direction)
        {
            if (_character.CharacterModel != null)
            {
                _targetModelRotation = (direction == 1) ? ModelRotationValueRight : ModelRotationValueLeft;
                _targetModelRotation.x = _targetModelRotation.x % 360;
                _targetModelRotation.y = _targetModelRotation.y % 360;
                _targetModelRotation.z = _targetModelRotation.z % 360;
            }
        }

        /// <summary>
        /// Flips the model only, no impact on weapons or attachments
        /// </summary>
        public virtual void FlipModel(int direction)
        {
            if (_character.CharacterModel != null)
            {
                _character.CharacterModel.transform.localScale = (direction == 1) ? ModelFlipValueRight : ModelFlipValueLeft;
            }
            else
            {
                _spriteRenderer.flipX = (direction == -1);
            }
        }

        /// <summary>
        /// Sends a flip event on all other abilities
        /// </summary>
        protected virtual void FlipAbilities()
        {
            if ((_directionLastFrame != 0) && (_directionLastFrame != _direction))
            {
                _character.FlipAllAbilities();
            }
        }

    }
}
