using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{	
	[RequireComponent(typeof(Weapon))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Laser Sight")]
	/// <summary>
	/// Add this class to a weapon and it'll project a laser ray towards the direction the weapon is facing
	/// </summary>
	public class WeaponLaserSight : MonoBehaviour 
	{
		/// the origin of the raycast used to detect obstacles
		public Vector3 RaycastOriginOffset;
		/// the origin of the visible laser
		public Vector3 LaserOriginOffset;
		/// the maximum distance to which we should draw the laser
		public float LaserMaxDistance = 50;
		/// the collision mask containing all layers that should stop the laser
		public LayerMask LaserCollisionMask;
		/// the width of the laser
		public Vector2 LaserWidth = new Vector2(0.05f, 0.05f);
		/// the material used to render the laser
		public Material LaserMaterial;

		protected Weapon _weapon;
		protected LineRenderer _line;
		protected RaycastHit _hit;
		protected Vector3 _origin;
		protected Vector3 _destination;
		protected Vector3 _laserOffset;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Initialization()
		{
			_weapon = GetComponent<Weapon>();
			if (_weapon == null)
			{
				Debug.LogWarning("This WeaponLaserSight is not associated to a weapon. Please add it to a gameobject with a Weapon component.");
			}

			_line = gameObject.AddComponent<LineRenderer>();
			_line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			_line.receiveShadows = true;
			_line.startWidth = LaserWidth.x;
			_line.endWidth = LaserWidth.y;
			_line.material = LaserMaterial;
		}

		/// <summary>
		/// Every frame we draw our laser
		/// </summary>
		protected virtual void Update()
		{
			ShootLaser();
		}

		/// <summary>
		/// Draws the actual laser
		/// </summary>
		protected virtual void ShootLaser()
		{
			
			_laserOffset = LaserOriginOffset;
		
			// our laser will be shot from the weapon's laser origin
			_origin = MMMaths.RotatePointAroundPivot (_weapon.transform.position + _laserOffset, _weapon.transform.position, _weapon.transform.rotation);

			// we cast a ray in front of the weapon to detect an obstacle
			_hit = MMDebug.Raycast3D(_origin, _weapon.transform.forward, LaserMaxDistance, LaserCollisionMask, Color.red, true);

			// if we've hit something, our destination is the raycast hit
			if (_hit.transform != null)
			{
				_destination = _hit.point;
			}
			// otherwise we just draw our laser in front of our weapon 
			else
			{
				_destination = _origin +  _weapon.transform.forward * LaserMaxDistance;
				//_destination = MMMaths.RotatePointAroundPivot (_destination, _weapon.transform.position, _weapon.transform.rotation);
			}

			// we set our laser's line's start and end coordinates
			_line.SetPosition(0, _origin);
			_line.SetPosition(1, _destination);
		}

		/// <summary>
		/// Turns the laser on or off depending on the status passed in parameters
		/// </summary>
		/// <param name="status">If set to <c>true</c> status.</param>
		public virtual void LaserActive(bool status)
		{
			_line.enabled = status;
		}

	}
}