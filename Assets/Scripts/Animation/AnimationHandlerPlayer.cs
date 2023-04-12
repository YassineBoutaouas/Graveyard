using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Detections;
using Graveyard.CharacterSystem.Player;
using Graveyard.Combat;
using Graveyard.CharacterSystem.Animations.Procedural;
using Graveyard.Abilities;

namespace Graveyard.CharacterSystem.Animations.Player
{
    public class AnimationHandlerPlayer : AnimationHandler
    {
        private PlayerCharacterController _playerController;
        private InputManager _inputManager;
        public IKHandler IkHandler;
        private AbilityManager _abilityManagerPlayer;
        private AttackHandler _attackHandler;
        private SoundHandler _soundHandler;
        private int stepCounter = 2;

        private const string _isGrounded_Parameter = "IsGrounded";
        private const string _forwardInput_Parameter = "ForwardSpeed";

        protected DetectionObject animationGroundCheck;

        protected override void OnEnable()
        {
            base.OnEnable();
            _inputManager = InputManager.GetInstance();
            _playerController = (PlayerCharacterController)characterController;
            _abilityManagerPlayer = GetComponent<AbilityManager>();
            _attackHandler = GetComponent<AttackHandler>();
            animationGroundCheck = _playerController.Detections.GetDetectionObject("AnimationGround");
            _soundHandler = GetComponent<SoundHandler>();
        }

        protected override void Start()
        {
            base.Start();
            IkHandler.Initialize(CharacterAnimator, _playerController);

            _attackHandler.Instrument.OnAttackStart += ctx => CharacterAnimator.CrossFade(_attackHandler.Instrument.AttackAnimations[ctx], 0f);

            _attackHandler.OnBadHit += () => CharacterAnimator.CrossFade("BadHitStun_In", 0f);
            _attackHandler.OnBadHitRecover += () => CharacterAnimator.CrossFade("BadHitStun_Out", 0f);
            _attackHandler.OnBadHitReset += () => CharacterAnimator.CrossFade("Idle", 0.1f);

            _playerController.GroundDetection.OnValueChange += ctx => OnPlayParticlesAtPosition(ctx == true ? "Jump_Land" : "Jump_Up");
        }

        protected override void SetAnimationEventListeners()
        {
            base.SetAnimationEventListeners();
            _abilityManagerPlayer.GetAbilityByName("Jump").OnAbilityStarted += () => CharacterAnimator.SetTrigger("Jump");

            _abilityManagerPlayer.GetAbilityByName("Dash").OnAbilityDelayStart += () => CharacterAnimator.CrossFade("GroundDodge", 0.1f);
            _abilityManagerPlayer.GetAbilityByName("Dash").OnAbilityEnded += () => CharacterAnimator.SetTrigger("DodgeOut");
        }

        protected override void UpdateLocomotionValues()
        {
            base.UpdateLocomotionValues();
            CharacterAnimator.SetBool(_isGrounded_Parameter, animationGroundCheck.detectedCollisions.Length > 0);

            CharacterAnimator.SetFloat(_forwardInput_Parameter, characterController.CanMove ? _inputManager.MoveInput.magnitude * _playerController.SpeedMultiplier : 0f, 0.1f, Time.fixedDeltaTime);
        }

        protected override void OnAnimatorIK(int layerIndex)
        {
            base.OnAnimatorIK(layerIndex);
            IkHandler.AnimatorIK();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            IkHandler.UpdateIKs();
        }

        //protected void SetPivot()
        //{
        //    if (Vector3.Dot(_playerController.CalculateRelativeMovement().normalized, _playerController.transform.forward) < 0)
        //    {
        //        CharacterAnimator.SetTrigger("Pivot");
        //        _playerController.CanMove = false;
        //        _playerController.CanRotate = false;
        //    }
        //}

        #region Animation binding methods



        #endregion

        public override void OnStep(AnimationEvent stepEvent)
        {
            base.OnStep(stepEvent);
            _soundHandler.PlaySound("Step0" + stepCounter.ToString());

            stepCounter = 3 - stepCounter;

        }

        protected void OnDestroy()
        {
            _attackHandler.Instrument.ResetOnAttack();
            _attackHandler.ResetOnBadHit();
            _attackHandler.ResetOnBadHitRecover();
            _attackHandler.ResetOnBadHitReset();
            _playerController.GroundDetection.ResetOnValueChange();

            foreach (Ability ability in _abilityManagerPlayer.Abilities.Values)
                ability.UnbindEvents();
        }
    }
}