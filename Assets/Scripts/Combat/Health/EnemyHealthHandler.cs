using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Graveyard.CharacterSystem.Enemy;

namespace Graveyard.Health
{
    public class EnemyHealthHandler : HealthHandler
    {
        [Header("Display interpolation")]
        [Space(5)]
        public float DelayTime = 0.5f;
        public float SmoothingTime = 0.2f;

        private RectTransform _previousHealthDisplay;
        private RectTransform _currentHealthDisplay;
        private UnityEngine.UI.Image _recoveryFill;
        private EnemyCharacterHandler _enemyController;
        private IEnumerator _smoothHealth;
        private IEnumerator _recoveryRoutine;

        protected override void Start()
        {
            base.Start();
            _enemyController = (EnemyCharacterHandler)characterHandler;

            _previousHealthDisplay = _enemyController.EnemyHUD.GetHUDElement("LocalEnemyHUD").ImageElements["HealthBar_Interpolated"].GetComponent<RectTransform>();
            _currentHealthDisplay = _enemyController.EnemyHUD.GetHUDElement("LocalEnemyHUD").ImageElements["HealthBar_Health"].GetComponent<RectTransform>();
            _recoveryFill = _enemyController.EnemyHUD.GetHUDElement("LocalEnemyHUD").ImageElements["RecoveryBar"];

            _smoothHealth = SmoothBarDisplay(DelayTime, SmoothingTime, _currentHealthDisplay, _previousHealthDisplay);
        }

        public override void InitializeDeathAction() { deathAction = ExecuteKnockout; }

        public virtual void ExecuteKnockout()
        {
            _enemyController.Group.CurrentActiveEnemies--;
            if (_enemyController.Group.CurrentActiveEnemies <= 0)
            {
                _enemyController._hurtState.IsLastHit = true;
                _enemyController.Group.SetChoirActive(_enemyController);
                IsDamagable = false;
            }
            else
            {
                IsRecovering = true;
                isDamaged = false;
                IsDamagable = false;
                _recoveryFill.gameObject.SetActive(true);

                CurrentStun = 0;
                RelativeStun = 0;
                
                ChangeBarDisplay(_previousHealthDisplay, CurrentHealth);

                _recoveryRoutine = ExecuteRecovery();
                StartCoroutine(_recoveryRoutine);
            }
        }

        private IEnumerator ExecuteRecovery()
        {
            float t = 0;
            while (t <= _enemyController._recoveryState.RecoveryTime)
            {
                yield return null;
                t += Time.deltaTime;
                _recoveryFill.fillAmount = Mathf.Lerp(0, 1, t / _enemyController._recoveryState.RecoveryTime);
            }

            _enemyController.Group.CurrentActiveEnemies++;
            _recoveryFill.gameObject.SetActive(false);
            _recoveryFill.fillAmount = 0;
            RestoreHealth(MaxHealth / 2);

            _enemyController._recoveryState.DelayReturn = true;

            yield return new WaitForSeconds(_enemyController._recoveryState.RecoveryDelayTime);
            IsDamagable = true;
            IsRecovering = false;
            _recoveryRoutine = null;
        }

        public override void DisplayHealth()
        {
            base.DisplayHealth();

            ChangeBarDisplay(_currentHealthDisplay, RelativeHealthLeft);

            StopCoroutine(_smoothHealth);
            _smoothHealth = null;
            _smoothHealth = SmoothBarDisplay(DelayTime, SmoothingTime, _currentHealthDisplay, _previousHealthDisplay);
            StartCoroutine(_smoothHealth);
        }
    }
}