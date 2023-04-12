using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Enemy;
using Graveyard.AI;

public class IdleAnimationBehaviour : StateMachineBehaviour
{
    private EnemyCharacterHandler _enemyCharacterHandler;
    private string _currentDance;
    private bool _hasTransitioned;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        animator.gameObject.TryGetComponent(out _enemyCharacterHandler);

        _hasTransitioned = false;
        _currentDance = _enemyCharacterHandler._idleState.IdleAnimations[Random.Range(0, _enemyCharacterHandler._idleState.IdleAnimations.Length)];
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (stateInfo.normalizedTime > 0.8 && !_hasTransitioned)
        {
            _hasTransitioned = true;
            animator.CrossFade(_currentDance, 0.25f);
        }
    }
}