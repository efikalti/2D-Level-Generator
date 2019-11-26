using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// This feedback will trigger a flash event (to be caught by a MMFlash) when played
    /// </summary>
    [AddComponentMenu("")]
    public class MMFeedbackFlash : MMFeedback
    {
        [Header("Flash")]
        /// the color of the flash
        public Color FlashColor = Color.white;
        /// the flash duration (in seconds)
        public float FlashDuration = 0.2f;
        /// the alpha of the flash
        public float FlashAlpha = 1f;
        /// the ID of the flash (usually 0). You can specify on each MMFlash object an ID, allowing you to have different flash images in one scene and call them separately (one for damage, one for health pickups, etc)
        public int FlashID = 0;

        /// <summary>
        /// Custom name setup
        /// </summary>
        public override void SetCustomName()
        {
            Label = "Flash";
        }

        /// <summary>
        /// On Play we trigger a flash event
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
                MMFlashEvent.Trigger(FlashColor, FlashDuration * attenuation, FlashAlpha, FlashID);
            }
        }
    }
}
