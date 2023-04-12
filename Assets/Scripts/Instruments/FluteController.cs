using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Graveyard.CharacterSystem.Enemy;

namespace Graveyard.Combat
{
    [CreateAssetMenu(menuName = "Instruments/Flute", fileName = "New Flute")]
    public class FluteController : InstrumentController
    {
        [Header("Snapping values")]
        [Space(5)]
        public float SnapPadding = 0.9f;
        public float SnappingTime = 2f;
        //public float RotationTime = 0.5f;

        [Space(5)]
        public float StepSpeed = 5f;

        public override IEnumerator OnAttack()
        {
            if (CanAttack())
            {
                OnStartAttack(UnityEngine.Random.Range(0, AttackAnimations.Length - 1));
                attackHandler.PlayerController.CanMove = false;
                attackHandler.PlayerController.CanRotate = false;
                attackHandler.EnableCharacterMovement(false);

                if (attackHandler.GetValidTarget())
                {
                    OnValidAttackStart();

                    EnemyCharacterHandler enemyController = attackHandler.CurrentTarget.GetComponent<EnemyCharacterHandler>();
                    enemyController.AttackHandler.CanAttack = false;

                    #region play sound
                    string value = Enum.GetName(typeof(AudioSpectrumManager.BeatEvaluation), enemyController.PlayerController.CharacterAttackHandler.CurrentBeatEvaluation);
                    enemyController.CharacterSoundHandler.PlaySound(value + "Note");

                    if (value != "Bad")
                        enemyController.CharacterSoundHandler.PlaySound("StandardHit");

                    enemyController.CharacterSoundHandler.PlaySound(value + "Hit");
                    #endregion

                    float t = 0;
                    Vector3 startPosition = attackHandler.transform.position;
                    Vector3 targetPosition = GameManager.Instance.Settings.AutoTargeting ? Vector3.MoveTowards(attackHandler.CurrentTarget.transform.position, attackHandler.transform.position, SnapPadding) : attackHandler.transform.position + attackHandler.transform.forward * StepSpeed;

                    attackHandler.transform.rotation = Quaternion.LookRotation(Vector3.Scale(attackHandler.CurrentTarget.transform.position - attackHandler.transform.position, Vector3.forward + Vector3.right));

                    while (t < SnappingTime)
                    {
                        yield return null;
                        if (attackHandler.CurrentTarget == null) break;

                        t += Time.deltaTime;

                        if (GameManager.Instance.Settings.AutoTargeting)
                            attackHandler.transform.position = Vector3.Lerp(startPosition, targetPosition, InterpolationCurve.Evaluate(t / SnappingTime));
                        else if (Vector3.Distance(attackHandler.transform.position, targetPosition) > 1f)
                            attackHandler.PlayerController.Velocity = attackHandler.transform.forward * StepSpeed;
                    }

                    //attackHandler.PlayerController.RotateTowards(attackHandler.CurrentTarget.transform.position - attackHandler.transform.position);
                }

                attackHandler.PlayerController.Velocity = Vector3.zero;

                yield return new WaitForSeconds(Cooldown);

                attackHandler.EnableCharacterMovement(attackHandler.CanAttack);
                attackHandler.PlayerController.CanMove = !attackHandler.IsFinishing;
                attackHandler.PlayerController.CanRotate = !attackHandler.IsFinishing;
                attackHandler.IsAttacking = false;
                attackHandler.PlayerController.Velocity = attackHandler.CanAttack == false ? Vector3.zero : attackHandler.PlayerController.Velocity;

                OnEndAttack();
                attackHandler.CurrentTarget = null;
            }
        }

        public override bool CanAttack()
        {
            return base.CanAttack() && !attackHandler.IsAttacking && !attackHandler.IsFinishing && attackHandler.PlayerController.CanMove;
        }
    }
}