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
    /// Add this class to a Camera with a color grading post processing and it'll be able to "shake" its values by getting events
    /// </summary>
    public class MMColorGradingShaker : MonoBehaviour, MMEventListener<MMColorGradingShakeEvent>
    {
        public float ShakeDuration = 1f;
        public bool RelativeIntensity = true;

        public AnimationCurve ShakePostExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakePostExposureAmplitude = 0.2f;

        public AnimationCurve ShakeHueShift = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeHueShiftAmplitude = -50f;

        public AnimationCurve ShakeSaturation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeSaturationAmplitude = 200f;

        public AnimationCurve ShakeContrast = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public float ShakeContrastAmplitude = 100f;

        [ReadOnly]
        public bool Shaking = false;

        [InspectorButton("StartShaking")]
        public bool TestShakeButton;

        #if POSTPROCESSING_INSTALLED
            protected ColorGrading _colorGrading;
            protected PostProcessVolume _volume;
        #endif
        protected float _shakeStartedTimestamp;
        protected float _remappedTimeSinceStart;

        protected float _initialPostExposure;
        protected float _initialHueShift;
        protected float _initialSaturation;
        protected float _initialContrast;

        protected virtual void Awake()
        {
            #if POSTPROCESSING_INSTALLED
                _volume = this.gameObject.GetComponent<PostProcessVolume>();
                _volume.profile.TryGetSettings(out _colorGrading);
                _initialPostExposure = _colorGrading.postExposure;
                _initialHueShift = _colorGrading.hueShift;
                _initialSaturation = _colorGrading.saturation;
                _initialContrast = _colorGrading.contrast;
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
                    _colorGrading.postExposure.Override(_initialPostExposure);
                    _colorGrading.hueShift.Override(_initialHueShift);
                    _colorGrading.saturation.Override(_initialSaturation);
                    _colorGrading.contrast.Override(_initialContrast);
                #endif
            }
        }

        protected virtual void Shake()
        {
            _remappedTimeSinceStart = MMMaths.Remap(Time.time - _shakeStartedTimestamp, 0f, ShakeDuration, 0f, 1f);
            #if POSTPROCESSING_INSTALLED
                _colorGrading.postExposure.Override(ShakePostExposure.Evaluate(_remappedTimeSinceStart) * ShakePostExposureAmplitude);
                _colorGrading.hueShift.Override(ShakeHueShift.Evaluate(_remappedTimeSinceStart) * ShakeHueShiftAmplitude);
                _colorGrading.saturation.Override(ShakeSaturation.Evaluate(_remappedTimeSinceStart) * ShakeSaturationAmplitude);
                _colorGrading.contrast.Override(ShakeContrast.Evaluate(_remappedTimeSinceStart) * ShakeContrastAmplitude);

                if (RelativeIntensity) { _colorGrading.postExposure.Override(_colorGrading.postExposure + _initialPostExposure); }
                if (RelativeIntensity) { _colorGrading.hueShift.Override(_colorGrading.hueShift + _initialHueShift); }
                if (RelativeIntensity) { _colorGrading.saturation.Override(_colorGrading.saturation + _initialSaturation); }
                if (RelativeIntensity) { _colorGrading.contrast.Override(_colorGrading.contrast + _initialContrast); }
            #endif
        }


        public virtual void OnMMEvent(MMColorGradingShakeEvent shakeEvent)
        {            
            ShakeDuration = shakeEvent.Duration;
            RelativeIntensity = shakeEvent.RelativeIntensity;

            ShakePostExposure = shakeEvent.PostExposure;
            ShakePostExposureAmplitude = shakeEvent.PostExposureAmplitude * shakeEvent.Attenuation;

            ShakeHueShift = shakeEvent.HueShift;
            ShakeHueShiftAmplitude = shakeEvent.HueShiftAmplitude * shakeEvent.Attenuation;

            ShakeSaturation = shakeEvent.Saturation;
            ShakeSaturationAmplitude = shakeEvent.SaturationAmplitude * shakeEvent.Attenuation;

            ShakeContrast = shakeEvent.Contrast;
            ShakeContrastAmplitude = shakeEvent.ContrastAmplitude * shakeEvent.Attenuation;
            
            this.StartShaking();
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMColorGradingShakeEvent>();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMColorGradingShakeEvent>();
        }
    }

    public struct MMColorGradingShakeEvent
    {
        public float Duration;
        public float Attenuation;
        public bool RelativeIntensity;

        public AnimationCurve PostExposure;
        public float PostExposureAmplitude;

        public AnimationCurve HueShift;
        public float HueShiftAmplitude;

        public AnimationCurve Saturation;
        public float SaturationAmplitude;

        public AnimationCurve Contrast;
        public float ContrastAmplitude;

        public MMColorGradingShakeEvent(float duration, AnimationCurve postExposure, float postExposureAmplitude, AnimationCurve hueShift, float hueShiftAmplitude, 
            AnimationCurve saturation, float saturationAmplitude, AnimationCurve contrast, float contrastAmplitude, bool relativeIntensity = false, float attenuation = 1.0f)
        {            
            Duration = duration;
            RelativeIntensity = relativeIntensity;
            Attenuation = attenuation;
            PostExposure = postExposure;
            PostExposureAmplitude = postExposureAmplitude;
            HueShift = hueShift;
            HueShiftAmplitude = hueShiftAmplitude;
            Saturation = saturation;
            SaturationAmplitude = saturationAmplitude;
            Contrast = contrast;
            ContrastAmplitude = contrastAmplitude;
        }

        static MMColorGradingShakeEvent e;
        public static void Trigger(float duration, AnimationCurve postExposure, float postExposureAmplitude, AnimationCurve hueShift, float hueShiftAmplitude,
            AnimationCurve saturation, float saturationAmplitude, AnimationCurve contrast, float contrastAmplitude, bool relativeIntensity = false, float attenuation = 1.0f)
        {
            e.Duration = duration;
            e.RelativeIntensity = relativeIntensity;
            e.Attenuation = attenuation;
            e.PostExposure = postExposure;
            e.PostExposureAmplitude = postExposureAmplitude;
            e.HueShift = hueShift;
            e.HueShiftAmplitude = hueShiftAmplitude;
            e.Saturation = saturation;
            e.SaturationAmplitude = saturationAmplitude;
            e.Contrast = contrast;
            e.ContrastAmplitude = contrastAmplitude;
            MMEventManager.TriggerEvent(e);
        }
    }
}
