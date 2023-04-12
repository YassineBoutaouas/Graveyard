using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Enemy;

public class AttackStateAnimationBehaviour : StateMachineBehaviour
{
    private EnemyCharacterHandler _enemyCharacterHandler;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        //_enemyCharacterHandler = animator.GetComponent<EnemyCharacterHandler>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        //if(stateInfo.normalizedTime > 0.85f && _enemyCharacterHandler.AttackHandler.IsAttacking)
        //    _enemyCharacterHandler.AttackHandler.IsAttacking = false;
    }
}