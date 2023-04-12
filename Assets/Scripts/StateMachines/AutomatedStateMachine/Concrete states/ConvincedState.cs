using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Enemy;
using Graveyard.CharacterSystem.Player;

namespace Graveyard.AI
{
    [System.Serializable]
    public class ConvincedState : BaseState
    {
        public int DelayTimeInBeats = 4;
        public int FocusTimeInBeats = 4;

        //public float ReturnSpeed = 2f;
        //public float Acceleration = 2f;
        public bool ReachedTarget;

        private float _delayTime;
        private float _focusTime;
        private EnemyCharacterHandler _enemyController;
        private NavMeshAgent _navMeshAgent;
        private Vector3 _returnPosition;

        private List<Renderer> _meshRenderers = new List<Renderer>();

        public override void OnInitialize(CharacterHandler controller)
        {
            base.OnInitialize(controller);
            _enemyController = (EnemyCharacterHandler)characterController;
            _navMeshAgent = _enemyController.CharacterNavmeshAgent;

            _delayTime = DelayTimeInBeats.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);
            _focusTime = FocusTimeInBeats.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);

            _meshRenderers = _enemyController.transform.GetComponentsInChildren<Renderer>().ToList();
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            _returnPosition = _enemyController.Group.ChoirPositions[_enemyController.Group.Enemies.IndexOf(_enemyController)];

            _enemyController.CharacterHealthHandler.StopAllCoroutines();
            _enemyController.Group.StopAllCoroutines();

            _enemyController.EnemyHUD.GetHUDElement("LocalEnemyHUD").gameObject.SetActive(false);

            _enemyController.StartCoroutine(ExecuteReturn());

            _enemyController.EnemyHUD.EnableHUDElement("HealthBar", false);
            _enemyController.FaceHandler.SetEmotion(FaceSwap.Emotion.choir);

            if (_enemyController._hurtState.IsLastHit)
            {
                _enemyController.CharacterAnimator.CrossFade("Death", 0f);
                _enemyController.RotateTowardsPlayer();
            }

            if (_enemyController.CharacterNavmeshAgent.enabled)
                _enemyController.CharacterNavmeshAgent.isStopped = true;
        }

        private IEnumerator ExecuteReturn()
        {
            string animationName = _enemyController._hurtState.IsLastHit ? "StandUp" : "Focus";
            if (animationName != "StandUp") _enemyController.CharacterAnimator.CrossFade("DizzyIdle", 0.1f);

            yield return new WaitForSeconds(_delayTime);
            _enemyController.CharacterAnimator.CrossFade(animationName, 0f);
            
            yield return new WaitForSeconds(_focusTime);

            yield return new WaitUntil(() => AudioSpectrumManager.Instance.IsOnBeat);

            _enemyController.SwitchPhysicsMode(CharacterHandler.PhysicsMode.navmeshAgent);
            _enemyController.CharacterNavmeshAgent.isStopped = true;

            ObjectPoolerManager.Instance.SpawnFromPool("FX_Vanish", _enemyController.transform.position, Quaternion.identity);

            foreach (Renderer renderer in _meshRenderers)
                renderer.enabled = false;

            yield return new WaitForSeconds(_focusTime);

            yield return new WaitForSeconds((1.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute)) * _enemyController.Group.Enemies.IndexOf(_enemyController));

            NavMesh.SamplePosition(_returnPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas);
            _enemyController.transform.position = hit.position;

            ObjectPoolerManager.Instance.SpawnFromPool("FX_Appear", _enemyController.transform.position, Quaternion.identity);

            foreach (Renderer renderer in _meshRenderers)
                renderer.enabled = true;

            _enemyController.transform.Find("AdditionalObjects").Find("FX_Choir_Halo").gameObject.SetActive(true);

            _enemyController.CharacterAnimator.CrossFade("Singing", 0f);

            _enemyController.RotateTowards(_enemyController.Group.transform.forward);
            _enemyController._hurtState.IsLastHit = false;
            ReachedTarget = true;
            _enemyController.Velocity = Vector3.zero;
        }
    }
}