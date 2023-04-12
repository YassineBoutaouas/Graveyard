using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Enemy;

public class HurtStateAnimationBehaviour : StateMachineBehaviour
{
    private EnemyCharacterHandler _enemyCharacterHandler;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        animator.TryGetComponent(out _enemyCharacterHandler);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (stateInfo.normalizedTime > 0.9)
            _enemyCharacterHandler.CharacterHealthHandler.SetIsDamaged(false); //Eliminate
    }
}
