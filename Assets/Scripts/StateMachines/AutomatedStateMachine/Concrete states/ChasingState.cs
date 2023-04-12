using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Enemy;

namespace Graveyard.AI
{
    [System.Serializable]
    public class ChasingState : BaseState
    {
        public float ChaseSpeed = 20f;
        public float Acceleration = 8f;
        public float ChasingDistance = 8f;
        public float AttackingDistance = 10f;
        public float ChasingDelay = 4;

        [HideInInspector] public bool IsInRange;
        private EnemyCharacterHandler _enemyController;
        private Vector3 _previousPosition;

        public override void OnInitialize(CharacterHandler characterController)
        {
            base.OnInitialize(characterController);
            _enemyController = (EnemyCharacterHandler)characterController;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            _enemyController.SwitchPhysicsMode(CharacterHandler.PhysicsMode.navmeshAgent);
            _enemyController.CharacterNavmeshAgent.SetAgentValues(1f, ChaseSpeed, Acceleration);
            _enemyController.CharacterAnimator.CrossFade("ChasingIdle", 0f);

            _enemyController.Group.AddChaser(_enemyController);

            _previousPosition = GameManager.Instance.PlayerController.transform.position;

            _enemyController.EnemyHUD.EnableHUDElement("HealthBar", true);
            _enemyController.FaceHandler.SetEmotion(FaceSwap.Emotion.angry);

            if (_enemyController.EnemyStateMachine.LastStateName == _enemyController._idleState.GetType().Name)
            {
                ObjectPoolerManager.Instance.SpawnFromPool("Alert", _enemyController.transform.position + Vector3.up * 2, Quaternion.identity);
                _enemyController.CharacterAnimator.CrossFade("AngryReaction", 0f);
                _enemyController.StartCoroutine(DelayChasing());
            }
            else
                characterController.CharacterNavmeshAgent.SetAgentDestination(GetPosition());

            _enemyController.RotateTowards(GameManager.Instance.PlayerController.transform.position - _enemyController.transform.position);
        }

        private IEnumerator DelayChasing()
        {
            if (_enemyController.CharacterNavmeshAgent.enabled)
                _enemyController.CharacterNavmeshAgent.isStopped = true;
            
            yield return new WaitForSeconds(ChasingDelay);
            _enemyController.CharacterAnimator.CrossFade("ChasingIdle", 0.1f);

            if (_enemyController.CharacterNavmeshAgent.enabled)
                _enemyController.CharacterNavmeshAgent.isStopped = false;

            characterController.CharacterNavmeshAgent.SetAgentDestination(GetPosition());
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            if (Vector3.Distance(_previousPosition, GameManager.Instance.PlayerController.transform.position) > ChasingDistance)
            {
                characterController.CharacterNavmeshAgent.SetAgentDestination(GetPosition());
                _previousPosition = GameManager.Instance.PlayerController.transform.position;
            }

            if (Vector3.Distance(_enemyController.CharacterNavmeshAgent.destination, _enemyController.transform.position) < 2f && _enemyController.CharacterNavmeshAgent.velocity.magnitude < 2f)
                _enemyController.CurrentOrientationMethod = _enemyController.RotateTowardsPlayer;
            else if (_enemyController.CurrentOrientationMethod == _enemyController.RotateTowardsPlayer)
                _enemyController.CurrentOrientationMethod = _enemyController.RotateTowardsMovement;

            if (Vector3.Distance(_enemyController.transform.position, GameManager.Instance.PlayerController.transform.position) <= AttackingDistance)
                EnemyManager.Instance.AddEnemyToAggroList(_enemyController);
            else
                EnemyManager.Instance.RemoveEnemyFromAggroList(_enemyController);
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            _enemyController.StopCoroutine(DelayChasing());

            _enemyController.Group.RemoveChaser(_enemyController);
            _enemyController.CurrentOrientationMethod = _enemyController.RotateTowardsMovement;

            EnemyManager.Instance.RemoveEnemyFromAggroList(_enemyController);

            _enemyController.FaceHandler.SetEmotion(FaceSwap.Emotion.idle);
        }

        private Vector3 GetPosition()
        {
            float segment = _enemyController.Group.ChasingEnemies.IndexOf(_enemyController) * (2 * Mathf.PI) / _enemyController.Group.ChasingEnemies.Count;
            float x = Mathf.Cos(segment) * ChasingDistance;
            float z = Mathf.Sin(segment) * ChasingDistance;

            return new Vector3(GameManager.Instance.PlayerController.transform.position.x + x, _enemyController.transform.position.y, GameManager.Instance.PlayerController.transform.position.z + z);
        }
    }
}