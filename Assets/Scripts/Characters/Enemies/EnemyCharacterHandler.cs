using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Graveyard.CharacterSystem.Player;
using Graveyard.AI;

namespace Graveyard.CharacterSystem.Enemy
{
    public class EnemyCharacterHandler : CharacterHandler
    {
        #region Properties
        public StateMachine EnemyStateMachine { get { return _stateMachine; } }
        public EnemyAttackHandler AttackHandler { get { return _attackHandler; } }
        public EnemyGroupController Group { get { return _enemyGroup; } }
        public PlayerCharacterController PlayerController { get { return _playerCharacterController; } }
        public Vector3 GroupPosition { get { return _groupPosition; } }
        public HUDManager EnemyHUD { get { return _enemyHUD; } }
        public FaceSwap FaceHandler { get { return _faceHandler; } }
        #endregion

        #region Public fields
        [Header("Group ID")]
        [Space(10)]
        public int GroupID;

        [ReadOnly] public string CurrentStateName;
        [ReadOnly] public string LastStateName;

        [Header("States")]
        [Space(10)]
        public IdleState _idleState;
        public ChasingState _chasingState;

        public ReturnState _returnState;

        public HurtState _hurtState;
        public StunnedState _stunnedState;

        public BlockingState _blockingState;

        public RecoveryState _recoveryState;

        public ConvincedState _convincedState;
        #endregion

        #region Non-Public fields
        private PlayerCharacterController _playerCharacterController;
        private EnemyAttackHandler _attackHandler;
        private CapsuleCollider _capsuleCollider;
        private StateMachine _stateMachine;
        private EnemyGroupController _enemyGroup;
        private Vector3 _groupPosition;
        private HUDManager _enemyHUD;
        private FaceSwap _faceHandler;
        #endregion

        public override void GetCollider<T>()
        {
            base.GetCollider<CapsuleCollider>();
            _capsuleCollider = (CapsuleCollider)AttachedCollider;
        }

        protected override void Awake()
        {
            base.Awake();
            _enemyHUD = GetComponentInChildren<HUDManager>(true);
            _attackHandler = GetComponent<EnemyAttackHandler>();
            _faceHandler = GetComponentInChildren<FaceSwap>();
        }

        protected override void Start()
        {
            base.Start();
            _playerCharacterController = GameManager.Instance.PlayerController;
            CurrentOrientationMethod = () => RotateTowardsMovement();

            _stateMachine = new StateMachine();

            _enemyGroup = EnemyManager.Instance.GetEnemyGroup(GroupID);
            _groupPosition = _enemyGroup.transform.position;

            _idleState.OnInitialize(this);
            _chasingState.OnInitialize(this);
            
            _returnState.OnInitialize(this);

            _hurtState.OnInitialize(this);
            _blockingState.OnInitialize(this);

            _stunnedState.OnInitialize(this);
            _recoveryState.OnInitialize(this);

            _convincedState.OnInitialize(this);

            _attackHandler.InitialzeStates(this);

            _stateMachine.AddAnyTransition(_convincedState, () => Group.Active == false);
            _stateMachine.AddAnyTransition(_recoveryState, () => CharacterHealthHandler.IsRecovering);
            _stateMachine.AddAnyTransition(_hurtState, () => CharacterHealthHandler.IsDamaged);

            _stateMachine.AddAnyTransition(_blockingState, () => _blockingState.IsBlocking); //

            _stateMachine.AddAnyTransition(_stunnedState, () => CharacterHealthHandler.IsStunned);

            _stateMachine.AddTransition(_recoveryState, _chasingState, () => !CharacterHealthHandler.IsRecovering);

            _stateMachine.AddTransition(_idleState, _chasingState, () => _enemyGroup.EngagedInFight);

            _stateMachine.AddTransition(_stunnedState, _chasingState, () => !CharacterHealthHandler.IsStunned);
            _stateMachine.AddTransition(_hurtState, _chasingState, () => !CharacterHealthHandler.IsDamaged);

            _stateMachine.AddTransition(_returnState, _idleState, () => _returnState.HasReturned);
            _stateMachine.AddTransition(_returnState, _chasingState, () => _enemyGroup.EngagedInFight);

            _stateMachine.AddTransition(_blockingState, _chasingState, () => !_blockingState.IsBlocking); //

            _stateMachine.AddTransition(_chasingState, _returnState, () => !_enemyGroup.EngagedInFight);

            //Add all attacks
            _stateMachine.AddTransition(_chasingState, AttackHandler._pushAttackState, () => AttackHandler.IsAttacking && AttackHandler.CurrentAttackState == AttackHandler._pushAttackState);
            _stateMachine.AddTransition(_chasingState, AttackHandler._bottleThrowingState, () => AttackHandler.IsAttacking && AttackHandler.CurrentAttackState == AttackHandler._bottleThrowingState);

            _stateMachine.AddTransition(AttackHandler._pushAttackState, _chasingState, () => !AttackHandler.IsAttacking);
            _stateMachine.AddTransition(AttackHandler._bottleThrowingState, _chasingState, () => !AttackHandler.IsAttacking);


            _stateMachine.SetState(_idleState);

            OnCharacterSetup();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        protected override void Update()
        {
            base.Update();
            _stateMachine.Update();
            CurrentStateName = _stateMachine.CurrentStateName;
            LastStateName = _stateMachine.LastStateName;
        }

        public void RotateTowardsMovement()
        {
            if (CharacterNavmeshAgent.enabled)
                if (CharacterNavmeshAgent.velocity.magnitude > 2)
                    SmoothRotateTowards(CharacterNavmeshAgent.velocity);
        }

        public void RotateTowardsPlayer()
        {
            SmoothRotateTowards(GameManager.Instance.PlayerController.transform.position - transform.position);
        }

        protected override bool UseGravity()
        {
            return false;
        }
    }
}