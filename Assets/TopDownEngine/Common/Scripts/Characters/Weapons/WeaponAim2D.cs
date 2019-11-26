using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(Weapon))]
    /// <summary>
    /// Add this component to a Weapon and you'll be able to aim it (meaning you'll rotate it)
    /// Supported control modes are mouse, primary movement (you aim wherever you direct your character) and secondary movement (using a secondary axis, separate from the movement).
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Aim 2D")]
    public class WeaponAim2D : WeaponAim
    {
        protected float _lastNonNullXMovement = 0f;
        protected Vector2 _inputMovement;

        /// the current angle the weapon is aiming at, adjusted to compensate for the current orientation of the character
        public override float CurrentAngleRelative
        {
            get
            {
                if (_weapon != null)
                {
                    if (_weapon.Owner != null)
                    {
                        if (_weapon.Owner.Orientation2D != null)
                        {
                            if (_weapon.Owner.Orientation2D.IsFacingRight)
                            {
                                return CurrentAngle;
                            }
                            else
                            {
                                return -CurrentAngle;
                            }                        
                        }
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// Computes the current aim direction
        /// </summary>
        protected override void GetCurrentAim()
        {
            if (_weapon.Owner == null)
            {
                return;
            }

            if ((_weapon.Owner.LinkedInputManager == null) && (_weapon.Owner.CharacterType == Character.CharacterTypes.Player))
            {
                return;
            }

            AutoDetectWeaponMode();

            switch (AimControl)
            {
                case AimControls.Off:
                    if (_weapon.Owner == null) { return; }

                    _currentAim = Vector2.right;
                    _currentAimAbsolute = Vector2.right;
                    _direction = Vector2.right;
                   
                    break;

                case AimControls.Script:
                    _currentAim = (_weapon.Owner.Orientation2D.IsFacingRight) ? _currentAim : -_currentAim;
                    _currentAimAbsolute = _currentAim;
                    _direction = -(transform.position - _currentAim);
                    break;

                case AimControls.PrimaryMovement:
                    if (_weapon.Owner == null)
                    {
                        return;
                    }

                    _inputMovement = _weapon.Owner.LinkedInputManager.PrimaryMovement;
                    _inputMovement.x = Mathf.Abs(_inputMovement.x) > 0 ? _inputMovement.x : _lastNonNullXMovement;

                    _currentAimAbsolute = _inputMovement;

                    if (_weapon.Owner.Orientation2D.IsFacingRight)
                    {
                        _currentAim = _inputMovement;
                        _direction = transform.position + _currentAim;
                    }
                    else
                    {
                        _currentAim = -_inputMovement;
                        _direction = -(transform.position - _currentAim);
                    }

                    _lastNonNullXMovement = Mathf.Abs(_inputMovement.x) > 0 ? _inputMovement.x : _lastNonNullXMovement;
                    break;

                case AimControls.SecondaryMovement:
                    if (_weapon.Owner == null) { return; }

                    _inputMovement = _weapon.Owner.LinkedInputManager.SecondaryMovement;
                    _inputMovement.x = Mathf.Abs(_inputMovement.x) > 0 ? _inputMovement.x : _lastNonNullXMovement;

                    _currentAimAbsolute = _inputMovement;

                    if (_weapon.Owner.Orientation2D.IsFacingRight)
                    {
                        _currentAim = _inputMovement;
                        _direction = transform.position + _currentAim;
                    }
                    else
                    {
                        _currentAim = -_inputMovement;
                        _direction = -(transform.position - _currentAim);
                    }

                    _lastNonNullXMovement = Mathf.Abs(_inputMovement.x) > 0 ? _inputMovement.x : _lastNonNullXMovement;
                    break;

                case AimControls.Mouse:
                    if (_weapon.Owner == null)
                    {
                        return;
                    }

                    _mousePosition = Input.mousePosition;
                    _mousePosition.z = 10;


                    _direction = Camera.main.ScreenToWorldPoint(_mousePosition);
                    _direction.z = transform.position.z;

                    _reticlePosition = _direction;

                    _currentAimAbsolute = _direction - transform.position;
                    if (_weapon.Owner.Orientation2D.IsFacingRight)
                    {
                        _currentAim = _direction - transform.position;
                        _currentAimAbsolute = _currentAim;
                    }
                    else
                    {
                        _currentAim = transform.position - _direction;
                    }
                    break;
            }
        }

        /// <summary>
        /// Every frame, we compute the aim direction and rotate the weapon accordingly
        /// </summary>
        protected override void Update()
        {
            GetCurrentAim();
            DetermineWeaponRotation();
            MoveReticle();
            HideReticle();
        }

        /// <summary>
        /// Determines the weapon rotation based on the current aim direction
        /// </summary>
        protected override void DetermineWeaponRotation()
        {
            if (_currentAim != Vector3.zero)
            {
                if (_direction != Vector3.zero)
                {
                    CurrentAngle = Mathf.Atan2(_currentAim.y, _currentAim.x) * Mathf.Rad2Deg;
                    CurrentAngleAbsolute = Mathf.Atan2(_currentAimAbsolute.y, _currentAimAbsolute.x) * Mathf.Rad2Deg;
                    if (RotationMode == RotationModes.Strict4Directions || RotationMode == RotationModes.Strict8Directions)
                    {
                        CurrentAngle = MMMaths.RoundToClosest(CurrentAngle, _possibleAngleValues);
                    }
                    if (RotationMode == RotationModes.Strict2Directions)
                    {
                        CurrentAngle = 0f;
                    }

                    // we add our additional angle
                    CurrentAngle += _additionalAngle;

                    // we clamp the angle to the min/max values set in the inspector
                    if (_weapon.Owner.Orientation2D.IsFacingRight)
                    {
                        CurrentAngle = Mathf.Clamp(CurrentAngle, MinimumAngle, MaximumAngle);
                    }
                    else
                    {
                        CurrentAngle = Mathf.Clamp(CurrentAngle, -MaximumAngle, -MinimumAngle);
                    }
                    _lookRotation = Quaternion.Euler(CurrentAngle * Vector3.forward);
                    RotateWeapon(_lookRotation);
                }
            }
            else
            {
                CurrentAngle = 0f;
                RotateWeapon(_initialRotation);
            }
            MMDebug.DebugDrawArrow(this.transform.position, _currentAimAbsolute.normalized, Color.green);
        }
        
        /// <summary>
        /// If a reticle has been set, instantiates the reticle and positions it
        /// </summary>
        protected override void InitializeReticle()
        {
            if (_weapon.Owner == null) { return; }
            if (Reticle == null) { return; }
            if (ReticleType == ReticleTypes.None) { return; }

            if (ReticleType == ReticleTypes.Scene)
            {
                _reticle = (GameObject)Instantiate(Reticle);

                if (!ReticleAtMousePosition)
                {
                    if (_weapon.Owner != null)
                    {
                        _reticle.transform.SetParent(_weapon.transform);
                        _reticle.transform.localPosition = ReticleDistance * Vector3.right;
                    }
                }
                
            }

            if (ReticleType == ReticleTypes.UI)
            {
                _reticle = (GameObject)Instantiate(Reticle);
                _reticle.transform.SetParent(GUIManager.Instance.MainCanvas.transform);
                _reticle.transform.localScale = Vector3.one;
                if (_reticle.gameObject.GetComponentNoAlloc<MMUIFollowMouse>() != null)
                {
                    _reticle.gameObject.GetComponentNoAlloc<MMUIFollowMouse>().TargetCanvas = GUIManager.Instance.MainCanvas;
                }
            }
        }

        /// <summary>
        /// Every frame, moves the reticle if it's been told to follow the pointer
        /// </summary>
        protected override void MoveReticle()
        {
            if (ReticleType == ReticleTypes.None) { return; }
            if (_reticle == null) { return; }

            if (ReticleType == ReticleTypes.Scene)
            {
                // if we're not supposed to rotate the reticle, we force its rotation, otherwise we apply the current look rotation
                if (!RotateReticle)
                {
                    _reticle.transform.rotation = Quaternion.identity;
                }
                else
                {
                    if (ReticleAtMousePosition)
                    {
                        _reticle.transform.rotation = _lookRotation;
                    }
                }

                // if we're in follow mouse mode and the current control scheme is mouse, we move the reticle to the mouse's position
                if (ReticleAtMousePosition && AimControl == AimControls.Mouse)
                {
                    _reticle.transform.position = _reticlePosition;
                }

                if (MoveCameraTargetTowardsReticle && (_weapon.Owner != null))
                {
                    _newCamTargetPosition = Vector3.Lerp(this.transform.position, _reticlePosition, CameraTargetOffset);
                    _newCamTargetDirection = _newCamTargetPosition - this.transform.position;
                    if (_newCamTargetDirection.magnitude > CameraTargetMaxDistance)
                    {
                        _newCamTargetDirection = _newCamTargetDirection.normalized * CameraTargetMaxDistance;
                    }
                    _newCamTargetPosition = this.transform.position + _newCamTargetDirection;
                    _weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
                }
            }
        }

        /// <summary>
        /// Hides or show the mouse pointer based on the settings
        /// </summary>
        protected virtual void HideReticle()
        {
            if (GameManager.Instance.Paused)
            {
                Cursor.visible = true;
                return;
            }
            if (ReplaceMousePointer)
            {
                Cursor.visible = false;
            }
            else
            {
                Cursor.visible = true;
            }
        }
    }
}