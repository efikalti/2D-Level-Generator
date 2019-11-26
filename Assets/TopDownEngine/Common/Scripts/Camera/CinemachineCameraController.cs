using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
#if CINEMACHINE_INSTALLED
    using Cinemachine;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A class that handles camera follow for Cinemachine powered cameras
    /// </summary>
    public class CinemachineCameraController : MonoBehaviour
    {
        /// True if the camera should follow the player
        public bool FollowsPlayer { get; set; }

        #if CINEMACHINE_INSTALLED
            protected CinemachineVirtualCamera _virtualCamera;
            protected CinemachineConfiner _confiner;
        #endif

        /// <summary>
        /// On Awake we grab our components
        /// </summary>
        protected virtual void Awake()
        {
            #if CINEMACHINE_INSTALLED
                _virtualCamera = GetComponent<CinemachineVirtualCamera>();
                _confiner = GetComponent<CinemachineConfiner>();
            #endif
        }

        /// <summary>
        /// On Start we assign our bounding volume
        /// </summary>
        protected virtual void Start()
        {
            #if CINEMACHINE_INSTALLED
                _confiner.m_BoundingVolume = LevelManager.Instance.BoundsCollider;
            #endif
        }

        /// <summary>
        /// Starts following the LevelManager's main player
        /// </summary>
        public virtual void StartFollowing()
        {
            FollowsPlayer = true;
            #if CINEMACHINE_INSTALLED
                _virtualCamera.Follow = LevelManager.Instance.Players[0].CameraTarget.transform;
            #endif
        }

        /// <summary>
        /// Stops following any target
        /// </summary>
        public virtual void StopFollowing()
        {
            #if CINEMACHINE_INSTALLED
                _virtualCamera.Follow = null;
            #endif
        }
    }
}
