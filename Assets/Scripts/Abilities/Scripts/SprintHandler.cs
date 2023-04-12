using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;

namespace Graveyard.Abilities
{
    [CreateAssetMenu(fileName = "New Sprint", menuName = "Abilities / Ability / Sprint / Sprint")]
    public class SprintHandler : Ability
    {
        [Space(10)]
        [Header("Sprint Ability")]
        public float SpeedAddition = 0.6f;
        public float AccelerationTime = 0.2f;

        private float _startSpeed;
        private float _currentAccelerationTime;
        private IEnumerator _speedRoutine;

        private GameObject _sprintParticles;
        private ParticleSystem _sprintParticleSystem;

        public override void InitializeAbility(CharacterHandler c)
        {
            base.InitializeAbility(c);
            ExecutionTime = Mathf.Infinity;
            ExecutionAmount = int.MaxValue;
            CanInterrupt = true;

            _startSpeed = controller.SpeedMultiplier;

            _sprintParticles = ObjectPoolerManager.Instance.SpawnFromPool("Sprint_Dust", controller.transform.position, Quaternion.identity);
            _sprintParticles.SetActive(false);
            _sprintParticleSystem = _sprintParticles.GetComponent<ParticleSystem>();

            _sprintParticles.transform.SetParent(controller.transform.Find("AdditionalObjects"));
        }

        public override void StartAbility()
        {
            base.StartAbility();
            if (_speedRoutine != null)
                controller.StopCoroutine(_speedRoutine);

            _speedRoutine = null;
            _speedRoutine = InterpolateSpeedMultiplier(_startSpeed + SpeedAddition);

            controller.StartCoroutine(_speedRoutine);

            _sprintParticles.SetActive(true);
            _sprintParticleSystem.Play(true);
        }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();

            if (!controller.IsGrounded && _sprintParticleSystem.isEmitting)
                _sprintParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            else if (controller.IsGrounded && !_sprintParticleSystem.isEmitting)
                _sprintParticleSystem.Play(true);
        }

        private IEnumerator InterpolateSpeedMultiplier(float speed)
        {
            while (_currentAccelerationTime < AccelerationTime)
            {
                yield return null;
                controller.SpeedMultiplier = Mathf.Lerp(controller.SpeedMultiplier, speed, _currentAccelerationTime / AccelerationTime);
                _currentAccelerationTime += Time.deltaTime;
            }

            _currentAccelerationTime = 0f;
            controller.SpeedMultiplier = speed;
        }

        public override void CancelAbility()
        {
            base.CancelAbility();
            StopSprint();
        }

        public override void EndAbility()
        {
            base.EndAbility();
            StopSprint();
        }

        public void StopSprint()
        {
            if (_speedRoutine != null)
                controller.StopCoroutine(_speedRoutine);

            _speedRoutine = null;
            _speedRoutine = InterpolateSpeedMultiplier(_startSpeed);

            if (controller.gameObject.activeInHierarchy)
                controller.StartCoroutine(_speedRoutine);

            _sprintParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}