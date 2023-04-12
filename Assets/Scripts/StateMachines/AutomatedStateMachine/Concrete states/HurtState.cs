using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Enemy;
using UnityEngine;
using Graveyard.CharacterSystem.Animations;
using System;

namespace Graveyard.AI
{
    [System.Serializable]
    public class HurtState : BaseState
    {
        private EnemyCharacterHandler _enemyCharacterHandler;
        private float _elapsedDuration;

        [HideInInspector] public bool IsLastHit;
        private float _hurtDuration = 0.23f;

        public override void OnInitialize(CharacterHandler characterHandler)
        {
            base.OnInitialize(characterHandler);
            _enemyCharacterHandler = (EnemyCharacterHandler)characterHandler;

            _hurtDuration = Array.Find(_enemyCharacterHandler.CharacterAnimator.runtimeAnimatorController.animationClips, c => c.name == "Damage_Strong").length;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            _enemyCharacterHandler.Group.StartAggroTimer();

            characterController.CanMove = false;
            characterController.CanRotate = false;
            characterController.Velocity = Vector3.zero;
            if (characterController.CharacterNavmeshAgent.enabled)
                characterController.CharacterNavmeshAgent.isStopped = true;

            characterController.CharacterAnimator.CrossFade("Damage_Strong", 0f);

            characterController.RotateTowards(GameManager.Instance.PlayerController.transform.position - characterController.transform.position);

            _enemyCharacterHandler.EnemyHUD.EnableHUDElement("HealthBar", true);
            _enemyCharacterHandler.FaceHandler.SetEmotion(FaceSwap.Emotion.shortScream);

            _elapsedDuration = _hurtDuration;
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            while (_elapsedDuration > 0)
            {
                _elapsedDuration -= Time.deltaTime;
                return;
            }

            _enemyCharacterHandler.CharacterHealthHandler.SetIsDamaged(false);
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            if (characterController.CharacterNavmeshAgent.enabled)
                characterController.CharacterNavmeshAgent.isStopped = false;

            characterController.CanMove = true;
            characterController.CanRotate = true;
        }
    }
}