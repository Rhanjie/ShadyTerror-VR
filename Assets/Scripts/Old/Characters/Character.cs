using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Character : MonoBehaviour, ICharacter
    {
        [Header("References")]
        [SerializeField]
        protected Transform shootingDirection;
        
        [SerializeField]
        protected Transform shootingPoint;
    
        [SerializeField]
        protected TrailRenderer shootTrail;
        
        [SerializeField]
        protected Animator gunAnimator;
        
        [SerializeField]
        private ParticleSystem muzzleFlash;

        [Space(10)]
        [Header("Settings")]
        [SerializeField]
        protected int maxHealth = 5;
        
        [SerializeField]
        protected float maxDistance = 100;

        [SerializeField]
        protected float minGunScatter = 0.1f;
        
        [SerializeField]
        protected float maxGunScatter = 0.3f;
    
        [SerializeField]
        protected float shootingSpeed = 150f;
        
        protected Rigidbody rigidbody;
        protected AudioSource gunAudioSource;
    
        protected float currentGunScatter;
        protected int currentHealth;
        protected bool isHeadshot = false;

        protected bool isDead;
        private static readonly int ShootTrigger = Animator.StringToHash("Shoot");

        protected virtual void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            gunAudioSource = gunAnimator.GetComponent<AudioSource>();
            
            currentHealth = maxHealth;

            if (minGunScatter > maxGunScatter)
            {
                (maxGunScatter, minGunScatter) = (minGunScatter, maxGunScatter);
            }
        }

        protected virtual void Update()
        {
            if (isDead)
                return;

            UpdateCustomBehaviour();
            
            if (currentGunScatter > minGunScatter)
            {
                currentGunScatter -= 0.1f * Time.deltaTime;
            }

            else currentGunScatter = minGunScatter;

            if (currentHealth <= 0)
            {
                StartCoroutine(DieRoutine());
            }
        }

        public abstract void UpdateCustomBehaviour();

        public virtual void Shoot()
        {
            gunAnimator.SetTrigger(ShootTrigger);
            gunAudioSource.Play();
            
            var trail = Instantiate(shootTrail, shootingPoint.position, Quaternion.identity);
            if (Physics.Raycast(transform.position, GetDirection(), out var hit, maxDistance))
            {
                if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("Enemy"))
                {
                    if (hit.transform.gameObject.TryGetComponent(out ICharacter character))
                    {
                        character.Hit(hit.collider.name);
                    }
                    
                    else Debug.LogWarning("The hit target does not inherit from the ICharacter interface!");
                }

                StartCoroutine(SpawnTrail(trail, hit.point, hit));
            }
            else
            {
                var fakeHitPoint = transform.position + (GetDirection() * maxDistance);
                StartCoroutine(SpawnTrail(trail, fakeHitPoint));
            }
        
            currentGunScatter = maxGunScatter;
            muzzleFlash.Play();
        }

        public virtual void Hit(string colliderName)
        {
            //If it is a headshot, then kill the enemy immediately
            isHeadshot = colliderName.Contains("head", StringComparison.CurrentCultureIgnoreCase);
            
            currentHealth -= (isHeadshot) ? maxHealth : 1;
        }

        public virtual IEnumerator DieRoutine()
        {
            isDead = true;

            yield return null;
        }

        protected virtual IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, RaycastHit hit = default)
        {
            var trailPosition = trail.transform.position;

            var distance = Vector3.Distance(trailPosition, hitPoint);
            var remainingDistance = distance;

            while (remainingDistance > 0)
            {
                var time = 1 - (remainingDistance / distance);
                trail.transform.position = Vector3.Lerp(trailPosition, hitPoint, time);

                remainingDistance -= shootingSpeed * Time.deltaTime;

                yield return null;
            }

            trail.transform.position = hitPoint;

            Destroy(trail.gameObject, trail.time);
        }
    
        protected Vector3 GetDirection()
        {
            var direction = shootingDirection.forward;

            direction += new Vector3(
                Random.Range(-currentGunScatter, currentGunScatter),
                Random.Range(-currentGunScatter, currentGunScatter),
                Random.Range(-currentGunScatter, currentGunScatter)
            );
        
            direction.Normalize();
            return direction;
        }
    }
}
