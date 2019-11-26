using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{	
    /// <summary>
    /// Do not use this class directly, use TopDownController2D for 2D characters, or TopDownController3D for 3D characters
    /// Both of these classes inherit from this one
    /// </summary>
	public class TopDownController : MonoBehaviour 
	{
        [Header("Gravity")]
        /// the current gravity to apply to our character (negative goes down, positive goes up, higher value, higher acceleration)
		public float Gravity = -30f;
        /// whether or not the gravity is currently being applied to this character
        public bool GravityActive = true;

        [Header("Crouch dimensions")]
        /// by default, the length of the raycasts used to get back to normal size will be auto generated based on your character's normal/standing height, but here you can specify a different value
        public float CrouchedRaycastLengthMultiplier = 1f;

        [ReadOnly]
        /// the current speed of the character
		public Vector3 Speed;
		[ReadOnly]
        /// the current velocity
		public Vector3 Velocity;
		[ReadOnly]
        /// the velocity of the character last frame
		public Vector3 VelocityLastFrame;
		[ReadOnly]
        /// the current acceleration
		public Vector3 Acceleration;
		[ReadOnly]
        /// whether or not the character is grounded
		public bool Grounded;
		[ReadOnly]
        /// whether or not the character got grounded this frame
		public bool JustGotGrounded;
		[ReadOnly]
        /// the current movement of the character
		public Vector3 CurrentMovement;
		[ReadOnly]
        /// the direction the character is going in
		public Vector3 CurrentDirection;
		[ReadOnly]
        /// the current friction
		public float Friction;
		[ReadOnly]
        /// the current added force, to be added to the character's movement
		public Vector3 AddedForce;
        [ReadOnly]
        /// whether or not the character is in free movement mode or not
        public bool FreeMovement = true;

        /// the collider's center coordinates
        public virtual Vector3 ColliderCenter { get { return Vector3.zero; }  }
        /// the collider's bottom coordinates
		public virtual Vector3 ColliderBottom { get { return Vector3.zero; }  }
        /// the collider's top coordinates
		public virtual Vector3 ColliderTop { get { return Vector3.zero; }  }
        /// the object (if any) below our character
		public GameObject ObjectBelow { get; set; }
        /// the surface modifier object below our character (if any)
		public SurfaceModifier SurfaceModifierBelow { get; set; }
        public virtual Vector3 AppliedImpact { get { return _impact; } }

        protected Vector3 _positionLastFrame;
		protected Vector3 _speedComputation;
		protected bool _groundedLastFrame;
        protected Vector3 _impact;		
		protected const float _smallValue=0.0001f;

        /// <summary>
        /// On awake, we initialize our current direction
        /// </summary>
		protected virtual void Awake()
		{			
			CurrentDirection = transform.forward;
		}

        /// <summary>
        /// On update, we check if we're grounded, and determine the direction
        /// </summary>
		protected virtual void Update()
		{
			CheckIfGrounded ();
			HandleFriction ();
			DetermineDirection ();
		}

        /// <summary>
        /// Computes the speed
        /// </summary>
		protected virtual void ComputeSpeed ()
		{
			Speed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
            // we round the speed to 2 decimals
            Speed.x = Mathf.Round(Speed.x * 100f) / 100f;
            Speed.y = Mathf.Round(Speed.y * 100f) / 100f;
            Speed.z = Mathf.Round(Speed.z * 100f) / 100f;
            _positionLastFrame = this.transform.position;
		}

		protected virtual void DetermineDirection()
		{
			
		}

		protected virtual void FixedUpdate()
		{
		}

		protected virtual void LateUpdate()
        {
            ComputeSpeed();
        }

		protected virtual void CheckIfGrounded()
		{
            JustGotGrounded = (!_groundedLastFrame && Grounded);
            _groundedLastFrame = Grounded;
        }
        
        public virtual void Impact(Vector3 direction, float force)
        {

        }


        public virtual void AddForce(Vector3 movement)
		{

		}

		public virtual void SetMovement(Vector3 movement)
		{

		}

		public virtual void MovePosition(Vector3 newPosition)
		{
			
		}

		public virtual void ResizeCollider(float newSize)
		{

		}

		public virtual void ResetColliderSize()
		{

		}

		public virtual bool CanGoBackToOriginalSize()
		{
			return true;
		}

		public virtual void CollisionsOn()
		{

		}

		public virtual void CollisionsOff()
		{

		}

        public virtual void SetKinematic(bool state)
        {

        }

        protected virtual void HandleFriction()
        {

        }

        public virtual void Reset()
        {
            _impact = Vector3.zero;
            GravityActive = true;
            Speed = Vector3.zero;
            Velocity = Vector3.zero;
            VelocityLastFrame = Vector3.zero;
            Acceleration = Vector3.zero;
            Grounded = true;
            JustGotGrounded = false;
            CurrentMovement = Vector3.zero;
            CurrentDirection = Vector3.zero;
            AddedForce = Vector3.zero;
        }
    }
}
