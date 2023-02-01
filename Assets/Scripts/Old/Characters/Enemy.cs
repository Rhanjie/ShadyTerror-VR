using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    public class Enemy : Character
    {
        [Header("Custom")]
        [SerializeField] protected AudioClip laugh;
        [SerializeField] protected AudioClip scream;
        [SerializeField] protected List<Transform> waypoints;

        private Renderer _renderer;
        private Coroutine _attackCoroutineObject;
        private float _visionLength;
        private float _maxDistance;
        private float _speed;
        private bool _foundPlayer;
        private bool _goalReached;

        private AudioSource audioSource;
        private GameObject targetToShoot;
        private Vector3 targetToReach;
        private Func<Vector3> randomPositionMethod;
        
        private int _currentWaypointIndex = 0;
        
        private static readonly int DissolvePowerID = Shader.PropertyToID("_DissolvePower");

        protected override void Start()
        {
            base.Start();

            _speed = Random.Range(3, 5);
            _visionLength = 20f;
            _maxDistance = Random.Range(2, 3);

            _renderer = GetComponent<Renderer>();
            audioSource = GetComponent<AudioSource>();
            
            var target = GameObject.FindWithTag("Testable"); /*.GetComponent<PlayerBehaviour>()*/;
            if (target == null)
            {
                throw new NotSupportedException("Not found player in the scene!");
            }
            
            targetToShoot = target;
        }

        public override void UpdateCustomBehaviour()
        {
            TryToFindTarget();
            UpdateRoutine();
        }

        private void TryToFindTarget()
        {
            targetToReach = targetToShoot.transform.position;
            
            var distance = Vector3.Distance(targetToReach, transform.position);
            if (distance > _visionLength)
                return;
            
            PlaySoundWithRandomPitch(laugh, 0.9f, 1.1f);
            FaceToTarget();
            
            _foundPlayer = true;
        }

        private void UpdateRoutine()
        {
            if (!_foundPlayer)
                targetToReach = waypoints[_currentWaypointIndex].position;
            
            var distance = Vector2.Distance(targetToReach, transform.position);
            if (distance > 0.5f)
            {
                GoToTarget();
            }

            else HandleDesinationReached();
        }

        private void GoToTarget()
        {
            var cachedPosition = transform.position;
            var direction = (targetToReach - cachedPosition).normalized;
                
            cachedPosition += direction * _speed * Time.deltaTime;
            transform.position = cachedPosition;
        }
        
        private void HandleDesinationReached()
        {
            if (_foundPlayer)
            {
                _attackCoroutineObject ??= StartCoroutine(AttackRoutine());
            }
            
            else _currentWaypointIndex = GetNextWaypointIndex(_currentWaypointIndex);
        }

        private int GetNextWaypointIndex(int previousIndex)
        {
            return (previousIndex + 1) % waypoints.Count;
        }

        public override IEnumerator DieRoutine()
        {
            yield return base.DieRoutine();
            
            //targetToShoot.SendDeathMessage(isHeadshot);
            StopCoroutine(_attackCoroutineObject);
            
            PlaySoundWithRandomPitch(scream, 0.9f, 1.1f);
            
            dynamicBody.freezeRotation = false;
            dynamicBody.AddForce(-transform.forward * 50f);

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
                if (dynamicBody.detectCollisions && time >= 0.5f)
                {
                    dynamicBody.detectCollisions = false;
                }

                _renderer.material.SetFloat(DissolvePowerID, dissolvePower);
                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }
        
        private IEnumerator AttackRoutine()
        {
            PlaySoundWithRandomPitch(laugh, 0.9f, 1.1f);
            
            Attack();
            
            yield return new WaitForSeconds(Random.Range(1f, 4f));
            yield return null;
        }
        
        private void FaceToTarget()
        {
            var direction = (targetToShoot.transform.position - transform.position).normalized;
            var lookRotation = Quaternion.LookRotation(direction);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
        }

        protected void PlaySoundWithRandomPitch(AudioClip clip, float minPitch, float maxPitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            
            audioSource.PlayOneShot(clip);
        }
    }
}
