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
        
        protected float visionLength;
        protected bool foundPlayer;
        protected float attackDuration;

        private CharacterController _characterController;
        private Renderer _renderer;
        protected Coroutine _attackCoroutineObject;
        private AudioSource audioSource;
        private GameObject player;
        protected Vector2 targetToReach;
        private Func<Vector3> randomPositionMethod;

        protected List<Vector2> _waypoints;
        protected int _currentWaypointIndex = 0;
        
        protected static readonly int DissolvePowerID = Shader.PropertyToID("_DissolvePower");
        protected static readonly int VelocityHash = Animator.StringToHash("Velocity");
        protected static readonly int AttackHash = Animator.StringToHash("Attack");

        protected override void Start()
        {
            base.Start();

            walkSpeed = Random.Range(0.2f, 2f);
            runSpeed = Random.Range(3, 5);
            visionLength = 10f;

            _waypoints = GetWaypoints();
            attackDuration = GetAttackDuration();
            
            if (transformToRotate == null)
                transformToRotate = transform;

            _characterController = GetComponent<CharacterController>();
            _renderer = GetComponent<Renderer>();
            audioSource = GetComponent<AudioSource>();
            
            var target = GameObject.FindWithTag("Player"); /*.GetComponent<PlayerBehaviour>()*/;
            if (target == null)
            {
                throw new NotSupportedException("Not found player in the scene!");
            }
            
            player = target;
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
            var playerPosition = ConvertToVector2(player.transform.position);
            var distance = Vector3.Distance(playerPosition, transform.position);
            
            //TODO: THROW RAYCAST
            
            return distance <= visionLength;
        }

        protected void UpdateAttackRoutine()
        {
            if (!foundPlayer)
            {
                PlaySoundWithRandomPitch(laugh, 0.9f, 1.1f);
                
                foundPlayer = true;
            }

            targetToReach = ConvertToVector2(player.transform.position);
        }

        protected void UpdateWalkRoutine()
        {
            FaceToTarget();
            
            
            var position2D = ConvertToVector2(transform.position);
            var distance = Vector2.Distance(targetToReach, position2D);
            if (distance > 1.5f)
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
            var velocity = foundPlayer ? runSpeed : walkSpeed;
            var positionMove = direction * (velocity * Time.deltaTime);
            
            animator.SetFloat(VelocityHash, velocity);

            _characterController.Move(new Vector3(positionMove.x, 0, positionMove.y));
        }
        
        private void HandleDestinationReached()
        {
            animator.SetFloat(VelocityHash, 0);
            
            if (foundPlayer)
            {
                _attackCoroutineObject ??= StartCoroutine(AttackRoutine());
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
            
            //targetToShoot.SendDeathMessage(isHeadshot);
            StopCoroutine(_attackCoroutineObject);
            
            PlaySoundWithRandomPitch(scream, 0.9f, 1.1f);
            
            _characterController.attachedRigidbody.freezeRotation = false;
            _characterController.attachedRigidbody.AddForce(-transform.forward * 50f);

            yield return DissolveRoutine(2);

            Destroy(gameObject);
        }

        private IEnumerator DissolveRoutine(int seconds)
        {
            var dissolvePower = 1f;
            var time = 0f;
            
            while (dissolvePower > 0)
            {
                time += Time.deltaTime / seconds;
                dissolvePower = Mathf.Lerp(1, 0, time);

                //If enemy is half invisible then remove collisions
                if (_characterController.attachedRigidbody.detectCollisions && time >= 0.5f)
                {
                    _characterController.attachedRigidbody.detectCollisions = false;
                }

                _renderer.material.SetFloat(DissolvePowerID, dissolvePower);
                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }
        
        private IEnumerator AttackRoutine()
        {
            animator.SetTrigger(AttackHash);
            PlaySoundWithRandomPitch(laugh, 0.9f, 1.1f);
            
            Attack();
            
            yield return new WaitForSeconds(attackDuration);
            
            _attackCoroutineObject = null;
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

        protected void PlaySoundWithRandomPitch(AudioClip clip, float minPitch, float maxPitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip);
        }
    }
}
