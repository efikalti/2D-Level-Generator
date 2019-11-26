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
    /// Add this class to a Camera with a bloom post processing and it'll be able to "shake" its values by getting events
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MMBloomShaker : MonoBehaviour, MMEventListener<MMBloomShakeEvent>
    {
        public float ShakeDuration = 0.2f;
        public bool RelativeIntensity = false;
        public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeIntensityAmplitude = 1.0f;
        public AnimationCurve ShakeThreshold = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeThresholdAmplitude = 1.0f;

        [ReadOnly]
        public bool Shaking = false;

        [InspectorButton("StartShaking")]
        public bool TestShakeButton;

        #if POSTPROCESSING_INSTALLED
            protected Bloom _bloom;
            protected PostProcessVolume _volume;
        #endif
        protected float _shakeStartedTimestamp;
        protected float _remappedTimeSinceStart;

        protected float _initialIntensity;
        protected float _initialThreshold;

        protected virtual void Awake()
        {
            #if POSTPROCESSING_INSTALLED
                _volume = this.gameObject.GetComponent<PostProcessVolume>();
                _volume.profile.TryGetSettings(out _bloom);
                _initialIntensity = _bloom.intensity;
                _initialThreshold = _bloom.threshold;
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
                    _bloom.intensity.Override(_initialIntensity);
                    _bloom.threshold.Override(_initialThreshold);
                #endif
            }
        }

        protected virtual void Shake()
        {
            _remappedTimeSinceStart = MMMaths.Remap(Time.time - _shakeStartedTimestamp, 0f, ShakeDuration, 0f, 1f);
            #if POSTPROCESSING_INSTALLED
                _bloom.intensity.Override(ShakeIntensity.Evaluate(_remappedTimeSinceStart) * ShakeIntensityAmplitude);
                _bloom.threshold.Override(ShakeThreshold.Evaluate(_remappedTimeSinceStart) * ShakeThresholdAmplitude);
                if (RelativeIntensity) { _bloom.intensity.Override(_bloom.intensity + _initialIntensity); }
                if (RelativeIntensity) { _bloom.threshold.Override(_bloom.threshold + _initialThreshold); }
            #endif
        }


        public virtual void OnMMEvent(MMBloomShakeEvent shakeEvent)
        {            
            ShakeDuration = shakeEvent.Duration;
            ShakeIntensity = shakeEvent.Intensity;
            ShakeIntensityAmplitude = shakeEvent.IntensityAmplitude * shakeEvent.Attenuation;
            ShakeThreshold = shakeEvent.Threshold;
            ShakeThresholdAmplitude = shakeEvent.ThresholdAmplitude * shakeEvent.Attenuation;
            RelativeIntensity = shakeEvent.RelativeIntensity;
            this.StartShaking();
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMBloomShakeEvent>();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMBloomShakeEvent>();
        }
    }

    public struct MMBloomShakeEvent
    {
        public float Duration;
        public AnimationCurve Intensity;
        public float IntensityAmplitude;
        public AnimationCurve Threshold;
        public float ThresholdAmplitude;
        public float Attenuation;
        public bool RelativeIntensity;

        public MMBloomShakeEvent(float duration, AnimationCurve intensity, float intensityAmplitude, AnimationCurve threshold, float thresholdAmplitude, bool relativeIntensity = false, float attenuation = 1.0f)
        {
            Intensity = intensity;
            Threshold = threshold;
            Duration = duration;
            IntensityAmplitude = intensityAmplitude;
            ThresholdAmplitude = thresholdAmplitude;
            RelativeIntensity = relativeIntensity;
            Attenuation = attenuation;
        }

        static MMBloomShakeEvent e;
        public static void Trigger(float duration, AnimationCurve intensity, float intensityAmplitude, AnimationCurve threshold, float thresholdAmplitude, bool relativeIntensity = false, float attenuation = 1.0f)
        {
            e.Intensity = intensity;
            e.Threshold = threshold;
            e.Duration = duration;
            e.IntensityAmplitude = intensityAmplitude;
            e.ThresholdAmplitude = thresholdAmplitude;
            e.RelativeIntensity = relativeIntensity;
            e.Attenuation = attenuation;
            MMEventManager.TriggerEvent(e);
        }
    }
}
