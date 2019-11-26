using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.ToolsForThirdParty
{
    [AddComponentMenu("")]
    public class MMFeedbackVignette : MMFeedback
    {
        [Header("Vignette")]
        public AnimationCurve Intensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float Duration = 0.2f;
        public float Amplitude = 1.0f;
        public bool RelativeIntensity = false;

        public override void SetCustomName()
        {
            Label = "Vignette";
        }

        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
                MMVignetteShakeEvent.Trigger(Intensity, Duration, Amplitude, RelativeIntensity, attenuation);
            }
        }
    }
}
