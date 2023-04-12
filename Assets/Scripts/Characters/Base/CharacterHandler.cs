using System;
using UnityEngine;
using UnityEngine.AI;
using Graveyard.CharacterSystem.Detections;
using Graveyard.CharacterSystem.Stats;
using Graveyard.Health;

namespace Graveyard.CharacterSystem
{
    [RequireComponent(typeof(Animator))]
    public class CharacterHandler : PhysicsObject, IReset
    {
        #region Events and delegates
        public event Action OnCharacterSetUp;
        public void OnCharacterSetup() { OnCharacterSetUp?.Invoke(); }
        public delegate void OrientationMethod();
        public OrientationMethod CurrentOrientationMethod;
        public event Action<PhysicsMode, PhysicsMode> OnPhysicsModeChange;
        #endregion

        #region Properties
        public SoundHandler CharacterSoundHandler { get { return _soundHandler; } }
        public Animator CharacterAnimator { get { return _animator; } }
        public NavMeshAgent CharacterNavmeshAgent { get { return _navMeshAgent; } }
        public HealthHandler CharacterHealthHandler { get { return _healthHandler; } }
        public DetectionObject GroundDetection { get { return groundDetection; } }

        public enum PhysicsMode { dynamic = 0, kinematic = 1, rootmotion = 2, navmeshAgent = 3 };
        public bool ShellCollision { get { return _shellCollision; } set { _shellCollision = value; } }
        public bool IsGrounded { get { return _isGrounded; } set { _isGrounded = value; } }

        public float SpeedMultiplier { get { return _speedMultiplier; } set { _speedMultiplier = value; } }
        public Vector3 DesiredTargetDirection { get { return _desiredTargetDirection; } set { _desiredTargetDirection = value; } }
        public Vector3 AddedVelocity { get { return _addedVelocity; } set { _addedVelocity = value; } }
        public float CurrentProjectionForce { get { return _currentProjectionForce; } set { _currentProjectionForce = value; } }
        #endregion

        #region const fields
        public const string Ground_Detection_Name = "Ground";
        public const string Shell_Detection_Name = "Shell";
        public const string Bottom_Step_Detection_Name = "BottomStep";
        public const string Top_Step_Detection_Name = "TopStep";
        #endregion

        #region Public fields
        [Space(10)]
        public PhysicsMode CurrentPhysicsMode;

        [Header("Character stats")]
        [Space(5)]
        public CharacterStats Stats;

        [Header("Constraints")]
        [ReadOnly] public bool CanMove;
        [ReadOnly] public bool CanRotate;

        [Header("Stair Climbing")]
        [Space(10)]
        [Range(0f, 1f)]
        public float MaxStepHeight = 0.3f;
        public float SteppingSpeed = 0.1f;
        #endregion

        #region Non-Public fields
        protected float startSteppingSpeed;
        protected Vector3 capsuleBottomSphere;
        protected Vector3 capsuleTopSphere;
        protected bool isClimbingStair;
        
        protected RadialDetection topStepCheck;
        protected RadialDetection bottomStepCheck;        
        protected DetectionObject shellDetection;
        protected DetectionObject groundDetection;
        
        private Animator _animator;
        private NavMeshAgent _navMeshAgent;
        private HealthHandler _healthHandler;
        private SoundHandler _soundHandler;

        private bool _isGrounded;
        private bool _shellCollision;
        private RaycastHit[] _groundHits;
        
        private float _speedMultiplier;
        private Vector3 _desiredTargetDirection;
        private Vector3 _addedVelocity;
        private float _currentProjectionForce;
        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
            GetComponents();

            AttachedRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            AttachedRigidbody.mass = Stats.Weight;
            AttachedRigidbody.drag = Stats.Drag;
            _speedMultiplier = Stats.SpeedMultiplier;
            startSteppingSpeed = SteppingSpeed;

            shellDetection = Detections.GetDetectionObject(Shell_Detection_Name);
            groundDetection = Detections.GetDetectionObject(Ground_Detection_Name);
            Detections.InitializeRadialDetection(Top_Step_Detection_Name, Vector3.up * MaxStepHeight, out topStepCheck);
            Detections.InitializeRadialDetection(Bottom_Step_Detection_Name, out bottomStepCheck);

            SwitchPhysicsMode(CurrentPhysicsMode);
        }

        protected virtual void GetComponents()
        {
            _animator = GetComponent<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _healthHandler = GetComponent<HealthHandler>();
            _soundHandler = GetComponent<SoundHandler>();
        }

        protected override void Start()
        {
            base.Start();
            CanMove = true;
            CanRotate = true;
        }

        protected override void FixedUpdate()
        {
            if (_animator.applyRootMotion) return;
            base.FixedUpdate();
        }

        protected override void UpdatePhysicsDetections()
        {
            base.UpdatePhysicsDetections();

            #region Capsule cast update
            capsuleBottomSphere = transform.position + (Vector3.up * AttachedCollider.bounds.size.z);
            capsuleTopSphere = (transform.position + (Vector3.up * AttachedCollider.bounds.size.y)) - (Vector3.up * AttachedCollider.bounds.size.z);
            #endregion

            #region Ground Detection
            _groundHits = Physics.SphereCastAll(groundDetection.detectionObject.transform.position + Vector3.up * groundDetection.radius, groundDetection.radius, Vector3.down, groundDetection.radius, groundDetection.layerMask, groundDetection.TriggerInteraction);

            if (_groundHits.Length == 0)
                _isGrounded = false;
            else
            {
                _isGrounded = CurrentNormal.y > MinVerticalGroundNormal;

                if (!_isGrounded)
                    SlopeLimitMethod();

                //CanMove = _isGrounded;
            }

            groundDetection.CheckValueChange(_isGrounded);

            _shellCollision = _isGrounded && shellDetection.detectedCollisions.Length > 0;
            #endregion
        }

        public virtual void SlopeLimitMethod()
        {
            //CanMove = !_shellCollision;
            Debug.Log("Limit hit, slope too steep!");
        }

        public virtual void SwitchPhysicsMode(PhysicsMode physicsMode)
        {
            PhysicsMode lastPhysicsMode = CurrentPhysicsMode;
            CurrentPhysicsMode = physicsMode;
            OnPhysicsModeChange?.Invoke(lastPhysicsMode, CurrentPhysicsMode);

            switch (CurrentPhysicsMode)
            {
                case PhysicsMode.dynamic:
                    AttachedRigidbody.isKinematic = false;
                    AttachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    AttachedRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                    AttachedRigidbody.interpolation = RigidbodyInterpolation.None;

                    SteppingSpeed = startSteppingSpeed + 1;

                    _animator.applyRootMotion = false;

                    if (_navMeshAgent != null)
                        _navMeshAgent.enabled = false;
                    break;

                case PhysicsMode.kinematic:
                    AttachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    AttachedRigidbody.isKinematic = true;
                    AttachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                    ShellRadius = StartShellRadius;

                    _animator.applyRootMotion = false;

                    Velocity = AttachedRigidbody.velocity;
                    Velocity += GroundNormal * ShellRadius;

                    SteppingSpeed = startSteppingSpeed;

                    if (_isGrounded)
                        AttachedRigidbody.position += transform.up * ShellRadius;

                    if (_navMeshAgent != null)
                        _navMeshAgent.enabled = false;
                    break;

                case PhysicsMode.rootmotion:
                    _animator.applyRootMotion = true;
                    AttachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    AttachedRigidbody.isKinematic = true;

                    if (_navMeshAgent != null)
                        _navMeshAgent.enabled = false;
                    break;

                case PhysicsMode.navmeshAgent:
                    Velocity = Vector3.zero;

                    if (_navMeshAgent != null)
                        _navMeshAgent.enabled = true;
                    break;

                default:
                    break;
            }
        }

        protected override void ComputeOrientation()
        {
            base.ComputeOrientation();
            ComputeOrientation(CurrentOrientationMethod);
        }

        protected virtual void ComputeOrientation(OrientationMethod orientationMethod)
        {
            if (orientationMethod == null) return;
            orientationMethod();
        }

        public virtual void SmoothRotateTowards(Vector3 direction)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.Scale(direction.normalized, Vector3.right + Vector3.forward)), Stats.TurningSpeed * Time.deltaTime);
        }

        public void RotateTowards(Vector3 direction)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.Scale(direction.normalized, Vector3.right + Vector3.forward));
        }

        #region Obstacle handling
        protected virtual void HandleStairs()
        {
            if (bottomStepCheck.IsObjectDetected)
            {
                if (!topStepCheck.IsObjectDetected)
                {
                    if (Mathf.Abs(Vector3.Cross(bottomStepCheck.hitInfo.normal, transform.right).y) > MinVerticalGroundNormal)
                    {
                        isClimbingStair = true;
                        Velocity += transform.up * SteppingSpeed;
                    }
                }
            }

            isClimbingStair = false;
        }

        public override float ModifyProjectionForce()
        {
            _currentProjectionForce = 1 / _speedMultiplier * Time.timeScale;
            return _currentProjectionForce;
        }

        /// <summary>
        /// This method projects the velocity on a certain plane.
        /// </summary>
        /// <param name="movement"></param>
        /// <returns></returns>
        public Vector3 ProjectForceOnWall(Vector3 movement)
        {
            Vector3 dir = movement.normalized;
            Physics.CapsuleCast(capsuleBottomSphere, capsuleTopSphere, AttachedCollider.bounds.size.z / 2 + StartShellRadius, dir, out RaycastHit wallHit, (AttachedCollider.bounds.size.z / 2 + StartShellRadius) * _speedMultiplier, ~0, QueryTriggerInteraction.Ignore);

            return ProjectForceOnPlane(movement, wallHit.normal);
        }
        #endregion

        #region Virtual Checks
        protected override bool UseGravity()
        {
            return base.UseGravity();
        }

        protected virtual bool CanUpdateMovement()
        {
            return CanMove && CharacterAnimator.applyRootMotion == false;
        }

        protected virtual bool CanUpdateRotation()
        {
            return CanRotate && CharacterAnimator.applyRootMotion == false;
        }
        #endregion

        #region Getter Methods
        public void GetAnimator()
        {
            _animator = GetComponent<Animator>();
        }
        #endregion

        #region Loading
        public virtual void OnSave() { }
        
        public virtual void OnReload() { }
        #endregion

        protected virtual void OnDestroy() { }

        #region Debugging
        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
#if UNITY_EDITOR
            if (!Detections.DebugMode) return;

            if (AttachedCollider)
            {
                Gizmos.DrawWireSphere(capsuleBottomSphere, AttachedCollider.bounds.size.z / 2);
                Gizmos.DrawWireSphere(capsuleTopSphere, AttachedCollider.bounds.size.z / 2);
            }
#endif
        }
        #endregion
    }
}