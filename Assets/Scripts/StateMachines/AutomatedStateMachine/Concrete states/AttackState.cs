using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Enemy;
using Graveyard.CharacterSystem.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graveyard.AI
{
    [System.Serializable]
    public class AttackState : BaseState
    {
        [Range(0, 1)]
        public float Probability = 0.5f;
        public bool Uncancelable;

        [Space(5)]
        public float CoolDown;

        [Header("Damage values")]
        [Space(10)]
        public float PushDuration = 0.5f;
        public float PushForce = 10f;
        public float StunDuration = 1;

        public int ClockDamage = 10;

        public float Range = 2f;

        public string StunAnimation = "Bottle_Stun";

        #region Properties
        public EnemyCharacterHandler EnemyController { get { return _enemyController; } }
        public PlayerCharacterController PlayerController { get { return _playerController; } }
        public EnemyAttackHandler AttackHandler { get { return _enemyAttackHandler; } }
        public GameObject PlayerHitParticles { get { return _playerHitParticles; } }
        #endregion

        #region Private variables
        private EnemyCharacterHandler _enemyController;
        private PlayerCharacterController _playerController;
        private EnemyAttackHandler _enemyAttackHandler;

        private GameObject _playerHitParticles;
        #endregion

        public override void OnInitialize(CharacterHandler character)
        {
            base.OnInitialize(character);
            _enemyController = (EnemyCharacterHandler)characterController;
            _playerController = GameManager.Instance.PlayerController;
            _enemyAttackHandler = _enemyController.AttackHandler;

            _playerHitParticles = _playerController.transform.Find("AdditionalObjects").transform.Find("FX_Impact_Player").gameObject;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            _enemyController.RotateTowards(_playerController.transform.position - _enemyController.transform.position);
            //_enemyController.EnemyHUD.EnableHUDElement("HealthBar", true);

            if(_enemyController.CharacterNavmeshAgent.enabled)
                EnemyController.CharacterNavmeshAgent.ResetPath();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            _enemyAttackHandler.IsCoolingDown = true;
            _enemyAttackHandler.IsAttacking = false;

            EnemyManager.Instance.CurrentAttackingEnemy = null;
            EnemyManager.Instance.StartCoroutine(ExecuteCoolDown());

            _enemyAttackHandler.GetAttack();
        }

        public virtual void OnAttack()
        {
            if (CanAttackHit())
            {
                OnExecuteAttack();
                _enemyController.StartCoroutine(ResetAttack());
            }

            _enemyAttackHandler.IsAttacking = false;
        }

        public virtual void OnExecuteAttack()
        {
            _playerController.IsDamagable = false;
            _playerController.CanMove = false;
            _playerController.CanRotate = false;
            _playerController.Velocity = Vector3.zero;
            _playerController.RotateTowards(_enemyController.transform.position - _playerController.transform.position);

            _playerHitParticles.SetActive(true);
            _playerController.CharacterAttackHandler.ComboManager.ResetCombo();

            GameManager.Instance.LevelTimer.OnClockDamaged(ClockDamage);
        }

        public virtual IEnumerator ResetAttack()
        {
            float t = 0;
            while (t < PushDuration)
            {
                yield return null;
                t += Time.deltaTime;

                _playerController.Velocity = (-_playerController.transform.forward + Vector3.up).normalized * PushForce;
            }

            _playerHitParticles.SetActive(false);
            _playerController.CharacterAnimator.CrossFade(StunAnimation, 0.1f);
            _playerController.Velocity = Vector3.zero;

            yield return new WaitForSeconds(StunDuration);
            _playerController.CharacterAnimator.CrossFade("Idle", 0.1f);

            _playerController.CharacterAttackHandler.CanAttack = true;
            _playerController.IsDamagable = true;
            _playerController.CanMove = true;
            _playerController.CanRotate = true;
        }

        public virtual IEnumerator ExecuteCoolDown()
        {
            yield return new WaitForSeconds(CoolDown);
            _enemyAttackHandler.CanAttack = true;
            _enemyAttackHandler.IsCoolingDown = false;
        }

        public virtual bool CanAttackHit()
        {
            return Vector3.Distance(_enemyController.transform.position, _playerController.transform.position) <= Range && _playerController.IsDamagable;
        }
    }
}