using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    public class Enemy : Character
    {
        [Header("Custom")]
        [SerializeField]
        protected AudioClip laugh;
        
        [SerializeField]
        protected AudioClip scream;
        
        private Renderer _renderer;
        private Coroutine _attackCoroutineObject;
        private float _visionLength;
        private float _maxDistance;
        private float _speed;
        private bool _foundPlayer;
        private bool _goalReached;

        protected AudioSource audioSource;
        protected GameObject targetToShoot;
        protected Vector3 targetToReach;
        protected Func<Vector3> randomPositionMethod;
        
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

            Init(target, null);
        }

        public void Init(GameObject target, Func<Vector3> randomPosition = null)
        {
            targetToShoot = target;
            randomPositionMethod = randomPosition;
            
            //targetToReach = randomPositionMethod?.Invoke() ?? targetToShoot.transform.position;
        }

        public override void UpdateCustomBehaviour()
        {
            targetToReach = targetToShoot.transform.position;
            var distance = Vector3.Distance(targetToReach, transform.position);
            
            if (!_foundPlayer)
            {
                //TODO: Throw a raycast to check if there are any obstacles between enemy and player
                if (distance <= _visionLength)
                {
                    PlaySoundWithRandomPitch(laugh, 0.9f, 1.1f);
                    _foundPlayer = true;
                }
            }

            if (!_foundPlayer)
                return;
            
            FaceToTarget();

            if (_goalReached)
                return;
            
            if (distance <= _maxDistance)
            {
                if (_attackCoroutineObject == null)
                    _attackCoroutineObject = StartCoroutine(AttackRoutine());
            }

            else
            {
                var cachedPosition = transform.position;
                var direction = (targetToReach - cachedPosition).normalized;
                
                cachedPosition += direction * _speed * Time.deltaTime;
                transform.position = cachedPosition;
            }
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
