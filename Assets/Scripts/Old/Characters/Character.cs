using System;
using System.Collections;
using UnityEngine;

namespace Characters
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Character : MonoBehaviour, ICharacter
    {
        [Header("References")]
        [SerializeField]
        protected Transform attackPoint;
        
        [SerializeField]
        protected TrailRenderer attackTrail;
        
        [Space(10)]
        [Header("Settings")]
        [SerializeField]
        protected int maxHealth = 5;
        
        protected Rigidbody dynamicBody;
        
        protected int currentHealth;
        protected bool isHeadshot = false;
        protected bool isDead;

        protected virtual void Start()
        {
            dynamicBody = GetComponent<Rigidbody>();

            currentHealth = maxHealth;
        }

        protected virtual void Update()
        {
            if (isDead)
                return;

            UpdateCustomBehaviour();
            
            if (currentHealth <= 0)
            {
                StartCoroutine(DieRoutine());
            }
        }

        public abstract void UpdateCustomBehaviour();

        public virtual void Attack()
        {
            var trail = Instantiate(attackTrail, attackPoint.position, Quaternion.identity);
            if (Physics.Raycast(transform.position, transform.forward, out var hit, 100f))
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
                var fakeHitPoint = transform.position + (transform.forward);
                StartCoroutine(SpawnTrail(trail, fakeHitPoint));
            }
            
            //hitSound.Play();
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

                remainingDistance -= 10 * Time.deltaTime;

                yield return null;
            }

            trail.transform.position = hitPoint;

            Destroy(trail.gameObject, trail.time);
        }
    }
}
