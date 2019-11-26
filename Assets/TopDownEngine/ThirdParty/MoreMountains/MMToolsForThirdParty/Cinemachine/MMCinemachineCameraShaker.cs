using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CINEMACHINE_INSTALLED
    using Cinemachine;
#endif
#if POSTPROCESSING_INSTALLED
using UnityEngine.Rendering.PostProcessing;
#endif
using MoreMountains.Tools;

namespace MoreMountains.ToolsForThirdParty
{
    /// <summary>
    /// Add this component to your Cinemachine Virtual Camera to have it shake when calling its ShakeCamera methods.
    /// </summary>
    public class MMCinemachineCameraShaker : MonoBehaviour, MMEventListener<MMCameraShakeEvent>
    {
        [Header("Settings")]
        /// The default amplitude that will be applied to your shakes if you don't specify one
        public float DefaultShakeAmplitude = .5f;
        /// The default frequency that will be applied to your shakes if you don't specify one
        public float DefaultShakeFrequency = 10f;
        [ReadOnly]
        /// the amplitude of the camera's noise when it's idle
        public float IdleAmplitude;
        [ReadOnly]
        /// the frequency of the camera's noise when it's idle
        public float IdleFrequency = 1f;

        public float LerpSpeed = 5f;

        [Header("Test")]
        public float TestDuration = 0.3f;
        public float TestAmplitude = 2f;
        public float TestFrequency = 20f;

        [InspectorButton("TestShake")]
        public bool TestShakeButton;

        protected Vector3 _initialPosition;
        protected Quaternion _initialRotation;


#if CINEMACHINE_INSTALLED
        protected Cinemachine.CinemachineBasicMultiChannelPerlin _perlin;
        protected Cinemachine.CinemachineVirtualCamera _virtualCamera;
#endif

        protected float _targetAmplitude;
        protected float _targetFrequency;

        private Coroutine _shakeCoroutine;

        /// <summary>
        /// On awake we grab our components
        /// </summary>
        protected virtual void Awake()
        {
#if CINEMACHINE_INSTALLED
                _virtualCamera = GameObject.FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
                _perlin = _virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
#endif
        }

        /// <summary>
        /// On Start we reset our camera to apply our base amplitude and frequency
        /// </summary>
        protected virtual void Start()
        {
#if CINEMACHINE_INSTALLED
                IdleAmplitude = _perlin.m_AmplitudeGain;
                IdleFrequency = _perlin.m_FrequencyGain;
#endif

            _targetAmplitude = IdleAmplitude;
            _targetFrequency = IdleFrequency;
        }

        protected virtual void Update()
        {
#if CINEMACHINE_INSTALLED
                _perlin.m_AmplitudeGain = _targetAmplitude;
                _perlin.m_FrequencyGain = Mathf.Lerp(_perlin.m_FrequencyGain, _targetFrequency, Time.deltaTime * LerpSpeed);
#endif
        }

        /// <summary>
        /// Use this method to shake the camera for the specified duration (in seconds) with the default amplitude and frequency
        /// </summary>
        /// <param name="duration">Duration.</param>
        public virtual void ShakeCamera(float duration)
        {
            StartCoroutine(ShakeCameraCo(duration, DefaultShakeAmplitude, DefaultShakeFrequency));
        }

        /// <summary>
        /// Use this method to shake the camera for the specified duration (in seconds), amplitude and frequency
        /// </summary>
        /// <param name="duration">Duration.</param>
        /// <param name="amplitude">Amplitude.</param>
        /// <param name="frequency">Frequency.</param>
        public virtual void ShakeCamera(float duration, float amplitude, float frequency)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }
            _shakeCoroutine = StartCoroutine(ShakeCameraCo(duration, amplitude, frequency));
        }

        /// <summary>
        /// This coroutine will shake the 
        /// </summary>
        /// <returns>The camera co.</returns>
        /// <param name="duration">Duration.</param>
        /// <param name="amplitude">Amplitude.</param>
        /// <param name="frequency">Frequency.</param>
        protected virtual IEnumerator ShakeCameraCo(float duration, float amplitude, float frequency)
        {
            _targetAmplitude  = amplitude;
            _targetFrequency = frequency;
            yield return new WaitForSeconds(duration);
            CameraReset();
        }

        /// <summary>
        /// Resets the camera's noise values to their idle values
        /// </summary>
        public virtual void CameraReset()
        {
            _targetAmplitude = IdleAmplitude;
            _targetFrequency = IdleFrequency;
        }

        public virtual void OnMMEvent(MMCameraShakeEvent shakeEvent)
        {
            this.ShakeCamera(shakeEvent.Properties.Duration, shakeEvent.Properties.Amplitude, shakeEvent.Properties.Frequency);
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMCameraShakeEvent>();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMCameraShakeEvent>();
        }

        protected virtual void TestShake()
        {
            MMCameraShakeEvent.Trigger(TestDuration, TestAmplitude, TestFrequency);
        }
    }
}