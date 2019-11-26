using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
#if CINEMACHINE_INSTALLED
    using Cinemachine;
#endif

namespace MoreMountains.ToolsForThirdParty
{
    [AddComponentMenu("")]
    public class MMFeedbackCinemachineImpulse : MMFeedback
    {
        [Header("Cinemachine Impulse")]
#if CINEMACHINE_INSTALLED
            [CinemachineImpulseDefinitionProperty]
            public CinemachineImpulseDefinition m_ImpulseDefinition;
#endif
        public Vector3 Velocity;

        public override void SetCustomName()
        {
            Label = "Cinemachine Impulse";
        }

        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
#if CINEMACHINE_INSTALLED
                m_ImpulseDefinition.CreateEvent(position, Velocity);
#endif
            }
        }
    }
}
