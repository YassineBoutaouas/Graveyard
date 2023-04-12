using Graveyard.CharacterSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graveyard.AI
{
    [System.Serializable]
    public class PushAttackState : AttackState
    {
        [Space(10)]
        public float RunUpDelay;
        public float RunUpSpeed = 3f;
        public float Acceleration = 3f;
        public float StoppingDistance = 3f;

        private float _elapsedTime;
        private bool _startedAttack;
        private bool _isInRunUp;

        private ParticleSystem _attackParticles;

        public override void OnInitialize(CharacterHandler character)
        {
            base.OnInitialize(character);

            _attackParticles = EnemyController.transform.Find("AdditionalObjects").GetComponentInChildren<ParticleSystem>(true);
            _attackParticles.gameObject.SetActive(false);
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            if (EnemyController.CharacterNavmeshAgent.enabled)
                EnemyController.CharacterNavmeshAgent.isStopped = true;

            EnemyController.CharacterAnimator.CrossFade("Anticipation", 0.1f);

            _attackParticles.gameObject.SetActive(true);
            _elapsedTime = RunUpDelay;

            _isInRunUp = false;
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            if (_elapsedTime > 0)
            {
                _elapsedTime -= Time.deltaTime;
                return;
            }

            if (!_isInRunUp)
            {
                if (EnemyController.CharacterNavmeshAgent.enabled)
                    EnemyController.CharacterNavmeshAgent.isStopped = false;

                EnemyController.CharacterAnimator.CrossFade("RunUp", 0.1f);
                EnemyController.SwitchPhysicsMode(CharacterHandler.PhysicsMode.navmeshAgent);
                EnemyController.CharacterNavmeshAgent.SetAgentValues(StoppingDistance, RunUpSpeed, Acceleration);
                EnemyController.CharacterNavmeshAgent.SetAgentDestination(PlayerController.transform.position);

                _isInRunUp = true;
            }

            if (!_startedAttack && _isInRunUp && Vector3.Distance(EnemyController.transform.position, EnemyController.CharacterNavmeshAgent.destination) < StoppingDistance)
            {
                EnemyController.CharacterAnimator.CrossFade("Attack", 0.1f);
                _startedAttack = true;
                EnemyController.CharacterSoundHandler.PlaySound("Push");
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();

            _attackParticles.gameObject.SetActive(false);
            _startedAttack = false;
            _isInRunUp = false;
          
        }
    }
}