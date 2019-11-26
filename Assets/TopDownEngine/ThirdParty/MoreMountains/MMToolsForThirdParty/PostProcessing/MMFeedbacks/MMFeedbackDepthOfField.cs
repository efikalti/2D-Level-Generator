using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.ToolsForThirdParty
{
    [AddComponentMenu("")]
    public class MMFeedbackDepthOfField : MMFeedback
    {
        [Header("Depth Of Field")]
        public float ShakeDuration = 0.2f;
        public bool RelativeIntensities = false;

        public AnimationCurve ShakeFocusDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeFocusDistanceAmplitude = 1.0f;

        public AnimationCurve ShakeAperture = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeApertureAmplitude = 1.0f;

        public AnimationCurve ShakeFocalLength = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeFocalLengthAmplitude = 1.0f;

        public override void SetCustomName()
        {
            Label = "Depth Of Field";
        }

        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
                MMDepthOfFieldShakeEvent.Trigger(ShakeDuration, ShakeFocusDistance, ShakeFocusDistanceAmplitude, ShakeAperture, ShakeApertureAmplitude, ShakeFocalLength, ShakeFocalLengthAmplitude, attenuation);
            }
        }
    }
}
