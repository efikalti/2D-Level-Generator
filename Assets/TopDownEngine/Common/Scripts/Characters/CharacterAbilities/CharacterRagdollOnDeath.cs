using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public class CharacterRagdollOnDeath : MonoBehaviour
    {
        [InspectorButton("Ragdoll")]
        public bool RagdollButton;
        public MMRagdoller Ragdoller;

        protected TopDownController _controller;
        protected Health _health;
        
        protected virtual void Awake()
        {
            _health = this.gameObject.GetComponent<Health>();
            _controller = this.gameObject.GetComponent<TopDownController>();
        }

        protected virtual void OnDeath()
        {
            Ragdoll();
        }

        protected virtual void Ragdoll()
        {
            Ragdoller.Ragdolling = true;
            Ragdoller.transform.SetParent(null);
            Ragdoller.MainRigidbody.AddForce(_controller.AppliedImpact.normalized * 10000f, ForceMode.Acceleration);
        }

        protected virtual void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDeath += OnDeath;
            }
        }
        
        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDeath -= OnDeath;
            }
        }
    }
}
