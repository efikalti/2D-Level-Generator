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
    /// This class will set the depth of field to focus on the set of targets specified in its inspector.
    /// </summary>
    public class MMAutoFocus : MonoBehaviour
    {
        // Array of targets
        public Transform[] FocusTargets;

        // Current target
        public float FocusTargetID;

        // Cache profile
        #if POSTPROCESSING_INSTALLED
            PostProcessVolume _volume;
            PostProcessProfile _profile;
            DepthOfField _depthOfField;
        #endif

        [Range(0.1f, 20f)] public float Aperture;


        void Start()
        {
            #if POSTPROCESSING_INSTALLED
                _volume = GetComponent<PostProcessVolume>();
                _profile = _volume.profile;
                _profile.TryGetSettings<DepthOfField>(out _depthOfField);
            #endif
        }

        void Update()
        {

            // Set variables
        #if POSTPROCESSING_INSTALLED
            // Get distance from camera and target
            float distance = Vector3.Distance(transform.position, FocusTargets[Mathf.FloorToInt(FocusTargetID)].position);
            _depthOfField.focusDistance.Override(distance);
            _depthOfField.aperture.Override(Aperture);
#endif
        }
    }
}
