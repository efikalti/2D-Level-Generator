using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// This feedback will trigger a freeze frame event when played, pausing the game for the specified duration (usually short, but not necessarily)
    /// </summary>
    [AddComponentMenu("")]
    public class MMFeedbackFreezeFrame : MMFeedback
    {
        [Header("Freeze Frame")]
        /// the duration of the freeze frame
        public float FreezeFrameDuration = 0.02f;

        /// <summary>
        /// Custom name setup
        /// </summary>
        public override void SetCustomName()
        {
            Label = "Freeze Frame";
        }

        /// <summary>
        /// On Play we trigger a freeze frame event
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
                MMFreezeFrameEvent.Trigger(FreezeFrameDuration);
            }
        }
    }
}
