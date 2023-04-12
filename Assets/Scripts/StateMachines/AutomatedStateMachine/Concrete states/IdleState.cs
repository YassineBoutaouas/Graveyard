using UnityEngine.AI;
using UnityEngine;
using Graveyard.CharacterSystem.Enemy;
using Graveyard.CharacterSystem;

namespace Graveyard.AI
{
    [System.Serializable]
    public class IdleState : BaseState
    {
        public string[] IdleAnimations;
        private EnemyCharacterHandler _enemyController;

        public override void OnInitialize(CharacterHandler character)
        {
            base.OnInitialize(character);
            _enemyController = (EnemyCharacterHandler)characterController;
            characterController.CharacterAnimator.CrossFade(IdleAnimations[Random.Range(0, IdleAnimations.Length)], 0f);
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            characterController.RotateTowards(_enemyController.Group.transform.position - _enemyController.transform.position);
            characterController.SwitchPhysicsMode(CharacterHandler.PhysicsMode.rootmotion);
            characterController.CharacterAnimator.CrossFade(IdleAnimations[Random.Range(0, IdleAnimations.Length)], 0f);

            _enemyController.EnemyHUD.EnableHUDElement("HealthBar", false);
            _enemyController.FaceHandler.SetEmotion(FaceSwap.Emotion.idle);
        }
    }
}