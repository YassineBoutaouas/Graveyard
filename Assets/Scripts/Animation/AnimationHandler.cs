using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graveyard.CharacterSystem.Animations
{
    public class AnimationHandler : MonoBehaviour
    {
        private Animator _animator;
        public Animator CharacterAnimator { get { return _animator; } }
        protected CharacterHandler characterController;
        protected RuntimeAnimatorController runTimeAnimatorController;

        public Dictionary<string, int> AnimationStates = new Dictionary<string, int>();

        protected virtual void OnEnable() { }

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
            runTimeAnimatorController = _animator.runtimeAnimatorController;
            characterController = GetComponent<CharacterHandler>();

            characterController.OnCharacterSetUp += SetAnimationEventListeners;
        }

        protected virtual void Start()
        {
        }

        protected virtual void FixedUpdate() { }

        protected virtual void Update()
        {
            UpdateLocomotionValues();
        }

        protected virtual void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        protected virtual void OnAnimatorIK(int layerIndex) { }

        protected virtual void UpdateLocomotionValues() { }

        protected virtual void SetAnimationEventListeners() { }

        public void OnPlayParticlesAtPosition(string particleTag)
        {
            ObjectPoolerManager.Instance.SpawnFromPool(particleTag, characterController.transform.position, Quaternion.identity);
        }

        public virtual void OnStep(AnimationEvent stepEvent)
        {
            if (!characterController.IsGrounded) return;

            string tag = string.IsNullOrEmpty(stepEvent.stringParameter) ? "Step_Dust" : stepEvent.stringParameter;

            if(stepEvent.intParameter == 0)
                ObjectPoolerManager.Instance.SpawnFromPool(tag, _animator.GetBoneTransform(HumanBodyBones.RightFoot).position, Quaternion.identity);
            else 
                ObjectPoolerManager.Instance.SpawnFromPool(tag, _animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, Quaternion.identity);
        }
    }
}