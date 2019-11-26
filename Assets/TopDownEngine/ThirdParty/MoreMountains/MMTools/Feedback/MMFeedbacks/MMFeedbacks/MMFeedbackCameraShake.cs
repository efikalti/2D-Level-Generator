using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// This feedback will send a shake event when played
    /// </summary>
    [AddComponentMenu("")]
    public class MMFeedbackCameraShake : MMFeedback
    {
        [Header("Camera Shake")]
        /// the properties of the shake (duration, intensity, frequenc)
        public MMCameraShakeProperties CameraShakeProperties = new MMCameraShakeProperties(0.1f, 0.2f, 40f);

        /// <summary>
        /// Custom name
        /// </summary>
        public override void SetCustomName()
        {
            Label = "Camera Shake";
        }

        /// <summary>
        /// On Play, sends a shake camera event
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
                MMCameraShakeEvent.Trigger(CameraShakeProperties.Duration, CameraShakeProperties.Amplitude * attenuation, CameraShakeProperties.Frequency);
            }
        }
    }
}
