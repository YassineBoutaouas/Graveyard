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
    public class BottleThrowingState : AttackState
    {
        [Space(10)]
        public AnimationCurve ThrowingInterpolation;

        public float PickUpDelay = 1;
        public float TrackingDuration = 1;
        public float ThrowingDuration = 3;
        public float ThrowRecoveryDuration = 2;

        private float _elapsedTime;
        private float _trackingTime;
        private bool _bottleThrownMotion;
        private bool _bottleThrown;
        private bool _bottlePicked;

        private GameObject _indicator;
        private Animator _dodgePromptAnimator;
        private BottleHandler _bottle;
        private ParticleSystem _attackParticles;
        private ObjectPoolerManager.Pool _bottlePool;

        public override void OnInitialize(CharacterHandler character)
        {
            base.OnInitialize(character);

            _attackParticles = EnemyController.transform.Find("AdditionalObjects").transform.Find("FX_Throwing").GetComponent<ParticleSystem>();
            _attackParticles.gameObject.SetActive(false);
            _bottlePool = ObjectPoolerManager.Instance.GetPool("Bottle");
            _dodgePromptAnimator = GlobalHUDManager.Instance.GetHUDElement("FX_Dodge").gameObject.GetComponent<Animator>();
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            _attackParticles.gameObject.SetActive(true);
            _bottleThrown = false;

            EnemyController.CharacterNavmeshAgent.isStopped = true;
            EnemyController.RotateTowards(PlayerController.transform.position - EnemyController.transform.position);

            _elapsedTime = PickUpDelay;
            _trackingTime = TrackingDuration;

            EnemyController.CharacterAnimator.CrossFade("Anticipation", 0.1f);
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            while (_elapsedTime > 0)
            {
                _elapsedTime -= Time.deltaTime;
                return;
            }

            if (!_bottlePicked)
            {
                EnemyController.CharacterAnimator.CrossFade("Throw_In", 0f);
                _trackingTime = TrackingDuration;

                _indicator = ObjectPoolerManager.Instance.SpawnFromPool("BottleIndicator", GameManager.Instance.PlayerController.transform.position, Quaternion.Euler(-90, 0, 0));

                _indicator.transform.localScale = Vector3.one * Range;

                _bottle = ObjectPoolerManager.Instance.SpawnFromPool("Bottle", EnemyController.transform.position, _bottlePool.Prefab.transform.rotation).GetComponent<BottleHandler>();

                _bottle.transform.SetParent(EnemyController.CharacterAnimator.GetBoneTransform(HumanBodyBones.RightHand));
                _bottle.transform.localPosition = _bottlePool.Prefab.transform.position;
                _bottle.transform.localRotation = _bottlePool.Prefab.transform.rotation;

                _bottlePicked = true;
            }

            while (_trackingTime > 0)
            {
                _trackingTime -= Time.deltaTime;
                _indicator.transform.position = GameManager.Instance.PlayerController.transform.position + Vector3.up * 0.5f;
                EnemyController.RotateTowards(PlayerController.transform.position - EnemyController.transform.position);
                return;
            }

            if (_bottlePicked)
            {
                EnemyController.RotateTowards(PlayerController.transform.position - EnemyController.transform.position);
                _indicator.transform.position = GameManager.Instance.PlayerController.transform.position + Vector3.up * 0.5f;
            }

            if (!_bottleThrownMotion)
            {
                _dodgePromptAnimator.gameObject.SetActive(true);
                _dodgePromptAnimator.Play("MissedPrompt");
                EnemyController.CharacterAnimator.CrossFade("Throw_Out", 0f);
                _bottleThrownMotion = true;
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();

            if (_bottle && !_bottleThrown)
                ReleaseBottle();

            if(_indicator)
                _indicator.SetActive(false);

            _bottleThrownMotion = false;
            _bottlePicked = false;
            _bottleThrown = false;
            _attackParticles.gameObject.SetActive(false);
            EnemyController.AttackHandler.IsAttacking = false;

            EnemyController.CharacterNavmeshAgent.isStopped = false;
        }

        public override void OnAttack()
        {
            EnemyController.StartCoroutine(ExecuteThrow());
            EnemyController.StartCoroutine(ExecuteThrowRecovery());
        }

        private IEnumerator ExecuteThrowRecovery()
        {
            yield return new WaitForSeconds(ThrowRecoveryDuration);
            EnemyController.AttackHandler.IsAttacking = false;
        }

        private IEnumerator ExecuteThrow()
        {
            float t = 0;
            _bottleThrown = true;
    
            if(_bottle)
                _bottle.transform.parent = null;
            Vector3 startingPos = _bottle.transform.position;
            _bottle.EnableCollider(true);
            _bottle.OnBottleCollision += OnBottleImpact;

            while (t < ThrowingDuration)
            {
                yield return null;
                t += Time.deltaTime;

                if (_bottle)
                {
                    _bottle.transform.position = Vector3.Lerp(startingPos, PlayerController.transform.position + Vector3.up * 0.5f, ThrowingInterpolation.Evaluate(t / ThrowingDuration));
                    EnemyController.RotateTowards(PlayerController.transform.position - EnemyController.transform.position);

                    if (Vector3.Distance(_bottle.transform.position, PlayerController.transform.position) < 1)
                    {
                        OnBottleImpact();
                        yield break;
                    }
                }
            }
        }

        private void OnBottleImpact()
        {
            ObjectPoolerManager.Instance.SpawnFromPool("BottleImpact", _bottle.transform.position, Quaternion.identity);
            ReleaseBottle();

            if (CanAttackHit())
            {
                OnExecuteAttack();
                EnemyController.StartCoroutine(ResetAttack());
            }

            _bottle.EnableCollider(false);
            _indicator.SetActive(false);
            _bottle.OnBottleCollision -= OnBottleImpact;
            _bottle = null;
            EnemyController.CharacterSoundHandler.PlaySound("BottleImpact");
        }

        private void ReleaseBottle()
        {
            _bottle.gameObject.SetActive(false);
            _bottle.transform.parent = _bottlePool.PoolParent;
            _bottle.transform.localScale = Vector3.one;

            _indicator.SetActive(false);
        }

        public override bool CanAttackHit()
        {
            return (Vector3.Distance(_bottle.transform.position, PlayerController.transform.position) < Range) && PlayerController.IsDamagable;
        }
    }
}