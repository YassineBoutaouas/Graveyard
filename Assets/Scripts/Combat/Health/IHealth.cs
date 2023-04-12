using System;
using UnityEngine;

namespace Graveyard.Health
{
    public interface IHealth
    {
        public event Action OnHealthDisplay;
        public event Action<float> OnDamageTaken;
        public event Action<bool> OnStunned;
        public event Action<float> OnRestoreHealth;
        public event Action<float, float> OnMaxHealthChange;
        public event Action OnDeath;
        public event Action<float, float> OnRespawn;

        public void DisplayHealth();

        public void TakeDamage(float damageAmount, float beatValue, GameObject causer);

        public void RestoreHealth(float restoredHealth);

        public void ChangeMaxHealth(float newMaxHealth);

        public void Death();

        public void Respawn();
    }
}