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
        [Header("Custom")]
        [SerializeField] protected AudioClip laugh;
        [SerializeField] protected AudioClip scream;
        [SerializeField] protected Transform waypointsParent;
        [SerializeField] protected Animator animator;
        
        [Header("Debug")]
        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;
        
        private float _visionLength;
        private bool _foundPlayer;
        private float _attackDuration;

        private CharacterController _characterController;
        private Renderer _renderer;
        private Coroutine _attackCoroutineObject;
        private AudioSource audioSource;
        private GameObject player;
        private Vector2 targetToReach;
        private Func<Vector3> randomPositionMethod;

        private List<Vector2> _waypoints;
        private int _currentWaypointIndex = 0;
        
        private static readonly int DissolvePowerID = Shader.PropertyToID("_DissolvePower");
        private static readonly int VelocityHash = Animator.StringToHash("Velocity");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        protected override void Start()
        {
            base.Start();

            walkSpeed = Random.Range(0.2f, 2f);
            runSpeed = Random.Range(3, 5);
            _visionLength = 10f;

            _waypoints = GetWaypoints();
            _attackDuration = GetAttackDuration();

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
            var attackClip = animator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Zombie_Attack");
            if (attackClip == null)
                throw new KeyNotFoundException("Not found 'Zombie_Attack' animation!");
                
            return attackClip.length;
        }

        public override void UpdateCustomBehaviour()
        {
            ServeGravity();
            
            if (_foundPlayer || TryToFindTarget())
                UpdateAttackRoutine();

            if (!_foundPlayer)
            {
                if (_waypoints.Count == 0)
                    return;
                
                targetToReach = _waypoints[_currentWaypointIndex];
            }
            
            if (_attackCoroutineObject == null)
                UpdateWalkRoutine();
        }

        private bool TryToFindTarget()
        {
            var playerPosition = ConvertToVector2(player.transform.position);
            var distance = Vector3.Distance(playerPosition, transform.position);
            
            //TODO: THROW RAYCAST
            
            return distance <= _visionLength;
        }

        private void UpdateAttackRoutine()
        {
            if (!_foundPlayer)
            {
                PlaySoundWithRandomPitch(laugh, 0.9f, 1.1f);
                
                _foundPlayer = true;
            }

            targetToReach = ConvertToVector2(player.transform.position);
        }

        private void UpdateWalkRoutine()
        {
            FaceToTarget();
            
            var position2D = ConvertToVector2(transform.position);
            var distance = Vector2.Distance(targetToReach, position2D);
            if (distance > 1.5f)
            {
                GoToTarget(position2D);
            }

            else HandleDestinationReached();
        }
        
        private void ServeGravity()
        {
            var verticalVelocity = 0f;
            if (!_characterController.isGrounded)
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            
            _characterController.Move(new Vector3(0f, verticalVelocity, 0f));
        }

        private void GoToTarget(Vector2 position2D)
        {
            var direction = (targetToReach - position2D).normalized;

            var velocity = _foundPlayer ? runSpeed : walkSpeed;
            position2D = direction * velocity * Time.deltaTime;
            
            animator.SetFloat(VelocityHash, velocity);

            _characterController.Move(new Vector3(position2D.x, 0, position2D.y));
        }
        
        private void HandleDestinationReached()
        {
            animator.SetFloat(VelocityHash, 0);
            
            if (_foundPlayer)
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
            
            yield return new WaitForSeconds(_attackDuration);
            
            _attackCoroutineObject = null;
        }
        
        private void FaceToTarget()
        {
            var position2D = ConvertToVector2(transform.position);
            var direction = (targetToReach - position2D).normalized;
            var direction3D = new Vector3(direction.x, 0f, direction.y);
            
            var cachedRotation = transform.rotation;
            var lookRotation = Quaternion.LookRotation(direction3D);
            
            cachedRotation = Quaternion.Slerp(cachedRotation, lookRotation, Time.deltaTime * 4f);
            transform.rotation = cachedRotation;
        }

        protected void PlaySoundWithRandomPitch(AudioClip clip, float minPitch, float maxPitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip);
        }
    }
}
