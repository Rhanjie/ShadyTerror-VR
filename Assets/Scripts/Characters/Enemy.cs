using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    public class Enemy : Character
    {
        [SerializeField] protected AudioClip laugh;
        [SerializeField] protected AudioClip scream;
        [SerializeField] protected Transform transformToRotate;
        [SerializeField] protected Transform waypointsParent;
        [SerializeField] protected Animator animator;
        
        [Header("Debug")]
        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;
        [SerializeField] private float mass = 3.0f; // defines the character mass
        
        protected float visionLength;
        protected bool foundPlayer;
        protected float attackDuration;

        protected CharacterController _characterController;
        protected Coroutine _attackCoroutineObject;
        private GameObject player;
        protected Vector2 targetToReach = Vector2.zero;
        protected Vector3 _impact;
        protected float acceleration = 2f;
        private Func<Vector3> randomPositionMethod;

        protected List<Vector2> _waypoints;
        protected int _currentWaypointIndex = 0;
        protected float _currentSpeed = 0f;
        
        protected const float MaxDistanceToReachDestination = 1.5f;
        
        protected static readonly int DissolvePowerID = Shader.PropertyToID("_DissolvePower");
        protected static readonly int VelocityHash = Animator.StringToHash("Velocity");
        protected static readonly int AttackHash = Animator.StringToHash("Attack");

        protected override void Start()
        {
            base.Start();

            walkSpeed = Random.Range(0.4f, 1.5f);
            runSpeed = Random.Range(3, 5);
            visionLength = 10f;

            _waypoints = GetWaypoints();
            attackDuration = GetAttackDuration();
            
            if (transformToRotate == null)
                transformToRotate = transform;

            _characterController = GetComponent<CharacterController>();

            var target = GameObject.FindWithTag("Player");
            if (target == null)
            {
                throw new NotSupportedException("Not found player in the scene!");
            }
            
            player = target;
        }
        
        protected override void Update()
        {
            if (_impact.magnitude > 0.2)
                _characterController.Move(_impact * Time.deltaTime);
            
            _impact = Vector3.Lerp(_impact, Vector3.zero, 5*Time.deltaTime);
            
            FaceToTarget();
            
            base.Update();
        }

        private List<Vector2> GetWaypoints()
        {
            var waypoints = new List<Vector2>();
            
            foreach (var waypoint in waypointsParent.Cast<Transform>())
            {
                var position = ConvertToVector2(waypoint.position);
                
                waypoints.Add(position);
            }

            return waypoints;
        }

        private float GetAttackDuration()
        {
            const float additionalOffset = 0.2f;
            
            var attackClip = animator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Zombie_Attack");
            if (attackClip == null)
                throw new KeyNotFoundException("Not found 'Zombie_Attack' animation!");
                
            return attackClip.length + additionalOffset;
        }

        public override void UpdateCustomBehaviour()
        {
            
        }

        protected bool TryToFindTarget()
        {
            var distance = Vector3.Distance(player.transform.position, transform.position);
            
            //TODO: THROW RAYCAST
            
            return distance <= visionLength;
        }

        protected void UpdateAttackRoutine()
        {
            if (!foundPlayer)
            {
                audioManager.PlaySoundWithRandomPitch("iSeeYouVoice", 0.9f, 1.1f);
                
                foundPlayer = true;
            }

            targetToReach = ConvertToVector2(player.transform.position);
        }

        protected void UpdateWalkRoutine()
        {
            var position2D = ConvertToVector2(transform.position);
            var distance = Vector2.Distance(targetToReach, position2D);
            if (distance > MaxDistanceToReachDestination)
            {
                var direction = GetDirectionToTarget();
                
                GoToDirection(direction);
            }

            else HandleDestinationReached();
        }
        
        protected void ServeGravity()
        {
            var verticalVelocity = 0f;
            if (!_characterController.isGrounded)
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            
            _characterController.Move(new Vector3(0f, verticalVelocity, 0f));
        }

        protected void GoToDirection(Vector2 direction)
        {
            var maxSpeed = foundPlayer ? runSpeed : walkSpeed;

            _currentSpeed += acceleration * Time.deltaTime;
            if (_currentSpeed > maxSpeed)
                _currentSpeed = maxSpeed;
            
            var positionMove = direction * (_currentSpeed * Time.deltaTime);
            _characterController.Move(new Vector3(positionMove.x, 0, positionMove.y));

            animator.SetFloat(VelocityHash, _currentSpeed);
        }
        
        private void HandleDestinationReached()
        {
            animator.SetFloat(VelocityHash, 0);
            
            if (foundPlayer)
            {
                Attack();
            }
            
            else _currentWaypointIndex = GetNextWaypointIndex(_currentWaypointIndex);
        }

        private int GetNextWaypointIndex(int previousIndex)
        {
            return (previousIndex + 1) % _waypoints.Count;
        }

        private static Vector2 ConvertToVector2(Vector3 position)
        {
            return new Vector2(position.x, position.z);
        }

        public override IEnumerator DieRoutine()
        {
            yield return base.DieRoutine();
            
            if (_attackCoroutineObject != null)
                StopCoroutine(_attackCoroutineObject);
            
            audioManager.PlaySoundWithRandomPitch("scream", 0.9f, 1.1f);
            AddImpact(-transform.forward, 50f);

            yield return DissolveRoutine(2);

            Destroy(gameObject);
        }
        
        public void AddImpact(Vector3 direction, float force){
            
            if (direction.y < 0)
                direction.y = -direction.y; // reflect down force on the ground
            
            _impact += (direction.normalized * force) / mass;
        }

        private IEnumerator DissolveRoutine(int seconds)
        {
            var dissolvePower = 1f;
            var time = 0f;
            
            while (dissolvePower > 0)
            {
                time += Time.deltaTime / seconds;
                dissolvePower = Mathf.Lerp(1, 0, time);
                
                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }
        
        public virtual void Attack()
        {
            _attackCoroutineObject ??= StartCoroutine(AttackRoutine());
        }
        
        protected virtual IEnumerator AttackRoutine()
        {
            animator.SetTrigger(AttackHash);

            //TODO: Play if hit (OnTriggerEnter())
            //audioManager.PlaySoundWithRandomPitch("hitPlayer", 0.9f, 1.1f);

            yield return new WaitForSeconds(attackDuration);
            _attackCoroutineObject = null;
        }
        
        public void ToggleAttackCollider(bool isActive)
        {
            attackPoint.gameObject.SetActive(isActive);
        }

        protected Vector2 GetDirectionToTarget()
        {
            var position2D = ConvertToVector2(transform.position);
            
            return (targetToReach - position2D).normalized;
        }
        
        protected void FaceToTarget()
        {
            var position2D = ConvertToVector2(transform.position);
            var direction = (targetToReach - position2D).normalized;
            var direction3D = new Vector3(direction.x, 0f, direction.y);
            
            var cachedRotation = transformToRotate.rotation;
            var lookRotation = Quaternion.LookRotation(direction3D);
            
            cachedRotation = Quaternion.Slerp(cachedRotation, lookRotation, Time.deltaTime * 4f);
            transformToRotate.rotation = cachedRotation;
        }
    }
}
