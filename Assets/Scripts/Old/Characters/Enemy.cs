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
        private Transform transformToRotate;
        
        [SerializeField]
        protected AudioClip laugh;
        
        [SerializeField]
        protected AudioClip scream;
        
        private Renderer _renderer;
        private Coroutine _shootingCoroutineObject;
        private float _maxDistance;
        private float _speed;
        private bool _goalReached;

        protected AudioSource audioSource;
        protected GameObject targetToShoot;
        protected Vector3 targetToReach;
        protected Func<Vector3> randomPositionMethod;
        
        private static readonly int DissolvePowerID = Shader.PropertyToID("_DissolvePower");

        protected override void Start()
        {
            base.Start();

            _speed = Random.Range(2, 4);
            _maxDistance = Random.Range(15, 25);

            _renderer = GetComponent<Renderer>();
            audioSource = GetComponent<AudioSource>();

            if (transformToRotate == null)
                transformToRotate = transform;

            _shootingCoroutineObject = StartCoroutine(ShootingRoutine());
        }

        public void Init(GameObject target, Func<Vector3> randomPosition = null)
        {
            targetToShoot = target;
            randomPositionMethod = randomPosition;
            
            targetToReach = randomPositionMethod?.Invoke() ?? targetToShoot.transform.position;
        }

        public override void UpdateCustomBehaviour()
        {
            FaceToTarget();

            if (_goalReached)
            {
                return;
            }

            var distance = Vector3.Distance(targetToReach, transform.position);
            if (distance <= _maxDistance)
            {
                if (randomPositionMethod == null)
                {
                    PlaySoundWithRandomPitch(laugh, 0.9f, 1.1f);
                    _goalReached = true;
                }
                    
                else targetToReach = randomPositionMethod();
            }

            else
            {
                var direction = (targetToReach - transform.position).normalized;
                
                transform.position += direction * _speed * Time.deltaTime;
            }
        }

        public override IEnumerator DieRoutine()
        {
            yield return base.DieRoutine();
            
            //targetToShoot.SendDeathMessage(isHeadshot);
            StopCoroutine(_shootingCoroutineObject);
            
            PlaySoundWithRandomPitch(scream, 0.9f, 1.1f);
            
            rigidbody.freezeRotation = false;
            rigidbody.AddForce(-transform.forward * 50f);

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
                if (rigidbody.detectCollisions && time >= 0.5f)
                {
                    rigidbody.detectCollisions = false;
                }

                _renderer.material.SetFloat(DissolvePowerID, dissolvePower);
                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }
        
        private IEnumerator ShootingRoutine()
        {
            //Endless loop but coroutine will be stopped when enemy dies
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(1f, 4f));

                Shoot();
            }
        }
        
        private void FaceToTarget()
        {
            var direction = (targetToShoot.transform.position - transformToRotate.position).normalized;
            var lookRotation = Quaternion.LookRotation(direction);
            
            transformToRotate.rotation = Quaternion.Slerp(transformToRotate.rotation, lookRotation, Time.deltaTime * 2f);
        }

        protected void PlaySoundWithRandomPitch(AudioClip clip, float minPitch, float maxPitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            
            audioSource.PlayOneShot(clip);
        }
    }
}
