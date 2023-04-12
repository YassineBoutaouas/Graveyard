using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Enemy;
using Graveyard.CharacterSystem.Player;

namespace Graveyard.AI
{
    [System.Serializable]
    public class AttackingState : BaseState
    {
        public int RunUpDelay = 4;
        public int CoolDown = 4;

        public float RunUpSpeed = 3f;
        public float Acceleration = 3f;
        public float StoppingDistance = 3f;
        public float HitDistance = 2f;

        public float PushDuration = 0.5f;
        public float PushForce = 10f;
        public int StunDuration = 1;

        public int ClockDamage = 10;

        [HideInInspector] public bool IsAttacking;
        [HideInInspector] public bool CanAttack;
        [HideInInspector] public bool IsCoolingDown;

        private EnemyCharacterHandler _enemyController;
        private PlayerCharacterController _playerController;

        private bool _isInRunUp;
        private bool _startingAttack;

        private float _elapsedTime;
        private float _stunDuration;

        private float _coolDownTime;
        private float _runUpDelayTime;
        private ParticleSystem _attackParticles;
        private SkinnedMeshRenderer _characterRenderer;
        private GameObject _hitParticles;

        public override void OnInitialize(CharacterHandler enemyController)
        {
            base.OnInitialize(enemyController);
            _enemyController = (EnemyCharacterHandler)characterController;
            _playerController = GameManager.Instance.PlayerController;

            _coolDownTime = CoolDown.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);
            _runUpDelayTime = RunUpDelay.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);
            _stunDuration = StunDuration.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);

            _attackParticles = _enemyController.transform.Find("AdditionalObjects").GetComponentInChildren<ParticleSystem>(true);
            _attackParticles.gameObject.SetActive(false);

            _characterRenderer = _enemyController.GetComponentInChildren<SkinnedMeshRenderer>();

            _hitParticles = _playerController.transform.Find("AdditionalObjects").transform.Find("FX_Impact_Player").gameObject;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            CanAttack = false;

            _enemyController.RotateTowards(_playerController.transform.position - _enemyController.transform.position);
            _enemyController.CharacterAnimator.CrossFade("Anticipation", 0.1f);

            _attackParticles.gameObject.SetActive(true);
            _elapsedTime = _runUpDelayTime;

            //_characterRenderer.sharedMaterial.SetFloat("_OutlineOpacity", 1f);

            _startingAttack = false;
            _isInRunUp = false;

            _enemyController.EnemyHUD.EnableHUDElement("HealthBar", true);
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            if (_elapsedTime > 0 && !_isInRunUp)
            {
                _elapsedTime -= Time.deltaTime;

                if (_elapsedTime <= 0)
                {
                    //Debug.Log("Running up to player");
                    _enemyController.CharacterAnimator.CrossFade("RunUp", 0.1f);
                    _enemyController.SwitchPhysicsMode(CharacterHandler.PhysicsMode.navmeshAgent);
                    _enemyController.CharacterNavmeshAgent.SetAgentValues(StoppingDistance, RunUpSpeed, Acceleration);
                    _enemyController.CharacterNavmeshAgent.SetAgentDestination(_playerController.transform.position);
                    _isInRunUp = true;
                }
            }

            if (!_startingAttack && _isInRunUp && Vector3.Distance(_enemyController.transform.position, _enemyController.CharacterNavmeshAgent.destination) < StoppingDistance)
            {
                _enemyController.CharacterAnimator.CrossFade("Attack", 0.1f);
                _startingAttack = true;
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();

            _attackParticles.gameObject.SetActive(false);
            EnemyManager.Instance.CurrentAttackingEnemy = null;

            _startingAttack = false;
            _isInRunUp = false;

            //_characterRenderer.sharedMaterial.SetFloat("_OutlineOpacity", 0f);

            _enemyController.StartCoroutine(ExecuteCoolDown());
        }

        private IEnumerator ExecuteCoolDown()
        {
            yield return new WaitForSeconds(_coolDownTime);
            CanAttack = true;
        }

        public void OnAttack()
        {
            if (Vector3.Distance(_enemyController.transform.position, _playerController.transform.position) <= HitDistance && _playerController.IsDamagable)
            {
                _playerController.IsDamagable = false;
                _playerController.CanMove = false;
                _playerController.CanRotate = false;
                _playerController.CharacterAttackHandler.CanAttack = false;
                _playerController.Velocity = Vector3.zero;

                _playerController.RotateTowards(_enemyController.transform.position - _playerController.transform.position);

                _hitParticles.SetActive(true);

                _playerController.CharacterAttackHandler.ComboManager.ResetCombo();
                _enemyController.StartCoroutine(ResetAttack());

                GameManager.Instance.LevelTimer.OnClockDamaged(ClockDamage);
            }
        }

        private IEnumerator ResetAttack()
        {
            float t = 0;
            while(t < PushDuration)
            {
                yield return null;
                t += Time.deltaTime;

                _playerController.Velocity = (_enemyController.transform.forward + Vector3.up).normalized * PushForce;
            }

            _hitParticles.SetActive(false);
            _playerController.CharacterAnimator.CrossFade("Stun", 0.1f);
            _playerController.Velocity = Vector3.zero;

            yield return new WaitForSeconds(_stunDuration);
            _playerController.CharacterAnimator.CrossFade("Idle", 0.1f);

            _playerController.CharacterAttackHandler.CanAttack = true;

            _playerController.IsDamagable = true;

            _playerController.CanMove = true;
            _playerController.CanRotate = true;
        }
    }
}