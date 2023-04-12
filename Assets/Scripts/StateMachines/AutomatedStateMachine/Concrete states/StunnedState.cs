using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Enemy;
using Graveyard.Health;

namespace Graveyard.AI
{
    [System.Serializable]
    public class StunnedState : BaseState
    {
        public int StunDuration = 10;

        private CharacterHandler.PhysicsMode _previousPhysicsMode;
        private EnemyCharacterHandler _enemyCharacterHandler;
        private ParticleSystem _stunnedStars;

        public override void OnInitialize(CharacterHandler character)
        {
            base.OnInitialize(character);
            _enemyCharacterHandler = (EnemyCharacterHandler)character;
            _enemyCharacterHandler.CharacterHealthHandler.StunDuration = StunDuration.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);
            _enemyCharacterHandler.FaceHandler.SetEmotion(FaceSwap.Emotion.stunned);
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            ObjectPoolerManager.Instance.SpawnFromPool("FX_Star_Stun_OneShot", characterController.transform.position + Vector3.up, Quaternion.identity);
            _stunnedStars = ObjectPoolerManager.Instance.SpawnFromPool("Stunned_Stars", characterController.transform.position + Vector3.up * 2.1f, Quaternion.identity).GetComponent<ParticleSystem>();

            _previousPhysicsMode = characterController.CurrentPhysicsMode;
            characterController.CharacterAnimator.CrossFade("Damage_Stun", 0.1f);

            characterController.Velocity = Vector3.zero;
            characterController.CanMove = false;
            characterController.CanRotate = false;
            characterController.SwitchPhysicsMode(CharacterHandler.PhysicsMode.kinematic);

            _enemyCharacterHandler.Group.StartAggroTimer();

        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            characterController.CharacterAnimator.CrossFade("ChasingIdle", 0.1f);

            characterController.SwitchPhysicsMode(_previousPhysicsMode);
            characterController.CanMove = true;
            characterController.CanRotate = true;

            _stunnedStars.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            //if (_enemyCharacterHandler._hurtState.IsLastHit)
            //    characterController.CharacterAnimator.CrossFade("Death", 0f);

            _enemyCharacterHandler.FaceHandler.SetEmotion(FaceSwap.Emotion.idle);

        }
    }
}