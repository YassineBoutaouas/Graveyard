using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Enemy;
using Graveyard.AI;

namespace Graveyard.CharacterSystem.Animations.Enemy
{
    public class AnimationHandlerEnemy : AnimationHandler
    {
        private EnemyCharacterHandler _enemyCharacterHandler;
        private IdleState _idleState;
        private SoundHandler _soundHandler;
        private int stepCounter = 2;

        protected override void Start()
        {
            base.Start();
            _soundHandler = GetComponent<SoundHandler>();
        }

        protected override void SetAnimationEventListeners()
        {
            base.SetAnimationEventListeners();
            _enemyCharacterHandler = (EnemyCharacterHandler)characterController;
            _idleState = _enemyCharacterHandler._idleState;

            //characterController.CharacterHealthHandler.OnDamageTaken += _ => OnDamaged();
        }

        protected override void UpdateLocomotionValues()
        {
            base.UpdateLocomotionValues();
            CharacterAnimator.SetFloat("Movement", _enemyCharacterHandler.CharacterNavmeshAgent.velocity.magnitude);
        }

        //protected void OnDamaged()
        //{
        //    string value = Enum.GetName(typeof(AudioSpectrumManager.BeatEvaluation), _enemyCharacterHandler.PlayerController.CharacterAttackHandler.CurrentBeatEvaluation);
        //    _enemyCharacterHandler.CharacterSoundHandler.PlaySound(value + "Note");
        //
        //    if (value != "Bad")
        //        _enemyCharacterHandler.CharacterSoundHandler.PlaySound("StandardHit");
        //
        //    _enemyCharacterHandler.CharacterSoundHandler.PlaySound(value + "Hit");
        //   
        //}

        public void SpawnStepParticles()
        {

        }

        public override void OnStep(AnimationEvent stepEvent)
        {
            base.OnStep(stepEvent);
            _soundHandler.PlaySound("Step0" + stepCounter.ToString());

            stepCounter = 3 - stepCounter;

        }
    }
}