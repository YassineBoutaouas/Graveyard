using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Enemy;

namespace Graveyard.AI
{
    [System.Serializable]
    public class ReturnState : BaseState
    {
        public float ReturnSpeed = 20f;
        public float Acceleration = 8f;
        public float StoppingDistance = 2f;

        public int DelayTime = 4;

        public string[] ShoutAnimations;

        private bool _isReturning;

        public bool HasReturned { get { return _hasReturned; } }

        #region Non-Public variables
        private float _delayTime;
        private float _elapsedDelayTime;
        private EnemyCharacterHandler _enemyController;
        private Vector3 _returnPosition;

        private bool _hasReturned;
        #endregion

        public override void OnInitialize(CharacterHandler characterController)
        {
            base.OnInitialize(characterController);
            _enemyController = (EnemyCharacterHandler)characterController;
            _delayTime = DelayTime.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            _returnPosition = _enemyController.Group.ReturnPositions[_enemyController.Group.Enemies.IndexOf(_enemyController)];
            _isReturning = false;

            characterController.CharacterAnimator.CrossFade(ShoutAnimations[Random.Range(0, ShoutAnimations.Length)], 0.1f);
            _enemyController.CanMove = false;
            _enemyController.CanRotate = false;
            _enemyController.SwitchPhysicsMode(CharacterHandler.PhysicsMode.kinematic);
            _elapsedDelayTime = _delayTime;

            _enemyController.EnemyHUD.EnableHUDElement("HealthBar", false);
            _enemyController.FaceHandler.SetEmotion(FaceSwap.Emotion.angry);
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
            _hasReturned = Vector3.Distance(_enemyController.transform.position, _returnPosition) <= StoppingDistance;

            if (!_isReturning)
            {
                if (_elapsedDelayTime > 0)
                    _elapsedDelayTime -= Time.deltaTime;
                else
                {
                    _isReturning = true;

                    _enemyController.CharacterAnimator.CrossFade("ChasingIdle", 0f);

                    _enemyController.CanMove = true;
                    _enemyController.CanRotate = true;
                    _enemyController.SwitchPhysicsMode(CharacterHandler.PhysicsMode.navmeshAgent);

                    _enemyController.CharacterNavmeshAgent.SetAgentValues(StoppingDistance, ReturnSpeed, Acceleration);

                    _enemyController.CharacterNavmeshAgent.SetAgentDestination(_returnPosition);
                }
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            _hasReturned = false;
            _isReturning = false;
            _enemyController.FaceHandler.SetEmotion(FaceSwap.Emotion.idle);
        }
    }
}