using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// This class will pilot the TopDownController component of your character.
    /// This is where you'll implement all of your character's game rules, like jump, dash, shoot, stuff like that.
    /// Animator parameters : Grounded (bool), xSpeed (float), ySpeed (float), 
    /// CollidingLeft (bool), CollidingRight (bool), CollidingBelow (bool), CollidingAbove (bool), Idle (bool)
    /// </summary>
    [SelectionBase]
	[AddComponentMenu("TopDown Engine/Character/Core/Character")] 
	public class Character : MonoBehaviour
    {
        /// the possible initial facing direction for your character
        public enum FacingDirections { Left, Right, Forwards, Backwards }
        /// the possible directions you can force your character to look at after its spawn
        public enum SpawnFacingDirections { Default, Left, Right, Up, Down }

        public enum CharacterDimensions { Type2D, Type3D }
        [ReadOnly]
        public CharacterDimensions CharacterDimension;

        /// the possible character types : player controller or AI (controlled by the computer)
        public enum CharacterTypes { Player, AI }

		[Information("The Character script is the mandatory basis for all Character abilities. Your character can either be a Non Player Character, controlled by an AI, or a Player character, controlled by the player. In this case, you'll need to specify a PlayerID, which must match the one specified in your InputManager. Usually 'Player1', 'Player2', etc.",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]
		/// Is the character player-controlled or controlled by an AI ?
		public CharacterTypes CharacterType = CharacterTypes.AI;
		/// Only used if the character is player-controlled. The PlayerID must match an input manager's PlayerID. It's also used to match Unity's input settings. So you'll be safe if you keep to Player1, Player2, Player3 or Player4
		public string PlayerID = "";				
		/// the various states of the character
		public CharacterStates CharacterState { get; protected set; }
	
		/// the direction the character will face on spawn
		[Information("Here you can force a direction the character should face when spawning. If set to default, it'll match your model's initial facing direction.",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]
		public SpawnFacingDirections DirectionOnSpawn = SpawnFacingDirections.Default;

		[Header("Animator")]
		[Information("The engine will try and find an animator for this character. If it's on the same gameobject it should have found it. If it's nested somewhere, you'll need to bind it below. You can also decide to get rid of it altogether, in that case, just uncheck 'use mecanim'.",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]
		/// the character animator
		public Animator CharacterAnimator;
		/// Set this to false if you want to implement your own animation system
		public bool UseDefaultMecanim = true;

		[Header("Model")]
		[Information("Leave this unbound if this is a regular, sprite-based character, and if the SpriteRenderer and the Character are on the same GameObject. If not, you'll want to parent the actual model to the Character object, and bind it below. See the 3D demo characters for an example of that. The idea behind that is that the model may move, flip, but the collider will remain unchanged.",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]
        /// the 'model' (can be any gameobject) used to manipulate the character. Ideally it's separated (and nested) from the collider/TopDown controller/abilities, to avoid messing with collisions.
        public GameObject CharacterModel;
        
        [Header("Events")]
		[Information("Here you can define whether or not you want to have that character trigger events when changing state. See the MMTools' State Machine doc for more info.",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]
		/// If this is true, the Character's state machine will emit events when entering/exiting a state
		public bool SendStateChangeEvents = true;
		/// If this is true, a state machine processor component will be added and it'll emit events on updates (see state machine processor's doc for more details)
		public bool SendStateUpdateEvents = true;

		/// State Machines
		public MMStateMachine<CharacterStates.MovementStates> MovementState;
		public MMStateMachine<CharacterStates.CharacterConditions> ConditionState;

		/// associated camera and input manager
		public InputManager LinkedInputManager { get; protected set; }
        /// the animator associated to this character
	    public Animator _animator { get; protected set; }
        /// a list of animator parameters
		public List<string> _animatorParameters { get; set; }

        public CharacterOrientation2D Orientation2D { get; protected set; }
        public CharacterOrientation3D Orientation3D { get; protected set; }
        public GameObject CameraTarget { get; set; }

        public bool OptimizeForMobile = true;

        protected Color _initialColor;
		protected CharacterAbility[] _characterAbilities;
	    protected float _originalGravity;
		protected Health _health;
		protected bool _spawnDirectionForced = false;
		protected TopDownController _controller;
        protected AIBrain _aiBrain;


        /// <summary>
        /// Initializes this instance of the character
        /// </summary>
        protected virtual void Awake()
		{		
			Initialization();
		}

		/// <summary>
		/// Gets and stores input manager, camera and components
		/// </summary>
		protected virtual void Initialization()
		{            
            if (this.gameObject.GetComponentNoAlloc<TopDownController2D>() != null)
            {
                CharacterDimension = CharacterDimensions.Type2D;
            }
            if (this.gameObject.GetComponentNoAlloc<TopDownController3D>() != null)
            {
                CharacterDimension = CharacterDimensions.Type3D;
            }

            // we initialize our state machines
            MovementState = new MMStateMachine<CharacterStates.MovementStates>(gameObject,SendStateChangeEvents);
			ConditionState = new MMStateMachine<CharacterStates.CharacterConditions>(gameObject,SendStateChangeEvents);

			// we get the current input manager
			GetInputManager();
			// we get the main camera
			// we store our components for further use 
			CharacterState = new CharacterStates();
			_characterAbilities = GetComponents<CharacterAbility>();
			_controller = GetComponent<TopDownController> ();
			_health = GetComponent<Health> ();
            _aiBrain = GetComponent<AIBrain>();

            Orientation2D = GetComponent<CharacterOrientation2D>();
            Orientation3D = GetComponent<CharacterOrientation3D>();

			if (CharacterAnimator != null)
			{
				_animator = CharacterAnimator;
			}
	        else
	        {
				_animator = GetComponent<Animator>();
	        }

	        if (_animator != null)
	        {
				InitializeAnimatorParameters();
	        }

            // instantiate camera target
            if (CameraTarget == null)
            {
                CameraTarget = new GameObject();
            }            
            CameraTarget.transform.SetParent(this.transform);
            CameraTarget.transform.localPosition = Vector3.zero;
            CameraTarget.name = "CameraTarget";

            if (LinkedInputManager != null)
            {
                if (OptimizeForMobile && LinkedInputManager.IsMobile)
                {
                    if (this.gameObject.GetComponentNoAlloc<MMConeOfVision2D>() != null)
                    {
                        this.gameObject.GetComponentNoAlloc<MMConeOfVision2D>().enabled = false;
                    }
                }
            }            
        }

		/// <summary>
		/// Initializes the animator parameters.
		/// </summary>
		protected virtual void InitializeAnimatorParameters()
		{
			if (_animator == null) { return; }

			_animatorParameters = new List<string>();

			MMAnimator.AddAnimatorParamaterIfExists(_animator,"Grounded",AnimatorControllerParameterType.Bool,_animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator,"xSpeed",AnimatorControllerParameterType.Float,_animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator,"ySpeed",AnimatorControllerParameterType.Float,_animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator,"zSpeed",AnimatorControllerParameterType.Float,_animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator,"Idle",AnimatorControllerParameterType.Bool,_animatorParameters);
			MMAnimator.AddAnimatorParamaterIfExists(_animator,"Alive",AnimatorControllerParameterType.Bool,_animatorParameters);
		}

		/// <summary>
		/// Gets (if it exists) the InputManager matching the Character's Player ID
		/// </summary>
		protected virtual void GetInputManager()
		{
			// we get the corresponding input manager
			if (!string.IsNullOrEmpty (PlayerID))
			{
				LinkedInputManager = null;
				InputManager[] foundInputManagers = FindObjectsOfType(typeof(InputManager)) as InputManager[];
				foreach (InputManager foundInputManager in foundInputManagers) 
		        {
					if (foundInputManager.PlayerID == PlayerID)
					{
						LinkedInputManager = foundInputManager;
					}
		        }
			}
		}

		/// <summary>
		/// Sets the player ID
		/// </summary>
		/// <param name="newPlayerID">New player ID.</param>
		public virtual void SetPlayerID(string newPlayerID)
		{
			PlayerID = newPlayerID;
			GetInputManager();
		}
		
		/// <summary>
		/// This is called every frame.
		/// </summary>
		protected virtual void Update()
		{		
			EveryFrame();
				
		}

		/// <summary>
		/// We do this every frame. This is separate from Update for more flexibility.
		/// </summary>
		protected virtual void EveryFrame()
		{
			// we process our abilities
			EarlyProcessAbilities();
			ProcessAbilities();
			LateProcessAbilities();

			// we send our various states to the animator.		 
			UpdateAnimators ();
		}

		/// <summary>
		/// Calls all registered abilities' Early Process methods
		/// </summary>
		protected virtual void EarlyProcessAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.EarlyProcessAbility();
				}
			}
		}

		/// <summary>
		/// Calls all registered abilities' Process methods
		/// </summary>
		protected virtual void ProcessAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.ProcessAbility();
				}
			}
		}

		/// <summary>
		/// Calls all registered abilities' Late Process methods
		/// </summary>
		protected virtual void LateProcessAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.LateProcessAbility();
				}
			}
		}


        /// <summary>
        /// This is called at Update() and sets each of the animators parameters to their corresponding State values
        /// </summary>
        protected virtual void UpdateAnimators()
		{	
			if ((UseDefaultMecanim) && (_animator!= null))
			{ 
				MMAnimator.UpdateAnimatorBool(_animator,"Grounded",_controller.Grounded,_animatorParameters);
				MMAnimator.UpdateAnimatorBool(_animator,"Alive",(ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead),_animatorParameters);
				MMAnimator.UpdateAnimatorFloat(_animator,"xSpeed",_controller.CurrentMovement.x,_animatorParameters);
				MMAnimator.UpdateAnimatorFloat(_animator,"ySpeed",_controller.CurrentMovement.y,_animatorParameters);
				MMAnimator.UpdateAnimatorFloat(_animator,"zSpeed",_controller.CurrentMovement.z,_animatorParameters);
				MMAnimator.UpdateAnimatorBool(_animator,"Idle",(MovementState.CurrentState == CharacterStates.MovementStates.Idle),_animatorParameters);


				foreach (CharacterAbility ability in _characterAbilities)
				{
					if (ability.enabled && ability.AbilityInitialized)
					{	
						ability.UpdateAnimator();
					}
				}
	        }
	    }

        /// <summary>
		/// Makes the player respawn at the location passed in parameters
		/// </summary>
		/// <param name="spawnPoint">The location of the respawn.</param>
		public virtual void RespawnAt(Transform spawnPoint, FacingDirections facingDirection)
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                //Debug.LogError("Spawn : your Character's gameobject is inactive");
            }

            // we raise it from the dead (if it was dead)
            ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
            // we re-enable its 2D collider
            if (this.gameObject.GetComponentNoAlloc<Collider2D>() != null)
            {
                this.gameObject.GetComponentNoAlloc<Collider2D>().enabled = true;
            }
            // we re-enable its 2D collider
            if (this.gameObject.GetComponentNoAlloc<Collider>() != null)
            {
                this.gameObject.GetComponentNoAlloc<Collider>().enabled = true;
            }

            // we make it handle collisions again
            _controller.enabled = true;
            _controller.CollisionsOn();
            _controller.Reset();

            // we kill all potential velocity
            if (this.gameObject.GetComponentNoAlloc<Rigidbody>() != null)
            {
                this.gameObject.GetComponentNoAlloc<Rigidbody>().velocity = Vector3.zero;
            }
            if (this.gameObject.GetComponentNoAlloc<Rigidbody2D>() != null)
            {
                this.gameObject.GetComponentNoAlloc<Rigidbody2D>().velocity = Vector3.zero;
            }

            Reset();

            if (_health != null)
            {
                _health.ResetHealthToMaxHealth();
                _health.Revive();
            }

            if (_aiBrain != null)
            {
                _aiBrain.enabled = true;
            }

            transform.position = spawnPoint.position;
        }

        public virtual void FlipAllAbilities()
        {
            foreach (CharacterAbility ability in _characterAbilities)
            {
                if (ability.enabled)
                {
                    ability.Flip();
                }
            }
        }
		/// <summary>
		/// Called to disable the player (at the end of a level for example. 
		/// It won't move and respond to input after this.
		/// </summary>
		public virtual void Disable()
		{
			this.enabled = false;
			_controller.enabled = false;			
		}
        
		/// <summary>
		/// Called when the Character dies. 
		/// Calls every abilities' Reset() method, so you can restore settings to their original value if needed
		/// </summary>
		public virtual void Reset()
		{
			_spawnDirectionForced = false;
			if (_characterAbilities == null)
			{
				return;
			}
			if (_characterAbilities.Length == 0)
			{
				return;
			}
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled)
				{
					ability.Reset();
				}
			}
		}

		/// <summary>
		/// On revive, we force the spawn direction
		/// </summary>
		protected virtual void OnRevive()
		{
            if (_aiBrain != null)
            {
                _aiBrain.enabled = true;
            }
        }

        protected virtual void OnDeath()
        {
            if (_aiBrain != null)
            {
                _aiBrain.enabled = false;
            }
            if (MovementState.CurrentState != CharacterStates.MovementStates.FallingDownHole)
            {
                MovementState.ChangeState(CharacterStates.MovementStates.Idle);
            }            
        }

        protected virtual void OnHit()
        {

        }

		/// <summary>
		/// OnEnable, we register our OnRevive event
		/// </summary>
		protected virtual void OnEnable ()
		{
			if (_health != null)
			{
				_health.OnRevive += OnRevive;
                _health.OnDeath += OnDeath;
                _health.OnHit += OnHit;
            }
		}

		/// <summary>
		/// OnDisable, we unregister our OnRevive event
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnRevive -= OnRevive;
                _health.OnDeath -= OnDeath;
                _health.OnHit -= OnHit;
            }			
		}
	}
}