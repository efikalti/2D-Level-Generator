using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.ToolsForThirdParty
{
    [AddComponentMenu("")]
    public class MMFeedbackChromaticAberration : MMFeedback
    {
        [Header("Chromatic Aberration")]
        public AnimationCurve Intensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float Duration = 0.2f;
        public float Amplitude = 1.0f;
        public bool RelativeIntensity = false;

        public override void SetCustomName()
        {
            Label = "Chromatic Aberration";
        }

        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
                MMChromaticAberrationShakeEvent.Trigger(Intensity, Duration, Amplitude, RelativeIntensity, attenuation);
            }
        }
    }
}
