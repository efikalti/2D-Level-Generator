using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// This base class, meant to be extended (see ProjectileWeapon.cs for an example of that) handles rate of fire (rate of use actually), and ammo reloading
    /// </summary>
    [SelectionBase]
    public class Weapon : MonoBehaviour
    {
        /// the name of the weapon, only used for debugging
        public string WeaponName;
        /// the possible use modes for the trigger
        public enum TriggerModes { SemiAuto, Auto }
        /// the possible states the weapon can be in
        public enum WeaponStates { WeaponIdle, WeaponStart, WeaponDelayBeforeUse, WeaponUse, WeaponDelayBetweenUses, WeaponStop, WeaponReloadNeeded, WeaponReloadStart, WeaponReload, WeaponReloadStop, WeaponInterrupted }

        [ReadOnly]
        /// whether or not the weapon is currently active
        public bool WeaponCurrentlyActive = true;

        [Header("Use")]
        /// is this weapon on semi or full auto ?
        public TriggerModes TriggerMode = TriggerModes.Auto;
        /// the delay before use, that will be applied for every shot
        public float DelayBeforeUse = 0f;
        /// whether or not the delay before used can be interrupted by releasing the shoot button (if true, releasing the button will cancel the delayed shot)
        public bool DelayBeforeUseReleaseInterruption = true;
        /// the time (in seconds) between two shots		
        public float TimeBetweenUses = 1f;
        /// whether or not the time between uses can be interrupted by releasing the shoot button (if true, releasing the button will cancel the time between uses)
        public bool TimeBetweenUsesReleaseInterruption = true;

        [Header("Magazine")]
        /// whether or not the weapon is magazine based. If it's not, it'll just take its ammo inside a global pool
        public bool MagazineBased = false;
        /// the size of the magazine
        public int MagazineSize = 30;
        /// if this is true, pressing the fire button when a reload is needed will reload the weapon. Otherwise you'll need to press the reload button
        public bool AutoReload;
        /// the time it takes to reload the weapon
        public float ReloadTime = 2f;
        /// the amount of ammo consumed everytime the weapon fires
        public int AmmoConsumedPerShot = 1;
        /// if this is set to true, the weapon will auto destroy when there's no ammo left
        public bool AutoDestroyWhenEmpty;
        /// the delay (in seconds) before weapon destruction if empty
        public float AutoDestroyWhenEmptyDelay = 1f;
        [ReadOnly]
        /// the current amount of ammo loaded inside the weapon
        public int CurrentAmmoLoaded = 0;

        [Header("Position")]
        /// an offset that will be applied to the weapon once attached to the center of the WeaponAttachment transform.
        public Vector3 WeaponAttachmentOffset = Vector3.zero;
        /// should that weapon be flipped when the character flips ?
        public bool FlipWeaponOnCharacterFlip = true;
        /// the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs
        public Vector3 RightFacingFlipValue = new Vector3(1, 1, 1);
        public Vector3 LeftFacingFlipValue = new Vector3(-1, 1, 1);

        [Header("Hands Position")]
        /// the transform to which the character's left hand should be attached to
        public Transform LeftHandHandle;
        /// the transform to which the character's right hand should be attached to
        public Transform RightHandHandle;

        [Header("Movement")]
        /// if this is true, a multiplier will be applied to movement while the weapon is active
        public bool ModifyMovementWhileAttacking = false;
        /// the multiplier to apply to movement while attacking
        public float MovementMultiplier = 0f;
        /// if this is true all movement will be prevented (even flip) while the weapon is active
        public bool PreventAllMovementWhileInUse = false;

        [Header("Recoil")]
        /// the force to apply to push the character back when shooting
        public float RecoilForce = 0f;

        [Header("Animation")]
        /// the other animators (other than the Character's) that you want to update every time this weapon gets used
        public List<Animator> Animators;

        [Header("Animation Parameters Names")]
        /// the ID of the weapon to pass to the animator
        public int WeaponAnimationID = 0;
        /// the name of the weapon's idle animation parameter : this will be true all the time except when the weapon is being used
        public string IdleAnimationParameter;
        /// the name of the weapon's start animation parameter : true at the frame where the weapon starts being used
        public string StartAnimationParameter;
        /// the name of the weapon's delay before use animation parameter : true when the weapon has been activated but hasn't been used yet
        public string DelayBeforeUseAnimationParameter;
        /// the name of the weapon's single use animation parameter : true at each frame the weapon activates (shoots)
        public string SingleUseAnimationParameter;
        /// the name of the weapon's in use animation parameter : true at each frame the weapon has started firing but hasn't stopped yet
        public string UseAnimationParameter;
        /// the name of the weapon's delay between each use animation parameter : true when the weapon is in use
        public string DelayBetweenUsesAnimationParameter;
        /// the name of the weapon stop animation parameter : true after a shot and before the next one or the weapon's stop 
        public string StopAnimationParameter;
        /// the name of the weapon reload start animation parameter
        public string ReloadStartAnimationParameter;
        /// the name of the weapon reload animation parameter
        public string ReloadAnimationParameter;
        /// the name of the weapon reload end animation parameter
        public string ReloadStopAnimationParameter;
        /// the name of the weapon's angle animation parameter
        public string WeaponAngleAnimationParameter;
        /// the name of the weapon's angle animation parameter, adjusted so it's always relative to the direction the character is currently facing
        public string WeaponAngleRelativeAnimationParameter;
        
        [Header("Feedbacks")]
        /// the feedback to play when the weapon starts being used
        public MMFeedbacks WeaponStartMMFeedback;
        /// the feedback to play while the weapon is in use
        public MMFeedbacks WeaponUsedMMFeedback;
        /// the feedback to play when the weapon stops being used
        public MMFeedbacks WeaponStopMMFeedback;
        /// the feedback to play when the weapon gets reloaded
        public MMFeedbacks WeaponReloadMMFeedback;
        /// the feedback to play when the weapon gets reloaded
        public MMFeedbacks WeaponReloadNeededMMFeedback;
        
        [Header("Settings")]
        /// If this is true, the weapon will initialize itself on start, otherwise it'll have to be init manually, usually by the CharacterHandleWeapon class
		public bool InitializeOnStart = false;
        /// whether or not this weapon can be interrupted 
        public bool Interruptable = false;

        /// the name of the inventory item corresponding to this weapon. Automatically set (if needed) by InventoryEngineWeapon
        public string WeaponID { get; set; }
        /// the weapon's owner
        public Character Owner { get; protected set; }
        /// the weapon's owner's CharacterHandleWeapon component
        public CharacterHandleWeapon CharacterHandleWeapon { get; set; }
        /// if true, the weapon is flipped
        [ReadOnly]
        public bool Flipped;
        /// the WeaponAmmo component optionnally associated to this weapon
        public WeaponAmmo WeaponAmmo { get; protected set; }
        /// the weapon's state machine
        public MMStateMachine<WeaponStates> WeaponState;

        protected SpriteRenderer _spriteRenderer;
        protected WeaponAim _aimableWeapon;
        protected float _movementMultiplierStorage = 1f;
        protected Animator _ownerAnimator;
        protected float _delayBeforeUseCounter = 0f;
        protected float _delayBetweenUsesCounter = 0f;
        protected float _reloadingCounter = 0f;
        protected bool _triggerReleased = false;
        protected bool _reloading = false;
        protected ComboWeapon _comboWeapon;
        protected TopDownController _controller;
        protected CharacterMovement _characterMovement;
        protected Vector3 _weaponOffset;
        protected Vector3 _weaponAttachmentOffset;
        protected Transform _weaponAttachment;
        protected List<List<string>> _animatorParameters;
        protected List<string> _ownerAnimatorParameters;

        /// <summary>
        /// On start we initialize our weapon
        /// </summary>
        protected virtual void Start()
        {
            if (InitializeOnStart)
            {
                Initialization();
            }
        }

        /// <summary>
        /// Initialize this weapon.
        /// </summary>
        public virtual void Initialization()
        {
            Flipped = false;
            _spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
            _comboWeapon = this.gameObject.GetComponent<ComboWeapon>();

            WeaponState = new MMStateMachine<WeaponStates>(gameObject, true);
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
            WeaponAmmo = GetComponent<WeaponAmmo>();
            _animatorParameters = new List<List<string>>();
            _aimableWeapon = GetComponent<WeaponAim>();
            InitializeAnimatorParameters();
            if (WeaponAmmo == null)
            {
                CurrentAmmoLoaded = MagazineSize;
            }
            InitializeAnimatorParameters();
            InitializeFeedbacks();       
        }

        protected virtual void InitializeFeedbacks()
        {
            WeaponStartMMFeedback?.Initialization(this.gameObject);
            WeaponUsedMMFeedback?.Initialization(this.gameObject);
            WeaponStopMMFeedback?.Initialization(this.gameObject);
            WeaponReloadNeededMMFeedback?.Initialization(this.gameObject);
            WeaponReloadMMFeedback?.Initialization(this.gameObject);
        }

        /// <summary>
        /// Initializes the combo weapon, if it's one
        /// </summary>
        public virtual void InitializeComboWeapons()
        {
            if (_comboWeapon != null)
            {
                _comboWeapon.Initialization();
            }
        }

        /// <summary>
        /// Sets the weapon's owner
        /// </summary>
        /// <param name="newOwner">New owner.</param>
        public virtual void SetOwner(Character newOwner, CharacterHandleWeapon handleWeapon)
        {
            Owner = newOwner;
            if (Owner != null)
            {
                CharacterHandleWeapon = handleWeapon;
                _characterMovement = Owner.GetComponent<CharacterMovement>();
                _controller = Owner.GetComponent<TopDownController>();

                if (CharacterHandleWeapon.AutomaticallyBindAnimator)
                {
                    if (CharacterHandleWeapon.CharacterAnimator != null)
                    {
                        _ownerAnimator = CharacterHandleWeapon.CharacterAnimator;
                    }
                }
            }
        }

        /// <summary>
        /// Called by input, turns the weapon on
        /// </summary>
        public virtual void WeaponInputStart()
        {
            if (_reloading)
            {
                return;
            }
            if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
            {
                _triggerReleased = false;
                TurnWeaponOn();
            }
        }

        /// <summary>
        /// Describes what happens when the weapon starts
        /// </summary>
        protected virtual void TurnWeaponOn()
        {
            TriggerWeaponStartFeedback();
            WeaponState.ChangeState(WeaponStates.WeaponStart);
            if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
            {
                _movementMultiplierStorage = _characterMovement.MovementSpeedMultiplier;
                _characterMovement.MovementSpeedMultiplier = MovementMultiplier;
            }
            if (_comboWeapon != null)
            {
                _comboWeapon.WeaponStarted(this);
            }
            if (PreventAllMovementWhileInUse && (_characterMovement != null) && (_controller != null))
            {
                _characterMovement.SetMovement(Vector2.zero);
                _characterMovement.MovementForbidden = true;
            }
        }

        /// <summary>
        /// On Update, we check if the weapon is or should be used
        /// </summary>
        protected virtual void Update()
        {
            FlipWeapon();
            ApplyOffset();            
            UpdateAnimator();
        }

        /// <summary>
        /// On LateUpdate, processes the weapon state
        /// </summary>
        protected virtual void LateUpdate()
        {
            ProcessWeaponState();
        }

        /// <summary>
        /// Called every lastUpdate, processes the weapon's state machine
        /// </summary>
        protected virtual void ProcessWeaponState()
        {
            if (WeaponState == null) { return; }

            switch (WeaponState.CurrentState)
            {
                case WeaponStates.WeaponIdle:
                    CaseWeaponIdle();
                    break;

                case WeaponStates.WeaponStart:
                    CaseWeaponStart();
                    break;

                case WeaponStates.WeaponDelayBeforeUse:
                    CaseWeaponDelayBeforeUse();
                    break;

                case WeaponStates.WeaponUse:
                    CaseWeaponUse();
                    break;

                case WeaponStates.WeaponDelayBetweenUses:
                    CaseWeaponDelayBetweenUses();
                    break;

                case WeaponStates.WeaponStop:
                    CaseWeaponStop();
                    break;

                case WeaponStates.WeaponReloadNeeded:
                    CaseWeaponReloadNeeded();
                    break;

                case WeaponStates.WeaponReloadStart:
                    CaseWeaponReloadStart();
                    break;

                case WeaponStates.WeaponReload:
                    CaseWeaponReload();
                    break;

                case WeaponStates.WeaponReloadStop:
                    CaseWeaponReloadStop();
                    break;

                case WeaponStates.WeaponInterrupted:
                    CaseWeaponInterrupted();
                    break;
            }
        }

        /// <summary>
        /// If the weapon is idle, we reset the movement multiplier
        /// </summary>
        protected virtual void CaseWeaponIdle()
        {
            ResetMovementMultiplier();
        }

        /// <summary>
        /// When the weapon starts we switch to a delay or shoot based on our weapon's settings
        /// </summary>
        protected virtual void CaseWeaponStart()
        {
            if (DelayBeforeUse > 0)
            {
                _delayBeforeUseCounter = DelayBeforeUse;
                WeaponState.ChangeState(WeaponStates.WeaponDelayBeforeUse);
            }
            else
            {
                ShootRequest();
            }
        }

        /// <summary>
        /// If we're in delay before use, we wait until our delay is passed and then request a shoot
        /// </summary>
        protected virtual void CaseWeaponDelayBeforeUse()
        {
            _delayBeforeUseCounter -= Time.deltaTime;
            if (_delayBeforeUseCounter <= 0)
            {
                ShootRequest();
            }
        }

        /// <summary>
        /// On weapon use we use our weapon then switch to delay between uses
        /// </summary>
        protected virtual void CaseWeaponUse()
        {
            WeaponUse();
            _delayBetweenUsesCounter = TimeBetweenUses;
            WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
        }

        /// <summary>
        /// When in delay between uses, we either turn our weapon off or make a shoot request
        /// </summary>
        protected virtual void CaseWeaponDelayBetweenUses()
        {
            _delayBetweenUsesCounter -= Time.deltaTime;
            if (_delayBetweenUsesCounter <= 0)
            {
                if ((TriggerMode == TriggerModes.Auto) && !_triggerReleased)
                {
                    ShootRequest();
                }
                else
                {
                    TurnWeaponOff();
                }
            }
        }

        /// <summary>
        /// On weapon stop, we switch to idle
        /// </summary>
        protected virtual void CaseWeaponStop()
        {
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }

        /// <summary>
        /// If a reload is needed, we mention it and switch to idle
        /// </summary>
        protected virtual void CaseWeaponReloadNeeded()
        {
            ReloadNeeded();
            ResetMovementMultiplier();
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }

        /// <summary>
        /// on reload start, we reload the weapon and switch to reload
        /// </summary>
        protected virtual void CaseWeaponReloadStart()
        {
            ReloadWeapon();
            _reloadingCounter = ReloadTime;
            WeaponState.ChangeState(WeaponStates.WeaponReload);
        }

        /// <summary>
        /// on reload, we reset our movement multiplier, and switch to reload stop once our reload delay has passed
        /// </summary>
        protected virtual void CaseWeaponReload()
        {
            ResetMovementMultiplier();
            _reloadingCounter -= Time.deltaTime;
            if (_reloadingCounter <= 0)
            {
                WeaponState.ChangeState(WeaponStates.WeaponReloadStop);
            }
        }

        /// <summary>
        /// on reload stop, we swtich to idle and load our ammo
        /// </summary>
        protected virtual void CaseWeaponReloadStop()
        {
            _reloading = false;
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
            if (WeaponAmmo == null)
            {
                CurrentAmmoLoaded = MagazineSize;
            }
        }

        /// <summary>
        /// on weapon interrupted, we turn our weapon off and switch back to idle
        /// </summary>
        protected virtual void CaseWeaponInterrupted()
        {
            TurnWeaponOff();
            ResetMovementMultiplier();
            WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }

        /// <summary>
        /// Call this method to interrupt the weapon
        /// </summary>
        public virtual void Interrupt()
        {
            if (Interruptable)
            {
                WeaponState.ChangeState(WeaponStates.WeaponInterrupted);
            }
        }

        /// <summary>
        /// Determines whether or not the weapon can fire
        /// </summary>
        protected virtual void ShootRequest()
        {
            // if we have a weapon ammo component, we determine if we have enough ammunition to shoot
            if (_reloading)
            {
                return;
            }

            if (MagazineBased)
            {
                if (WeaponAmmo != null)
                {
                    if (WeaponAmmo.EnoughAmmoToFire())
                    {
                        WeaponState.ChangeState(WeaponStates.WeaponUse);
                    }
                    else
                    {
                        if (AutoReload && MagazineBased)
                        {
                            InitiateReloadWeapon();
                        }
                        else
                        {
                            WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
                        }
                    }
                }
                else
                {
                    if (CurrentAmmoLoaded > 0)
                    {
                        WeaponState.ChangeState(WeaponStates.WeaponUse);
                        CurrentAmmoLoaded -= AmmoConsumedPerShot;
                    }
                    else
                    {
                        if (AutoReload)
                        {
                            InitiateReloadWeapon();
                        }
                        else
                        {
                            WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
                        }
                    }
                }
            }
            else
            {
                if (WeaponAmmo != null)
                {
                    if (WeaponAmmo.EnoughAmmoToFire())
                    {
                        WeaponState.ChangeState(WeaponStates.WeaponUse);
                    }
                    else
                    {
                        WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
                    }
                }
                else
                {
                    WeaponState.ChangeState(WeaponStates.WeaponUse);
                }
            }
        }

        /// <summary>
        /// When the weapon is used, plays the corresponding sound
        /// </summary>
        protected virtual void WeaponUse()
        {
            // apply recoil
            if ((RecoilForce > 0f) && (_controller != null))
            {
                if (Owner != null)
                {
                    if (Owner.Orientation2D != null)
                    {
                        if (Flipped)
                        {
                            _controller.Impact(this.transform.right, RecoilForce);
                        }
                        else
                        {
                            _controller.Impact(-this.transform.right, RecoilForce);
                        }
                    }
                    else
                    {
                        _controller.Impact(-this.transform.forward, RecoilForce);
                    }
                }                
            }
            TriggerWeaponUsedFeedback();
        }

        /// <summary>
        /// Called by input, turns the weapon off if in auto mode
        /// </summary>
        public virtual void WeaponInputStop()
        {
            if (_reloading)
            {
                return;
            }
            _triggerReleased = true;
            if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
            {
                _characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
            }
        }

        /// <summary>
        /// Turns the weapon off.
        /// </summary>
        public virtual void TurnWeaponOff()
        {
            if ((WeaponState.CurrentState == WeaponStates.WeaponIdle || WeaponState.CurrentState == WeaponStates.WeaponStop))
            {
                return;
            }
            _triggerReleased = true;

            TriggerWeaponStopFeedback();
            WeaponState.ChangeState(WeaponStates.WeaponStop);
            ResetMovementMultiplier();
            if (_comboWeapon != null)
            {
                _comboWeapon.WeaponStopped(this);
            }
            if (PreventAllMovementWhileInUse && (_characterMovement != null))
            {
                _characterMovement.MovementForbidden = false;
            }
        }

        protected virtual void ResetMovementMultiplier()
        {
            if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
            {
                _characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
            }
        }

        /// <summary>
        /// Describes what happens when the weapon needs a reload
        /// </summary>
        protected virtual void ReloadNeeded()
        {
            TriggerWeaponReloadNeededFeedback();
        }

        public virtual void InitiateReloadWeapon()
        {
            // if we're already reloading, we do nothing and exit
            if (_reloading)
            {
                return;
            }
            WeaponState.ChangeState(WeaponStates.WeaponReloadStart);
            _reloading = true;
        }

        /// <summary>
        /// Reloads the weapon
        /// </summary>
        /// <param name="ammo">Ammo.</param>
        protected virtual void ReloadWeapon()
        {
            if (MagazineBased)
            {
                TriggerWeaponReloadFeedback();
            }
        }

        /// <summary>
        /// Flips the weapon.
        /// </summary>
        public virtual void FlipWeapon()
        {
            if (Owner == null)
            {
                return;
            }

            if (Owner.Orientation2D == null)
            {
                return;
            }

            if (FlipWeaponOnCharacterFlip)
            {
                Flipped = !Owner.Orientation2D.IsFacingRight;
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.flipX = Flipped;
                }
                else
                {
                    transform.localScale = Flipped ? LeftFacingFlipValue : RightFacingFlipValue;
                }
            }

            if (_comboWeapon != null)
            {
                _comboWeapon.FlipUnusedWeapons();
            }
        }            
        
        /// <summary>
        /// Destroys the weapon
        /// </summary>
        /// <returns>The destruction.</returns>
        public virtual IEnumerator WeaponDestruction()
        {
            yield return new WaitForSeconds(AutoDestroyWhenEmptyDelay);
            // if we don't have ammo anymore, and need to destroy our weapon, we do it
            Destroy(this.gameObject);

            if (WeaponID != null)
            {
                // we remove it from the inventory
                List<int> weaponList = Owner.gameObject.GetComponentNoAlloc<CharacterInventory>().WeaponInventory.InventoryContains(WeaponID);
                if (weaponList.Count > 0)
                {
                    Owner.gameObject.GetComponentNoAlloc<CharacterInventory>().WeaponInventory.DestroyItem(weaponList[0]);
                }
            }
        }

        /// <summary>
        /// Applies the offset specified in the inspector
        /// </summary>
        public virtual void ApplyOffset()
        {

            if (!WeaponCurrentlyActive)
            {
                return;
            }
            
            _weaponAttachmentOffset = WeaponAttachmentOffset;

            if (Owner == null)
            {
                return;
            }

            if (Owner.Orientation2D != null)
            {
                if (Flipped)
                {
                    _weaponAttachmentOffset.x = -WeaponAttachmentOffset.x;
                }
                
                // we apply the offset
                if (transform.parent != null)
                {
                    _weaponOffset = transform.parent.position + _weaponAttachmentOffset;
                    transform.position = _weaponOffset;
                }
            }
            else
            {
                if (transform.parent != null)
                {
                    _weaponOffset = _weaponAttachmentOffset;
                    transform.localPosition = _weaponOffset;
                }
            }           
        }

        /// <summary>
        /// Plays the weapon's start sound
        /// </summary>
        protected virtual void TriggerWeaponStartFeedback()
        {
            WeaponStartMMFeedback?.PlayFeedbacks(this.transform.position);
        }

        /// <summary>
        /// Plays the weapon's used sound
        /// </summary>
        protected virtual void TriggerWeaponUsedFeedback()
        {
            WeaponUsedMMFeedback?.PlayFeedbacks(this.transform.position);
        }

        /// <summary>
        /// Plays the weapon's stop sound
        /// </summary>
        protected virtual void TriggerWeaponStopFeedback()
        {            
            WeaponStopMMFeedback?.PlayFeedbacks(this.transform.position);
        }

        /// <summary>
        /// Plays the weapon's reload needed sound
        /// </summary>
        protected virtual void TriggerWeaponReloadNeededFeedback()
        {
            WeaponReloadNeededMMFeedback?.PlayFeedbacks(this.transform.position);
        }

        /// <summary>
        /// Plays the weapon's reload sound
        /// </summary>
        protected virtual void TriggerWeaponReloadFeedback()
        {
            WeaponReloadMMFeedback?.PlayFeedbacks(this.transform.position);
        }

        /// <summary>
        /// Adds required animator parameters to the animator parameters list if they exist
        /// </summary>
        public virtual void InitializeAnimatorParameters()
        {
            if (Animators.Count > 0)
            {
                for (int i = 0; i < Animators.Count; i++)
                {
                    _animatorParameters.Add(new List<string>());
                }

                for (int i = 0; i < Animators.Count; i++)
                {
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], WeaponAngleAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], WeaponAngleRelativeAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], IdleAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], StartAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], DelayBeforeUseAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], DelayBetweenUsesAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], StopAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], ReloadStartAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], ReloadStopAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], ReloadAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], SingleUseAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters[i]);
                    MMAnimator.AddAnimatorParamaterIfExists(Animators[i], UseAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters[i]);
                }
            }            

            if (_ownerAnimator != null)
            {
                _ownerAnimatorParameters = new List<string>();
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, WeaponAngleAnimationParameter, AnimatorControllerParameterType.Float, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, WeaponAngleRelativeAnimationParameter, AnimatorControllerParameterType.Float, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, IdleAnimationParameter, AnimatorControllerParameterType.Bool, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, StartAnimationParameter, AnimatorControllerParameterType.Bool, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, DelayBeforeUseAnimationParameter, AnimatorControllerParameterType.Bool, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, DelayBetweenUsesAnimationParameter, AnimatorControllerParameterType.Bool, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, StopAnimationParameter, AnimatorControllerParameterType.Bool, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, ReloadStartAnimationParameter, AnimatorControllerParameterType.Bool, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, ReloadStopAnimationParameter, AnimatorControllerParameterType.Bool, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, ReloadAnimationParameter, AnimatorControllerParameterType.Bool, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, SingleUseAnimationParameter, AnimatorControllerParameterType.Bool, _ownerAnimatorParameters);
                MMAnimator.AddAnimatorParamaterIfExists(_ownerAnimator, UseAnimationParameter, AnimatorControllerParameterType.Bool, _ownerAnimatorParameters);
            }
        }

        /// <summary>
        /// Override this to send parameters to the character's animator. This is called once per cycle, by the Character
        /// class, after Early, normal and Late process().
        /// </summary>
        public virtual void UpdateAnimator()
        {
            if (_animatorParameters == null)
            {
                return;
            }
            for (int i = 0; i < Animators.Count; i++)
            {
                MMAnimator.UpdateAnimatorBool(Animators[i], IdleAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle), _animatorParameters[i]);
                MMAnimator.UpdateAnimatorBool(Animators[i], StartAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStart), _animatorParameters[i]);
                MMAnimator.UpdateAnimatorBool(Animators[i], DelayBeforeUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse), _animatorParameters[i]);
                MMAnimator.UpdateAnimatorBool(Animators[i], UseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), _animatorParameters[i]);
                MMAnimator.UpdateAnimatorBool(Animators[i], SingleUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse), _animatorParameters[i]);
                MMAnimator.UpdateAnimatorBool(Animators[i], DelayBetweenUsesAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), _animatorParameters[i]);
                MMAnimator.UpdateAnimatorBool(Animators[i], StopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStop), _animatorParameters[i]);
                MMAnimator.UpdateAnimatorBool(Animators[i], ReloadStartAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart), _animatorParameters[i]);
                MMAnimator.UpdateAnimatorBool(Animators[i], ReloadAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload), _animatorParameters[i]);
                MMAnimator.UpdateAnimatorBool(Animators[i], ReloadStopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop), _animatorParameters[i]);

                if (_aimableWeapon != null)
                {
                    MMAnimator.UpdateAnimatorFloat(Animators[i], WeaponAngleAnimationParameter, _aimableWeapon.CurrentAngle, _animatorParameters[i]);
                    MMAnimator.UpdateAnimatorFloat(Animators[i], WeaponAngleRelativeAnimationParameter, _aimableWeapon.CurrentAngleRelative, _animatorParameters[i]);
                }
                else
                {
                    MMAnimator.UpdateAnimatorFloat(Animators[i], WeaponAngleAnimationParameter, 0f, _animatorParameters[i]);
                    MMAnimator.UpdateAnimatorFloat(Animators[i], WeaponAngleRelativeAnimationParameter, 0f, _animatorParameters[i]);
                }
            }

            if ((_ownerAnimator != null) && (WeaponState != null) && (_ownerAnimatorParameters != null))
            {
                MMAnimator.UpdateAnimatorBool(_ownerAnimator, IdleAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle), _ownerAnimatorParameters);
                MMAnimator.UpdateAnimatorBool(_ownerAnimator, StartAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStart), _ownerAnimatorParameters);
                MMAnimator.UpdateAnimatorBool(_ownerAnimator, DelayBeforeUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse), _ownerAnimatorParameters);
                MMAnimator.UpdateAnimatorBool(_ownerAnimator, UseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), _ownerAnimatorParameters);
                MMAnimator.UpdateAnimatorBool(_ownerAnimator, SingleUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse), _ownerAnimatorParameters);
                MMAnimator.UpdateAnimatorBool(_ownerAnimator, DelayBetweenUsesAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), _ownerAnimatorParameters);
                MMAnimator.UpdateAnimatorBool(_ownerAnimator, StopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStop), _ownerAnimatorParameters);
                MMAnimator.UpdateAnimatorBool(_ownerAnimator, ReloadStartAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart), _ownerAnimatorParameters);
                MMAnimator.UpdateAnimatorBool(_ownerAnimator, ReloadAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload), _ownerAnimatorParameters);
                MMAnimator.UpdateAnimatorBool(_ownerAnimator, ReloadStopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop), _ownerAnimatorParameters);

                if (_aimableWeapon != null)
                {
                    MMAnimator.UpdateAnimatorFloat(_ownerAnimator, WeaponAngleAnimationParameter, _aimableWeapon.CurrentAngle, _ownerAnimatorParameters);
                    MMAnimator.UpdateAnimatorFloat(_ownerAnimator, WeaponAngleRelativeAnimationParameter, _aimableWeapon.CurrentAngleRelative, _ownerAnimatorParameters);
                }
                else
                {
                    MMAnimator.UpdateAnimatorFloat(_ownerAnimator, WeaponAngleAnimationParameter, 0f, _ownerAnimatorParameters);
                    MMAnimator.UpdateAnimatorFloat(_ownerAnimator, WeaponAngleRelativeAnimationParameter, 0f, _ownerAnimatorParameters);
                }
            }
        }
    }
}