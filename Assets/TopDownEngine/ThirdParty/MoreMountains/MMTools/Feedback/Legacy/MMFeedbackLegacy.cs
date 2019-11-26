using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MoreMountains.ToolsForThirdParty;
using UnityEngine.Events;
using UnityEngine.Audio;
using UnityEngine.Assertions;

namespace MoreMountains.Tools
{
    /// <summary>
    /// This class is to be used from other classes, to act as a center point for various feedbacks. 
    /// It's meant to help setup and trigger feedbacks such as vfx, sounds, camera zoom or shake, etc, from an automated entry points in other classes inspectors.
    /// </summary>
	[Serializable]
	public class MMFeedbackLegacy
    {
        [Header("Animation")]
        public bool UpdateAnimator;
        [Condition("UpdateAnimator", true)]
        public Animator FeedbackAnimator;
        [Condition("UpdateAnimator", true)]
        public string AnimatorTriggerParameterName;
        [Condition("UpdateAnimator", true)]
        public string AnimatorBoolParameterName;

        [Header("Scale")]
        public bool AnimateScale = false;
        [Condition("AnimateScale", true)]
        public Transform AnimateScaleTarget;
        [Condition("AnimateScale", true)]
        public float AnimateScaleDuration = 0.2f;
        [Condition("AnimateScale", true)]
        public AnimationCurve AnimateScaleX = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.3f, 1.05f), new Keyframe(1, 1));
        [Condition("AnimateScale", true)]
        public AnimationCurve AnimateScaleY = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.3f, 1.05f), new Keyframe(1, 1));
        [Condition("AnimateScale", true)]
        public AnimationCurve AnimateScaleZ = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.3f, 1.05f), new Keyframe(1, 1));
        
        [Header("Particles")]
	    public bool InstantiateParticles = false;
	    [Condition("InstantiateParticles", true)]
	    public ParticleSystem ParticlesPrefab;
	    [Condition("InstantiateParticles", true)]
	    public Transform InstantiateParticlesAt;

        public bool BindParticleSystem = false;
        [Condition("BindParticleSystem", true)]
        /// a particle system already present in your object that will play when the feedback is played, and stopped when it's stopped
        public ParticleSystem BoundParticleSystem;

        [Header("Instantiated VFX")]
        /// whether or not a VFX (or other...) object should be instantiated once when the feedback is played
        public bool InstantiateVFX;
        [Condition("InstantiateVFX", true)]
        /// the vfx object to instantiate
        public GameObject VfxToInstantiate;
        [Condition("InstantiateVFX", true)]
        /// the position offset at which to instantiate the vfx object
        public Vector3 VfxPositionOffset;
        [Condition("InstantiateVFX", true)]
        /// whether or not we should create automatically an object pool for this vfx
        public bool VfxCreateObjectPool;
        [Condition("InstantiateVFX", true)]
        /// the initial and planned size of this object pool
        public int VfxObjectPoolSize = 5;

        [Header("Flicker")]
        public bool FlickerRenderer;
        /// whether or not the renderer should flicker when playing this feedback
        [Condition("FlickerRenderer", true)]
        public Renderer FlickeringRenderer;
        /// the duration of the flicker when getting damage
        [Condition("FlickerRenderer", true)]
        public float FlickerDuration = 0.2f;
        /// the frequency at which to flicker
        [Condition("FlickerRenderer", true)]
        public float FlickerOctave = 0.04f;
        /// the color we should flicker the sprite to 
        [Condition("FlickerRenderer", true)]
        public Color FlickerColor = new Color32(255, 20, 20, 255);

        [Header("Sounds")]
        /// a sound fx to play when this feedback is played
        public AudioClip Sfx;
	    public AudioMixerGroup SfxAudioMixerGroup;

        [Header("Camera Shake")]
        /// whether or not the camera should shake
        public bool ShakeCamera = false;
        [Condition("ShakeCamera",true)]
        /// the properties of the shake (duration, intensity, frequenc)
        public MMCameraShakeProperties CameraShakeProperties = new MMCameraShakeProperties(0.1f, 0.2f, 40f);

        [Header("Camera Zoom")]
        /// whether or not the camera should zoom when this feedback is played
        public bool ZoomCamera = false;
        [Condition("ZoomCamera", true)]
        /// the zoom mode (for : forward for TransitionDuration, static for Duration, backwards for TransitionDuration)
        public MMCameraZoomModes ZoomMode = MMCameraZoomModes.For;
        [Condition("ZoomCamera", true)]
        /// the target field of view
        public float ZoomFieldOfView = 30f;
        [Condition("ZoomCamera", true)]
        /// the zoom transition duration
        public float ZoomTransitionDuration = 0.05f;
        [Condition("ZoomCamera", true)]
        /// the duration for which the zoom is at max zoom
        public float ZoomDuration = 0.1f;

        [Header("Freeze Frame")]
        /// whether or not we should freeze the frame when that feedback is played
        public bool FreezeFrame = false;
        [Condition("FreezeFrame", true)]
        /// the duration of the freeze frame
        public float FreezeFrameDuration;

        [Header("Timescale Modification")]
        /// whether or not we should modify the timescale when this feedback is played
        public bool ModifyTimescale = false;
        [Condition("ModifyTimescale", true)]
        /// the new timescale to apply
        public float TimeScale;
        [Condition("ModifyTimescale", true)]
        /// the duration of the timescale modification
        public float TimeScaleDuration;
        [Condition("ModifyTimescale", true)]
        /// whether or not we should lerp the timescale
        public bool TimeScaleLerp;
        [Condition("ModifyTimescale", true)]
        /// the speed at which to lerp the timescale
        public float TimeScaleLerpSpeed;

        [Header("Chromatic Aberration")]
        /// whether or not we should have a chromatic aberration effect when this feedback is played
        public bool ChromaticAberration = false;
        [Condition("ChromaticAberration", true)]
        public AnimationCurve ChromaticAberrationIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Condition("ChromaticAberration", true)]
        public float ChromaticAberrationDuration = 0.2f;
        [Condition("ChromaticAberration", true)]
        public float ChromaticAberrationAmplitude = 1.0f;

        [Header("Flash")]
        /// whether or not we should trigger a flash when this feedback is played
        public bool TriggerFlash;
        [Condition("TriggerFlash", true)]
        /// the color of the flash
        public Color FlashColor = Color.white;
        [Condition("TriggerFlash", true)]
        /// the flash duration (in seconds)
        public float FlashDuration = 0.2f;
        [Condition("TriggerFlash", true)]
        /// the alpha of the flash
        public float FlashAlpha = 1f;
        [Condition("TriggerFlash", true)]
        /// the ID of the flash (usually 0). You can specify on each MMFlash object an ID, allowing you to have different flash images in one scene and call them separately (one for damage, one for health pickups, etc)
        public int FlashID = 0;
        
        [Header("Events")]
        public UnityEvent PlayEvent;
        public UnityEvent StopEvent;

        protected MMSimpleObjectPooler _objectPool;
        protected GameObject _newGameObject;
        protected Color _initialFlickerColor;

        /// <summary>
        /// This method needs to be called by the parent class to initialize the various feedbacks
        /// </summary>
        public virtual void Initialization(GameObject gameObject = null)
        {
            if (InstantiateVFX && VfxCreateObjectPool)
            {
                GameObject objectPoolGo = new GameObject();
                objectPoolGo.name = "FeedbackObjectPool";
                _objectPool = objectPoolGo.AddComponent<MMSimpleObjectPooler>();
                _objectPool.GameObjectToPool = VfxToInstantiate;
                _objectPool.PoolSize = VfxObjectPoolSize;
                _objectPool.FillObjectPool();
            }
            if (FlickerRenderer && (FlickeringRenderer != null))
            {
                if(FlickeringRenderer.material.HasProperty("_Color"))
                {
                    _initialFlickerColor = FlickeringRenderer.material.color;
                }
            }
            if (FlickerRenderer && (FlickeringRenderer == null) && (gameObject != null))
            {
                if (gameObject.GetComponentNoAlloc<Renderer>() != null)
                {
                    FlickeringRenderer = gameObject.GetComponent<Renderer>();
                }
                if (FlickeringRenderer == null)
                {
                    FlickeringRenderer = gameObject.GetComponentInChildren<Renderer>();
                }
                if (FlickeringRenderer != null)
                {
                    if (FlickeringRenderer.material.HasProperty("_Color"))
                    {
                        _initialFlickerColor = FlickeringRenderer.material.color;
                    }
                }
            }
            if (InstantiateParticles)
            {
                if (BoundParticleSystem)
                {
                    GameObject.Destroy(BoundParticleSystem.gameObject);
                }                    

                Assert.IsNotNull(InstantiateParticlesAt, "When instantiating particles, a transform placement has to be assigned.");

                ParticleSystem particleSystem = GameObject.Instantiate(ParticlesPrefab) as ParticleSystem;
                if (InstantiateParticlesAt)
                {
                    particleSystem.transform.SetParent(InstantiateParticlesAt, false);
                }

                this.BoundParticleSystem = particleSystem;
            }

        }

        public virtual void ResetFlickerColor()
        {
            if (FlickerRenderer && (FlickeringRenderer != null))
            {
                if (FlickeringRenderer.material.HasProperty("_Color"))
                {
                    FlickeringRenderer.material.color = _initialFlickerColor;
                }
            }
        }

        /// <summary>
        /// Plays all the feedbacks that were enabled for this
        /// </summary>
        /// <param name="position"></param>
        public virtual void Play(Vector3 position, MonoBehaviour monobehaviour = null, float attenuation = 1.0f)
        {
            // Flickering
            if (FlickerRenderer && (FlickeringRenderer != null) && (monobehaviour != null))
            {
                monobehaviour.StartCoroutine(MMImage.Flicker(FlickeringRenderer, _initialFlickerColor, FlickerColor, FlickerOctave, FlickerDuration));
            }

            // Camera shake
            if (ShakeCamera)
            {
                MMCameraShakeEvent.Trigger(CameraShakeProperties.Duration, CameraShakeProperties.Amplitude * attenuation, CameraShakeProperties.Frequency);
            }

            // animator
            if (UpdateAnimator)
            {
                if (AnimatorTriggerParameterName != null)
                {
                    FeedbackAnimator.SetTrigger(AnimatorTriggerParameterName);
                }

                if (AnimatorBoolParameterName != null)
                {
                    FeedbackAnimator.SetBool(AnimatorBoolParameterName, true);
                }
            }

            // scale
            if (AnimateScale && (AnimateScaleTarget != null) && (monobehaviour != null))
            {
                if (monobehaviour.isActiveAndEnabled)
                {
                    monobehaviour.StartCoroutine(MMMovement.AnimateScale(AnimateScaleTarget, Vector3.zero, AnimateScaleDuration, AnimateScaleX, AnimateScaleY, AnimateScaleZ));
                }
            }

            // instantiated particles
            if (InstantiateVFX && VfxToInstantiate != null)
            {
                if (_objectPool != null)
                {
                    _newGameObject = _objectPool.GetPooledGameObject();
                    if (_newGameObject != null)
                    {
                        _newGameObject.transform.position = position + VfxPositionOffset;
                        _newGameObject.SetActive(true);
                    }
                }
                else
                {
                    _newGameObject = GameObject.Instantiate(VfxToInstantiate) as GameObject;
                    _newGameObject.transform.position = position + VfxPositionOffset;
                }
            }

            // Zoom
            if (ZoomCamera)
            {
                MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, ZoomDuration);
            }

            // Freeze Frame
            if (FreezeFrame)
            {
                MMFreezeFrameEvent.Trigger(FreezeFrameDuration);
            }
            
            // Time Scale
            if (ModifyTimescale)
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, TimeScaleDuration, TimeScaleLerp, TimeScaleLerpSpeed, false);
            }

            // Particles
            if (BindParticleSystem && (BoundParticleSystem != null))
            {
                BoundParticleSystem.Play();
            }

            // Chromatic Aberration
            if (ChromaticAberration)
            {
                MMChromaticAberrationShakeEvent.Trigger(ChromaticAberrationIntensity, ChromaticAberrationDuration, ChromaticAberrationAmplitude, false, attenuation);
            }
            
            // Sounds
            if (Sfx != null)
            {
                MMSfxEvent.Trigger(Sfx, SfxAudioMixerGroup);
            }
            
            // Flash
            if (TriggerFlash)
            {
                MMFlashEvent.Trigger(FlashColor, FlashDuration * attenuation, FlashAlpha, FlashID);
            }

            // Events
            if (PlayEvent != null)
            {
                PlayEvent.Invoke();
            }

        }

        /// <summary>
        /// Stops all the feedbacks that need stopping
        /// </summary>
        public virtual void Stop()
		{
            // Particles
            if (BindParticleSystem && (BoundParticleSystem != null))
            {
				BoundParticleSystem.Stop();
			}

            if (UpdateAnimator)
            {
                if (AnimatorBoolParameterName != null)
                {
                    FeedbackAnimator.SetBool(AnimatorBoolParameterName, false);
                }
            }

            // Events
            if (StopEvent != null)
            {
                 StopEvent.Invoke();
            }
        }
	}
}
