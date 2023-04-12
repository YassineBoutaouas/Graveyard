using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Stats;

namespace Graveyard.Health
{
    public class HealthHandler : MonoBehaviour, IHealth
    {
        #region Events
        public event Action OnHealthDisplay;
        public event Action<float> OnDamageTaken;
        public event Action<bool> OnStunned;
        public event Action<bool> OnStunReset;
        public event Action<float> OnRestoreHealth;
        public event Action<float, float> OnMaxHealthChange;
        public event Action OnDeath;
        public event Action<float, float> OnRespawn;

        public delegate void DeathAction();
        public DeathAction deathAction;
        #endregion

        public enum KnockbackType { small, medium, heavy };

        [Header("Health values")]
        [Space(5)]
        public KnockbackType currentKnockbackType;
        public readonly float MinMaxHealth = 10;

        #region Properties
        public float StunDuration { get; set; }
        public float MaxHealth { get { return _maxHealth; } }
        public float StartMaxHealth { get { return _startMaxHealth; } }
        public float CurrentHealth { get { return _currentHealth; } set { _currentHealth = value; } }
        public float StunResistance { get { return _stunResistance; } }
        public float CurrentStun { get { return _currentStun; } set { _currentStun = value; } }

        public bool IsDamagable { get { return _isDamagable; } set { _isDamagable = value; } }
        public float RelativeHealthLeft { get { return _relativeHealthLeft; } }
        public float RelativeStun { get { return _relativeStun; } set { _relativeStun = value; } }
        public bool IsStunned { get { return _isStunned; } }
        public bool IsDamaged { get { return isDamaged; } }
        public bool IsRecovering { get { return _isRecovering; } set { _isRecovering = value; } }
        #endregion

        #region Non-Public variables
        protected CharacterHandler characterHandler;
        protected bool isDamaged;

        private CharacterStats _characterHandlerStats;

        private float _startMaxHealth;
        private float _maxHealth;
        private float _currentHealth;
        private float _relativeHealthLeft;

        private float _stunResistance;
        private float _currentStun;
        private float _relativeStun;

        private bool _isDamagable = true;
        private bool _isStunned;
        private bool _isRecovering;
        #endregion

        protected virtual void Start()
        {
            characterHandler = GetComponent<CharacterHandler>();
            _characterHandlerStats = characterHandler.Stats;

            _characterHandlerStats.CurrentHealth = _characterHandlerStats.MaxHealth;
            _currentHealth = _characterHandlerStats.CurrentHealth;
            _maxHealth = _characterHandlerStats.MaxHealth;
            _startMaxHealth = _maxHealth;

            _characterHandlerStats.CurrentStun = 0;
            _stunResistance = _characterHandlerStats.StunResistance;
            _currentStun = _characterHandlerStats.CurrentStun;

            OnHealthDisplay += DisplayHealth;
            InitializeDeathAction();
        }

        protected virtual void Update() { }

        public virtual void DisplayHealth()
        {
            if (!gameObject.activeInHierarchy)
                return;
        }

        #region Damage methods
        public void SetIsDamaged(bool damaged) { isDamaged = damaged; }

        public virtual void TakeDamage(float damageAmount, float currentBeatValue, GameObject causer)
        {
            if (!IsDamagable) return;

            SetIsDamaged(true);
            OnDamageTaken?.Invoke(damageAmount);

            _currentHealth -= damageAmount;

            _currentHealth = Mathf.Clamp(_currentHealth, 0, int.MaxValue);

            _relativeHealthLeft = _currentHealth / _maxHealth;

            if (_currentHealth <= 0)
                Death();
            {
                _currentStun += damageAmount;

                _currentStun = Mathf.Clamp(_currentStun, 0, _stunResistance);
                _relativeStun = _currentStun / _stunResistance;

                StopCoroutine(TakeStunDamage());
                StartCoroutine(TakeStunDamage());
            }

            OnHealthDisplay?.Invoke();
        }

        public void ImmediateStun()
        {
            _currentStun = (_stunResistance * _characterHandlerStats.StunResistanceMultiplier);
            StartCoroutine(TakeStunDamage());
        }

        public virtual IEnumerator TakeStunDamage()
        {
            if (_currentStun >= (_stunResistance * _characterHandlerStats.StunResistanceMultiplier) && !_isStunned)
            {
                _isStunned = true;
                OnStunned?.Invoke(_isStunned);

                float t = 0;
                while (t < StunDuration)
                {
                    yield return null;
                    t += Time.deltaTime;

                    _relativeStun = Mathf.Lerp(1, 0, t / StunDuration);
                }

                _isStunned = false;
                _currentStun = 0;
                _relativeStun = 0;

                OnStunned?.Invoke(_isStunned);
            }
            else if(!_isStunned)
            {
                while (_currentStun > 0)
                {
                    yield return null;
                    _currentStun -= Time.deltaTime;
                    _relativeStun = _currentStun / _stunResistance;
                }

                OnStunReset?.Invoke(_isStunned);
                _currentStun = 0;
                _relativeStun = 0;
            }
        }
        #endregion

        #region Death methods
        public virtual void InitializeDeathAction()
        {
            deathAction = DisableCharacter;
        }

        public virtual void Death()
        {
            OnDeath?.Invoke();

            _currentHealth = 0;
            deathAction();

            OnHealthDisplay?.Invoke();
        }

        public virtual void DisableCharacter() => gameObject.SetActive(false);

        public virtual void DisableComponents()
        {
            foreach (Component component in GetComponents<Component>())
                if (!(component is Transform || component is Animator))
                    Destroy(component);
        }
        #endregion

        #region Regeneration methods
        public virtual void RestoreHealth(float restoredHealth)
        {
            OnRestoreHealth?.Invoke(restoredHealth);

            _currentHealth += restoredHealth;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            _relativeHealthLeft = _currentHealth / _maxHealth;

            OnHealthDisplay?.Invoke();
        }

        public virtual void ChangeMaxHealth(float newMaxHealth)
        {
            OnMaxHealthChange?.Invoke(_maxHealth, newMaxHealth);

            _maxHealth += newMaxHealth;
            _maxHealth = Mathf.Clamp(_maxHealth, MinMaxHealth, int.MaxValue);

            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);

            _relativeHealthLeft = _currentHealth / _maxHealth;

            OnHealthDisplay?.Invoke();
        }

        public virtual void SetMaxHealth(float newMaxHealth)
        {
            OnMaxHealthChange?.Invoke(_maxHealth, newMaxHealth);

            _maxHealth = newMaxHealth;
            _currentHealth = _maxHealth;

            _relativeHealthLeft = _currentHealth / _maxHealth;

            OnHealthDisplay?.Invoke();
        }
        #endregion

        public virtual void Respawn()
        {
            _maxHealth = Mathf.Clamp(_maxHealth, 0, int.MaxValue);

            OnRespawn?.Invoke(_currentHealth, _maxHealth);

            _currentHealth = _maxHealth;

            _relativeHealthLeft = _currentHealth / _maxHealth;

            gameObject.SetActive(true);

            OnHealthDisplay?.Invoke();
        }

        #region Bar methods
        public IEnumerator SmoothBarDisplay(float delayTime, float smoothingTime, Transform targetTransform, params Transform[] barDisplays)
        {
            yield return new WaitForSeconds(delayTime);

            float t = 0;
            while (t < smoothingTime)
            {
                yield return null;

                t += Time.deltaTime;

                foreach (Transform bar in barDisplays)
                    bar.localScale = Vector3.Lerp(bar.localScale, targetTransform.localScale, t / smoothingTime);
            }

            foreach (Transform bar in barDisplays)
                bar.localScale = targetTransform.localScale;
        }

        protected virtual void ChangeBarDisplay(RectTransform barDisplay, float relativeValue)
        {
            barDisplay.localScale = new Vector3(relativeValue, barDisplay.localScale.y, 1);
        }
        #endregion

        public virtual void ExecuteKnockback(Vector3 force, GameObject causer) { }
    }
}