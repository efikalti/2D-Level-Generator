using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// this feedback will let you animate the position of 
    /// </summary>
    [AddComponentMenu("")]
    public class MMFeedbackPosition : MMFeedback
    {
        [Header("Position")]
        /// the object this feedback will animate the position for
        public GameObject AnimatePositionTarget;
        /// the duration of the animation on play
        public float AnimatePositionDuration = 0.2f;
        /// the acceleration of the movement
        public AnimationCurve AnimatePositionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 0.05f), new Keyframe(0.9f, 0.95f), new Keyframe(1, 1));
        /// the initial position
        public Vector3 InitialPosition;
        /// the destination position
        public Vector3 DestinationPosition;
        /// the initial transform - if set, takes precedence over the Vector3 above
        public Transform InitialPositionTransform;
        /// the destination transform - if set, takes precedence over the Vector3 above
        public Transform DestinationPositionTransform;

        /// <summary>
        /// Custom name setup
        /// </summary>
        public override void SetCustomName()
        {
            Label = "Position";
        }

        /// <summary>
        /// On init, we set our initial and destination positions (transform will take precedence over vector3s)
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            if (Active)
            {
                if (AnimatePositionTarget == null)
                {
                    Debug.LogWarning("The animate position target for " + this + " is null, you have to define it in the inspector");
                    return;
                }

                if (InitialPositionTransform != null) 
                {
                    InitialPosition = InitialPositionTransform.position;
                }
                else
                {
                    InitialPosition = AnimatePositionTarget.transform.position + InitialPosition;
                }

                if (DestinationPositionTransform != null)
                {
                    DestinationPosition = DestinationPositionTransform.position;
                }
                else
                {
                    DestinationPosition = AnimatePositionTarget.transform.position + DestinationPosition;
                }
            }
        }

        /// <summary>
        /// On Play, we move our object from A to B
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active && (AnimatePositionTarget != null))
            {
                if (isActiveAndEnabled)
                {
                    StartCoroutine(MMMovement.MoveFromTo(AnimatePositionTarget, InitialPosition, DestinationPosition, AnimatePositionDuration, AnimatePositionCurve));
                }
            }
        }
    }
}
