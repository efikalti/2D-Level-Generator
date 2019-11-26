using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if POSTPROCESSING_INSTALLED
    using UnityEngine.Rendering.PostProcessing;
#endif
using MoreMountains.Tools;

namespace MoreMountains.ToolsForThirdParty
{
    /// <summary>
    /// Add this class to a Camera with a lens distortion post processing and it'll be able to "shake" its values by getting events
    /// </summary>
    public class MMLensDistortionShaker : MonoBehaviour, MMEventListener<MMLensDistortionShakeEvent>
    {
        public bool RelativeIntensity = false;
        public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0),
                                                                    new Keyframe(0.2f, 1),
                                                                    new Keyframe(0.25f, -1),
                                                                    new Keyframe(0.35f, 0.7f),
                                                                    new Keyframe(0.4f, -0.7f),
                                                                    new Keyframe(0.6f, 0.3f),
                                                                    new Keyframe(0.65f, -0.3f),
                                                                    new Keyframe(0.8f, 0.1f),
                                                                    new Keyframe(0.85f, -0.1f),
                                                                    new Keyframe(1, 0));
        public float ShakeDuration = 0.8f;
        public float ShakeAmplitude = 50f;

        [ReadOnly]
        public bool Shaking = false;

        [InspectorButton("StartShaking")]
        public bool TestShakeButton;

        #if POSTPROCESSING_INSTALLED
            protected LensDistortion _lensDistortion;
            protected PostProcessVolume _volume;
        #endif
        protected float _shakeStartedTimestamp;
        protected float _remappedTimeSinceStart;
        protected float _initialIntensity;

        protected virtual void Awake()
        {
            #if POSTPROCESSING_INSTALLED
                _volume = this.gameObject.GetComponent<PostProcessVolume>();
                _volume.profile.TryGetSettings(out _lensDistortion);
                _initialIntensity = _lensDistortion.intensity;
            #endif
            Shaking = false;
        }

        public virtual void StartShaking()
        {
            if (Shaking)
            {
                return;
            }
            else
            {
                _shakeStartedTimestamp = Time.time;
                Shaking = true;
            }
        }

        protected virtual void Update()
        {
            if (Shaking)
            {
                Shake();
            }

            if (Shaking && (Time.time - _shakeStartedTimestamp > ShakeDuration))
            {
                Shaking = false;
                #if POSTPROCESSING_INSTALLED
                    _lensDistortion.intensity.Override(_initialIntensity);
                #endif
            }
        }

        protected virtual void Shake()
        {
            _remappedTimeSinceStart = MMMaths.Remap(Time.time - _shakeStartedTimestamp, 0f, ShakeDuration, 0f, 1f);
            #if POSTPROCESSING_INSTALLED
                _lensDistortion.intensity.Override(ShakeIntensity.Evaluate(_remappedTimeSinceStart) * ShakeAmplitude);
                if (RelativeIntensity) { _lensDistortion.intensity.Override(_lensDistortion.intensity + _initialIntensity); }
            #endif
        }


        public virtual void OnMMEvent(MMLensDistortionShakeEvent shakeEvent)
        {            
            ShakeDuration = shakeEvent.Duration;
            ShakeIntensity = shakeEvent.Intensity;
            ShakeAmplitude = shakeEvent.Amplitude * shakeEvent.Attenuation;
            RelativeIntensity = shakeEvent.RelativeIntensity;
            this.StartShaking();
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMLensDistortionShakeEvent>();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMLensDistortionShakeEvent>();
        }
    }

    public struct MMLensDistortionShakeEvent
    {
        public AnimationCurve Intensity;
        public float Duration;
        public float Amplitude;
        public float Attenuation;
        public bool RelativeIntensity;

        public MMLensDistortionShakeEvent(AnimationCurve intensity, float duration, float amplitude, bool relativeIntensity = false, float attenuation = 1.0f)
        {
            Intensity = intensity;
            Duration = duration;
            Amplitude = amplitude;
            RelativeIntensity = relativeIntensity;
            Attenuation = attenuation;
        }

        static MMLensDistortionShakeEvent e;
        public static void Trigger(AnimationCurve intensity, float duration, float amplitude, bool relativeIntensity = false, float attenuation = 1.0f)
        {
            e.Intensity = intensity;
            e.Duration = duration;
            e.Amplitude = amplitude;
            e.Attenuation = attenuation;
            e.RelativeIntensity = relativeIntensity;
            MMEventManager.TriggerEvent(e);
        }
    }
}
