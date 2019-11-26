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
    /// Add this class to a Camera with a depth of field post processing and it'll be able to "shake" its values by getting events
    /// </summary>
    public class MMDepthOfFieldShaker : MonoBehaviour, MMEventListener<MMDepthOfFieldShakeEvent>
    {

        public float ShakeDuration = 0.2f;
        public bool RelativeIntensities = false;

        public AnimationCurve ShakeFocusDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeFocusDistanceAmplitude = 1.0f;

        public AnimationCurve ShakeAperture = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeApertureAmplitude = 1.0f;

        public AnimationCurve ShakeFocalLength = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeFocalLengthAmplitude = 1.0f;
        
        [ReadOnly]
        public bool Shaking = false;

        [InspectorButton("StartShaking")]
        public bool TestShakeButton;

        #if POSTPROCESSING_INSTALLED
            protected DepthOfField _depthOfField;
            protected PostProcessVolume _volume;
        #endif
        protected float _shakeStartedTimestamp;
        protected float _remappedTimeSinceStart;

        protected float _initialFocusDistance;
        protected float _initialAperture;
        protected float _initialFocalLength;

        protected virtual void Awake()
        {
            #if POSTPROCESSING_INSTALLED
                _volume = this.gameObject.GetComponent<PostProcessVolume>();
                _volume.profile.TryGetSettings(out _depthOfField);
                _initialFocusDistance = _depthOfField.focusDistance;
                _initialAperture = _depthOfField.aperture;
                _initialFocalLength = _depthOfField.focalLength;
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
                    _depthOfField.focusDistance.Override(_initialFocusDistance);
                    _depthOfField.aperture.Override(_initialAperture);
                    _depthOfField.focalLength.Override(_initialFocalLength);
                #endif
            }
        }

        protected virtual void Shake()
        {
            _remappedTimeSinceStart = MMMaths.Remap(Time.time - _shakeStartedTimestamp, 0f, ShakeDuration, 0f, 1f);
            #if POSTPROCESSING_INSTALLED
                _depthOfField.focusDistance.Override(ShakeFocusDistance.Evaluate(_remappedTimeSinceStart) * ShakeFocusDistanceAmplitude);
                _depthOfField.aperture.Override(ShakeAperture.Evaluate(_remappedTimeSinceStart) * ShakeApertureAmplitude);
                _depthOfField.focalLength.Override(ShakeFocalLength.Evaluate(_remappedTimeSinceStart) * ShakeFocalLengthAmplitude);
                if (RelativeIntensities)
                {
                    _depthOfField.focusDistance.Override(_depthOfField.focusDistance + _initialFocusDistance);
                    _depthOfField.aperture.Override(_depthOfField.aperture + _initialAperture);
                    _depthOfField.focalLength.Override(_depthOfField.focalLength + _initialFocalLength);
                }
            #endif
        }


        public virtual void OnMMEvent(MMDepthOfFieldShakeEvent shakeEvent)
        {
            ShakeDuration = shakeEvent.Duration;
            ShakeFocusDistance = shakeEvent.FocusDistanceIntensity;
            ShakeFocusDistanceAmplitude = shakeEvent.FocusDistanceAmplitude;
            ShakeAperture = shakeEvent.ApertureIntensity;
            ShakeApertureAmplitude = shakeEvent.ApertureAmplitude;
            ShakeFocalLength = shakeEvent.FocalLengthIntensity;
            ShakeFocalLengthAmplitude = shakeEvent.FocalLengthAmplitude;
            this.StartShaking();
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMDepthOfFieldShakeEvent>();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMDepthOfFieldShakeEvent>();
        }
    }

    public struct MMDepthOfFieldShakeEvent
    {
        public float Duration;

        public AnimationCurve FocusDistanceIntensity;
        public float FocusDistanceAmplitude;

        public AnimationCurve ApertureIntensity;
        public float ApertureAmplitude;

        public AnimationCurve FocalLengthIntensity;
        public float FocalLengthAmplitude;
        
        public float Attenuation;

        public MMDepthOfFieldShakeEvent(float duration, AnimationCurve focusDistanceIntensity, float focusDistanceAmplitude,
                                                        AnimationCurve apertureIntensity, float apertureAmplitude, 
                                                        AnimationCurve focalLengthIntensity, float focalLengthAmplitude, float attenuation = 1.0f)
        {
            Duration = duration;
            FocusDistanceIntensity = focusDistanceIntensity;
            FocusDistanceAmplitude = focusDistanceAmplitude;
            ApertureIntensity = apertureIntensity;
            ApertureAmplitude = apertureAmplitude;
            FocalLengthIntensity = focalLengthIntensity;
            FocalLengthAmplitude = focalLengthAmplitude;
            Attenuation = attenuation;
        }

        static MMDepthOfFieldShakeEvent e;
        public static void Trigger(float duration, AnimationCurve focusDistanceIntensity, float focusDistanceAmplitude,
                                                        AnimationCurve apertureIntensity, float apertureAmplitude,
                                                        AnimationCurve focalLengthIntensity, float focalLengthAmplitude, float attenuation = 1.0f)
        {
            e.Duration = duration;
            e.FocusDistanceIntensity = focusDistanceIntensity;
            e.FocusDistanceAmplitude = focusDistanceAmplitude;
            e.ApertureIntensity = apertureIntensity;
            e.ApertureAmplitude = apertureAmplitude;
            e.FocalLengthIntensity = focalLengthIntensity;
            e.FocalLengthAmplitude = focalLengthAmplitude;
            e.Attenuation = attenuation;
            MMEventManager.TriggerEvent(e);
        }
    }
}
