using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Graveyard.CharacterSystem;

namespace Graveyard.Abilities
{
    [CreateAssetMenu(fileName = "New Ability", menuName = "Abilities / Ability")]
    public class Ability : ScriptableObject
    {
        protected CharacterHandler controller;

        [Header("Ability")]
        [Space(10)]
        public string AbilityName = "New Ability";

        public int ExecutionAmount;
        public float DelayTime;
        public float ExecutionTime;
        public float CoolDownTime;

        [Space(5)]
        public bool CanInterrupt;
        public bool IsEnabled = true;

        public bool IsDelaying { get { return isDelaying; } }
        public bool IsExecuting { get { return isExecuting; } }
        public bool IsCoolingDown { get { return isCoolingDown; } }

        protected int currentExecutionAmount;
        protected bool isDelaying;
        protected bool isExecuting;
        protected bool isCoolingDown;

        #region Events
        public event Action OnAbilityDelayStart;
        public event Action OnAbilityDelaying;
        public event Action OnAbilityStarted;
        public event Action OnAbilityExecuting;
        public event Action OnAbilityCoolingDown;
        public event Action OnAbilityCanceled;
        public event Action OnAbilityEnded;
        #endregion

        public Ability()
        {

        }

        public virtual void InitializeAbility(CharacterHandler c)
        {
            controller = c;
            currentExecutionAmount = ExecutionAmount;
            isDelaying = false;
            isExecuting = false;
            isCoolingDown = false;
        }

        public virtual void StartDelay()
        {
            OnAbilityDelayStart?.Invoke();
        }

        public virtual void DelayAbility()
        {
            OnAbilityDelaying?.Invoke();
        }

        public virtual void StartAbility()
        {
            OnAbilityStarted?.Invoke();
        }

        public virtual void ExecuteAbility()
        {
            OnAbilityExecuting?.Invoke();
        }

        public virtual void CancelAbility()
        {
            if (!CanInterrupt)
                return;

            if (isExecuting)
            {
                OnAbilityCanceled?.Invoke();

                isExecuting = false;
                controller.StopCoroutine(CalculateAbilityDuration());
            }
        }

        public virtual void EndAbility()
        {
            isExecuting = false;
            OnAbilityEnded?.Invoke();
        }

        public void TryExecuteAbility()
        {
            if (IsEnabled)
            {
                UpdateAbility();

                if (isExecuting)
                {
                    CancelAbility();
                }
                else if (CanExecuteAbility())
                {
                    currentExecutionAmount -= 1;
                    controller.StartCoroutine(CalculateStart());
                }
            }
        }

        public virtual void UpdateAbility()
        {
            if (CanCoolDownAbility())
                controller.StartCoroutine(CalculateCoolDown());

            if (isExecuting)
            {
                if (CanRunAbility())
                    ExecuteAbility();
                else
                    CancelAbility();
            }
        }

        #region Coroutines
        public IEnumerator CalculateDelay()
        {
            StartDelay();

            while (isDelaying)
            {
                yield return null;
                DelayAbility();
            }
        }

        public IEnumerator CalculateStart()
        {
            isDelaying = true;
            controller.StartCoroutine(CalculateDelay());

            yield return new WaitForSeconds(DelayTime);
            isDelaying = false;
            StartAbility();
            if (ExecutionTime > 0)
            {
                isExecuting = true;
                controller.StartCoroutine(CalculateAbilityDuration());
            }
            else
            {
                ExecuteAbility();
                EndAbility();
            }
        }

        public IEnumerator CalculateAbilityDuration()
        {
            yield return new WaitForSeconds(ExecutionTime);
            EndAbility();
        }

        public IEnumerator CalculateCoolDown()
        {
            if (CanCoolDownAbility())
            {
                isCoolingDown = true;
                while (currentExecutionAmount < ExecutionAmount)
                {
                    yield return new WaitForSeconds(CoolDownTime);
                    currentExecutionAmount += 1;
                    OnAbilityCoolingDown?.Invoke();
                }

                isCoolingDown = false;
            }
        }
        #endregion

        #region Virtual bools 
        public virtual bool CanExecuteAbility()
        {
            return currentExecutionAmount > 0 && !isExecuting;
        }

        public virtual bool CanRunAbility()
        {
            return true;
        }

        public virtual bool CanCoolDownAbility()
        {
            return currentExecutionAmount <= 0 && !isCoolingDown;
        }
        #endregion

        public void UnbindEvents()
        {
            OnAbilityDelayStart = null;
            OnAbilityDelaying = null;
            OnAbilityStarted = null;
            OnAbilityExecuting = null;
            OnAbilityCoolingDown = null;
            OnAbilityCanceled = null;
            OnAbilityEnded = null;
        }
    }
}