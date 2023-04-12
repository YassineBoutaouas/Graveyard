using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Detections;

namespace Graveyard.CharacterSystem
{

    /// <summary>
    /// This class provides functionality for any complex physics body in the game
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class PhysicsObject : MonoBehaviour
    {
        #region Properties
        public Rigidbody AttachedRigidbody { get { return _rigidbody; } }        
        public Collider AttachedCollider { get { return _collider; } }
        
        public Vector3 Velocity { get { return _velocity; } set { _velocity = value; } }
        public Vector3 DeltaPosition { get { return _deltaPosition; } }
        public Vector3 ProjectedMoveVector { get { return _projectedMoveVector; } set { _projectedMoveVector = value; } }
        public float StartShellRadius { get { return _startShellRadius; } }
        public Vector3 GroundNormal { get { return _groundNormal; } }
        public Vector3 CurrentNormal { get { return _currentNormal; } }
        public Vector3 ProjectionForce { get { return _projectionForce; } }
        #endregion

        #region const values
        public const float MinMoveDistance = 0.001f;
        #endregion

        #region Public fields
        [Header("Collision Variables")]
        [Space(5)]
        [Header("Physics")]
        [Space(5)]
        public float ShellRadius = 0.02f;
        public float MinVerticalGroundNormal = 0.65f;
        public RaycastHit LastHit;
        public float FallMultiplier = 4f;
        [Range(0f, 1f)]
        public float GravityMultiplier = 1f;
        
        public DetectionHandler Detections;
        #endregion

        #region Non-Public variables
        protected List<RaycastHit> _hitList = new List<RaycastHit>(16);
        
        private Rigidbody _rigidbody;
        private Collider _collider;
        private Vector3 _velocity;
        private Vector3 _deltaPosition;
        private Vector3 _projectedMoveVector;
        private float _startShellRadius;
        private Vector3 _groundNormal;
        private Vector3 _currentNormal;
        private Vector3 _projectionForce;
        private RaycastHit[] _hitArray = new RaycastHit[16];
        #endregion

        #region Built in methods
        protected virtual void OnEnable()
        {
            GetRigidBody();
            GetCollider<Collider>();

            _rigidbody.useGravity = false;
            _startShellRadius = ShellRadius;
        }

        protected virtual void Awake() { }

        protected virtual void Start() { }

        protected virtual void Update()
        {
            ComputeVelocity();
            ComputeOrientation();
        }

        protected virtual void FixedUpdate()
        {
            UpdatePhysicsDetections();
            UpdateMovement();
            _rigidbody.velocity = _velocity;
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// This method is used to calculate velocity.
        /// </summary>
        protected virtual void ComputeVelocity() { }

        /// <summary>
        /// This method is used to reorient the physicsobject if needed
        /// </summary>
        protected virtual void ComputeOrientation() { }

        /// <summary>
        /// This method iterates through detection objects and assigns collider arrays or ray casts for physics detections
        /// </summary>
        protected virtual void UpdatePhysicsDetections()
        {
            foreach (DetectionObject detectionObject in Detections.DetectionObjects)
            {
                switch (detectionObject.detectionType)
                {
                    case DetectionObject.DetectionType.overlapSphere:
                        detectionObject.detectedCollisions = Physics.OverlapSphere(detectionObject.detectionObject.transform.position, detectionObject.radius, detectionObject.layerMask, detectionObject.TriggerInteraction);
                        break;
                    case DetectionObject.DetectionType.overlapCube:
                        detectionObject.detectedCollisions = Physics.OverlapBox(detectionObject.detectionObject.transform.position, detectionObject.detectionBounds / 2, Quaternion.Euler(detectionObject.detectionObject.transform.forward), detectionObject.layerMask, detectionObject.TriggerInteraction);
                        break;
                    case DetectionObject.DetectionType.rayCast:
                        detectionObject.IsObjectDetected = Physics.Raycast(detectionObject.detectionObject.transform.position, detectionObject.castDirection, out detectionObject.hitInfo, detectionObject.length, detectionObject.layerMask, detectionObject.TriggerInteraction);
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        #region Virtual bools
        protected virtual bool UseGravity()
        {
            return true;
        }

        protected virtual bool UsePlaneProjection()
        {
            return true;
        }

        protected virtual bool UseProjectionForce()
        {
            return true;
        }
        #endregion

        #region Movement methods
        private void UpdateMovement()
        {
            if (UseGravity())
                AddVelocity(_rigidbody.mass * FallMultiplier * GravityMultiplier * Physics.gravity.y * Vector3.up);
            else if (_velocity.y < 0)
                _velocity.y = 0;

            _deltaPosition = _velocity * Time.fixedDeltaTime;
            _projectedMoveVector = _deltaPosition;

            if (UsePlaneProjection())
                _projectedMoveVector = Vector3.ProjectOnPlane(_projectedMoveVector, _groundNormal);

            if (_rigidbody.isKinematic)
            {
                CollisionDetection(_projectedMoveVector, false, out float horizontalDistance);
                _rigidbody.position += _projectedMoveVector.normalized * horizontalDistance;

                _projectedMoveVector = Vector3.up * _deltaPosition.y;

                CollisionDetection(_projectedMoveVector, true, out float verticalDistance);
                _rigidbody.position += _projectedMoveVector.normalized * verticalDistance;
            }
            else
                CollisionDetection(_projectedMoveVector, false, out float _);
        }

        private void CollisionDetection(Vector3 moveVector, bool verticalMovement, out float distance)
        {
            distance = moveVector.magnitude;
            moveVector *= Time.fixedDeltaTime;

            _groundNormal = Vector3.up;

            if (distance > MinMoveDistance)
            {
                //Check for collisions
                _hitArray = _rigidbody.SweepTestAll(moveVector, distance + ShellRadius, QueryTriggerInteraction.Ignore);

                _hitList.Clear();

                for (int i = 0; i < _hitArray.Length; i++)
                {
                    LastHit = _hitArray[i];

                    _hitList.Add(_hitArray[i]);

                    //Getting for each hit the normal vector
                    _currentNormal = _hitList[i].normal;

                    if (_currentNormal.y > MinVerticalGroundNormal)
                    {
                        if (verticalMovement)
                        {
                            _groundNormal = _currentNormal;
                            _currentNormal.x = 0;
                            _currentNormal.z = 0;
                        }
                    }

                    float projection = Vector3.Dot(_velocity, _currentNormal);

                    if (projection < 0)
                    {
                        _projectionForce = (-projection * ModifyProjectionForce()) * _currentNormal;
                        AddRawVelocity(_projectionForce);
                    }

                    float modifiedDistance = _hitList[i].distance - ShellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }
        }
        #endregion

        #region Velocity Forces
        public void AddForce(Vector3 force)
        {
            _velocity += (force / _rigidbody.mass) / _rigidbody.angularDrag;
        }

        public void AddRawVelocity(Vector3 force)
        {
            _velocity += force;
        }

        public void AddVelocity(Vector3 force)
        {
            _velocity += force * Time.fixedDeltaTime;
        }

        public Vector3 ProjectForceOnPlane(Vector3 force, Vector3 normal)
        {
            return Vector3.ProjectOnPlane(force, normal);
        }

        public void AddVelocityAlongGround(Vector3 force)
        {
            AddVelocity(Vector3.ProjectOnPlane(force, _groundNormal));
        }

        public void AddForceAlongGround(Vector3 force)
        {
            AddForce(Vector3.ProjectOnPlane(force, _groundNormal));
        }

        public virtual float ModifyProjectionForce()
        {
            return 1;
        }
        #endregion

        #region Getter Methods
        public virtual void GetCollider<T>() where T : Collider
        {
            _collider = GetComponent<T>();
        }

        public void GetRigidBody()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
        #endregion

        #region Debugging
        public virtual void OnDrawGizmosSelected()
        {
            if (!Detections.DebugMode) return;

#if UNITY_EDITOR
            foreach (DetectionObject d in Detections.DetectionObjects)
            {
                switch (d.detectionType)
                {
                    case DetectionObject.DetectionType.overlapSphere:
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(d.detectionObject.transform.position, d.radius);
                        break;
                    case DetectionObject.DetectionType.overlapCube:
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireCube(d.detectionObject.transform.position, d.detectionBounds);
                        break;
                    case DetectionObject.DetectionType.rayCast:
                        Gizmos.color = Color.magenta;
                        Debug.DrawRay(d.detectionObject.transform.position, d.castDirection * d.length, Gizmos.color);
                        break;
                    default:
                        break;
                }
            }
#endif
        }
        #endregion
    }
}