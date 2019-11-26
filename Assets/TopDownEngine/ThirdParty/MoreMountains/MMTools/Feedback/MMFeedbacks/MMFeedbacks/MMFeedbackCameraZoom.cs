using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A feedback that will allow you to change the zoom of a (3D) camera when played
    /// </summary>
    [AddComponentMenu("")]
    public class MMFeedbackCameraZoom : MMFeedback
    {
        [Header("Camera Zoom")]
        /// the zoom mode (for : forward for TransitionDuration, static for Duration, backwards for TransitionDuration)
        public MMCameraZoomModes ZoomMode = MMCameraZoomModes.For;
        /// the target field of view
        public float ZoomFieldOfView = 30f;
        /// the zoom transition duration
        public float ZoomTransitionDuration = 0.05f;
        /// the duration for which the zoom is at max zoom
        public float ZoomDuration = 0.1f;

        /// <summary>
        /// Custom name setup
        /// </summary>
        public override void SetCustomName()
        {
            Label = "Camera Zoom";
        }

        /// <summary>
        /// On Play, triggers a zoom event
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
                MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, ZoomDuration);
            }
        }
    }
}
