using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Enemy;
using Graveyard.CharacterSystem.Player;

namespace Graveyard.AI
{
    [System.Serializable]
    public class BlockingState : AttackState
    {
        #region Public variables
        public float BlockingTimeOut = 2f;
        public float BlockingTime = 0.5f;
        public float BlockBreakDelay = 2f;

        public int HitsToBreak = 5;

        public bool IsBlocking;
        public bool CanCounterAttack;
        [ReadOnly] public bool CoolingDown;
        #endregion

        #region Non-public variables
        private float _blockTimeOut;
        private float _afterBlockTime;

        private int _hitsTaken;
        private bool _hasBlocked;
        private bool _hasBlockProbability;

        private TMPro.TextMeshProUGUI _shieldDisplay_Text;
        private HUDElementController _shieldDisplay;
        #endregion

        public override void OnInitialize(CharacterHandler character)
        {
            base.OnInitialize(character);
            _shieldDisplay = EnemyController.EnemyHUD.GetHUDElement("ShieldDisplay");
            _shieldDisplay_Text = _shieldDisplay.TextElements["ShieldIcon_Amount"];

            EnemyController.Group.OnEngaged += _ => { _hasBlockProbability = Random.Range(0f, 1f) <= Probability; EnemyController.EnemyHUD.EnableHUDElement("ShieldDisplay", _hasBlockProbability); };
            EnemyController.Group.OnDisEngaged += _ => { EnemyController.EnemyHUD.EnableHUDElement("ShieldDisplay", false); _hitsTaken = 0; };

            _hitsTaken = 0;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            if(EnemyController.CharacterNavmeshAgent.enabled)
                EnemyController.CharacterNavmeshAgent.isStopped = true;

            _shieldDisplay_Text.text = (HitsToBreak - _hitsTaken).ToString();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            if (_hasBlocked)
            {
                if (_afterBlockTime > 0)
                    _afterBlockTime -= Time.deltaTime;
                else
                {
                    _hasBlocked = false;
                    EnemyController.CharacterAnimator.CrossFade("BlockingIdle", 0f);
                }
            }

            if (_blockTimeOut > 0)
                _blockTimeOut -= Time.deltaTime;
            else
                IsBlocking = false;
        }

        public override void OnStateExit()
        {
            if (EnemyController.CharacterNavmeshAgent.enabled)
                EnemyController.CharacterNavmeshAgent.isStopped = false;

            EnemyController.AttackHandler.Invincible = false;
            _hasBlocked = false;

            EnemyController.AttackHandler.GetAttack();
        }

        public override bool CanAttackHit()
        {
            return IsBlocking = _hasBlockProbability && !EnemyController.CharacterHealthHandler.IsStunned && !EnemyController.CharacterHealthHandler.IsDamaged && !CoolingDown;
        }

        public void AddHits(AudioSpectrumManager.BeatEvaluation evaluation)
        {
            if (IsBlocking == false) return;

            if (evaluation == AudioSpectrumManager.BeatEvaluation.Good || evaluation == AudioSpectrumManager.BeatEvaluation.Perfect)
            {
                _hitsTaken++;
                _shieldDisplay_Text.text = (HitsToBreak - _hitsTaken).ToString();

                EnemyController.Group.StartAggroTimer();

                ObjectPoolerManager.Instance.SpawnFromPool("BlockingParticles", EnemyController.transform.position + Vector3.up, Quaternion.identity);
                EnemyController.CharacterAnimator.CrossFade("BlockingStance", 0f);
                EnemyController.RotateTowards(GameManager.Instance.PlayerController.transform.position - EnemyController.transform.position);
                _blockTimeOut = BlockingTimeOut;
                _afterBlockTime = BlockingTime;

                CameraShakerManager.Instance.PulseShake(2, 0.5f, 0.2f);

                _hasBlocked = true;

                if (_hitsTaken >= HitsToBreak)
                {
                    ObjectPoolerManager.Instance.SpawnFromPool("BlockBreakParticles", EnemyController.transform.position + Vector3.up, Quaternion.identity);
                    EnemyController.CharacterAnimator.CrossFade("BlockBreak", 0f);
                    CameraShakerManager.Instance.PulseShake(5, 1f, 0.2f);
                    EnemyController.StartCoroutine(DelayStun());
                    EnemyManager.Instance.StartCoroutine(ExecuteCoolDown());
                }
            }
            else
            {
                if (CanCounterAttack)
                {
                    EnemyController.AttackHandler.Invincible = true;

                    EnemyController.CharacterAnimator.CrossFade("BlockParry", 0f);
                    EnemyController.AttackHandler.CurrentAttackState = this;

                    PlayerController.CharacterAttackHandler.MissHit();
                    EnemyManager.Instance.StartCoroutine(ExecuteCoolDown());

                    EnemyController.Group.StartAggroTimer();
                }
                else
                {
                    PlayerController.CharacterAttackHandler.OnBadHitExecuted();
                    IsBlocking = false;

                    EnemyController.Group.StartAggroTimer();
                }
            }
        }

        public override void OnAttack()
        {
            IsBlocking = false;

            OnExecuteAttack();
            EnemyController.StartCoroutine(ResetAttack());
        }

        #region Routines
        public override IEnumerator ExecuteCoolDown()
        {
            CoolingDown = true;
            UnityEngine.UI.Image shieldMask = EnemyController.EnemyHUD.GetHUDElement("ShieldDisplay").ImageElements["ShieldIcon_Mask"];
            EnemyController.EnemyHUD.GetHUDElement("ShieldDisplay").ImageElements["HealthBar_Blocked"].gameObject.SetActive(false);
            shieldMask.fillAmount = 0;
            
            float t = 0;

            while(t < CoolDown)
            {
                t += Time.deltaTime;
                yield return null;

                shieldMask.fillAmount = Mathf.Lerp(0, 1, t / CoolDown);
            }

            _shieldDisplay_Text.text = (HitsToBreak - _hitsTaken).ToString();

            _hitsTaken = 0;
            EnemyController.EnemyHUD.GetHUDElement("ShieldDisplay").ImageElements["HealthBar_Blocked"].gameObject.SetActive(true);
            CoolingDown = false;
        }

        private IEnumerator DelayStun()
        {
            yield return new WaitForSeconds(BlockBreakDelay);
            EnemyController.CharacterHealthHandler.ImmediateStun();
            _hitsTaken = 0;
            IsBlocking = false;
        }
        #endregion
    }
}