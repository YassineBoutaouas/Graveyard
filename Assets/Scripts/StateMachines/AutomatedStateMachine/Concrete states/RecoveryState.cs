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
    public class RecoveryState : BaseState
    {
        public int RecoveryDuration = 21;
        public int RecoveryDelay = 2;
        [HideInInspector] public bool DelayReturn;

        public float RecoveryTime { get { return _recoveryTime; } }
        public float RecoveryDelayTime { get { return _delayTime; } }

        private float _recoveryTime = 10f;
        private float _delayTime = 2f;

        private EnemyCharacterHandler _enemyCharacterHandler;
        private ParticleSystem _recoveryParticles;
        private ParticleSystem _recoveryParticleNotes;
        private Quaternion _particleOrientation;

        float currentEnemies;
        float allEnemies;
        string longNoteAddition;

        public override void OnInitialize(CharacterHandler enemyController)
        {
            base.OnInitialize(enemyController);
            _enemyCharacterHandler = (EnemyCharacterHandler)characterController;
            _particleOrientation = ObjectPoolerManager.Instance.GetPool("FX_Recovery_Notes").Prefab.transform.rotation;

            _recoveryTime = RecoveryDuration.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);
            _delayTime = RecoveryDelay.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            _enemyCharacterHandler.Velocity = Vector3.zero;
            _enemyCharacterHandler.CanMove = false;
            _enemyCharacterHandler.CanRotate = false;

            _enemyCharacterHandler.SwitchPhysicsMode(CharacterHandler.PhysicsMode.kinematic);
            _enemyCharacterHandler.Velocity = Vector3.zero;

            _enemyCharacterHandler.AttachedCollider.isTrigger = true;
            _enemyCharacterHandler.CharacterAnimator.CrossFade("Scream_In", 0.1f);

            _recoveryParticles = ObjectPoolerManager.Instance.SpawnFromPool("FX_Recovery", _enemyCharacterHandler.transform.position + Vector3.up, Quaternion.identity).GetComponent<ParticleSystem>();
            _recoveryParticleNotes = ObjectPoolerManager.Instance.SpawnFromPool("FX_Recovery_Notes", _enemyCharacterHandler.transform.position + Vector3.up * 2, _particleOrientation).GetComponent<ParticleSystem>();

            _enemyCharacterHandler.Group.StartAggroTimer();
            _enemyCharacterHandler.EnemyHUD.EnableHUDElement("HealthBar", false);
            _enemyCharacterHandler.EnemyHUD.EnableHUDElement("Back", true);

             currentEnemies = _enemyCharacterHandler.Group.CurrentActiveEnemies;
             allEnemies = _enemyCharacterHandler.Group.Enemies.Count;
             longNoteAddition = "";

            //if this is the first raver
            if(currentEnemies +1 == allEnemies)
                longNoteAddition = "1";
            else if(currentEnemies == 0)
                longNoteAddition = "5";
            else
            {
                int randomTone = Random.Range(2, 5);
                longNoteAddition = randomTone.ToString();
            }

            _enemyCharacterHandler.CharacterSoundHandler.PlaySound("LongNote_0" + longNoteAddition);
            _enemyCharacterHandler.FaceHandler.SetEmotion(FaceSwap.Emotion.longScream);
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (DelayReturn)
            {
                _enemyCharacterHandler.CharacterAnimator.CrossFade("Recover", 0.1f);
                DelayReturn = false;
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();

            _enemyCharacterHandler.AttachedCollider.isTrigger = false;
            _enemyCharacterHandler.SwitchPhysicsMode(CharacterHandler.PhysicsMode.navmeshAgent);
            _enemyCharacterHandler.CanMove = true;
            _enemyCharacterHandler.CanRotate = true;

            _recoveryParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _recoveryParticleNotes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            _recoveryParticles.gameObject.SetActive(false);
            _recoveryParticleNotes.gameObject.SetActive(false);

            _enemyCharacterHandler.EnemyHUD.EnableHUDElement("HealthBar", true);
            _enemyCharacterHandler.EnemyHUD.EnableHUDElement("Back", false);

            _enemyCharacterHandler.CharacterSoundHandler.StopSound("LongNote_0" + longNoteAddition);

            if (_enemyCharacterHandler.Group.Active)
            {
                ObjectPoolerManager.Instance.SpawnFromPool("FX_Heal", _enemyCharacterHandler.transform.position, Quaternion.identity);
                _enemyCharacterHandler.CharacterSoundHandler.PlaySound("RecoverDone");
            }

            _enemyCharacterHandler.FaceHandler.SetEmotion(FaceSwap.Emotion.idle);
        }
    }
}