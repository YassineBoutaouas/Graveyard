using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Graveyard.AI;
using Graveyard.CharacterSystem.Enemy;

public class EnemyAttackHandler : MonoBehaviour
{
    public PushAttackState _pushAttackState;
    public BottleThrowingState _bottleThrowingState;

    [HideInInspector] public AttackState CurrentAttackState;

    [Header("Attack status")]
    [Space(5)]
    [ReadOnly] public bool IsAttacking;
    [ReadOnly] public bool IsCoolingDown;
    [ReadOnly] public bool Invincible;
    [ReadOnly] public bool CanAttack;

    private float _currentProbability;

    private void Awake()
    {
        GetAttack();
    }

    public void InitialzeStates(EnemyCharacterHandler enemy)
    {
        _pushAttackState.OnInitialize(enemy);
        _bottleThrowingState.OnInitialize(enemy);

        CurrentAttackState = _pushAttackState;
    }

    public AttackState GetAttack()
    {
        _currentProbability = UnityEngine.Random.Range(0f, 1f);

        CurrentAttackState = null;

        if (_currentProbability < _bottleThrowingState.Probability)
            CurrentAttackState = _bottleThrowingState;
        else
            CurrentAttackState = _pushAttackState;

        return CurrentAttackState;
    }

    public void OnAttack()
    {
        CurrentAttackState.OnAttack();
    }

    public void OnCounter()
    {

    }

    public void SetInvincible(bool invincible)
    {
        Invincible = invincible;
    }
}