using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
internal struct ImpactParticle
{
    public LayerMask layerMask;
    public ParticleSystem particleSystem;
}

public class GunBehaviour : MonoBehaviour
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
    protected float maxDistance = 100;
    [SerializeField]
    protected float minGunScatter = 0.1f;
    [SerializeField]
    protected float maxGunScatter = 0.3f;
    [SerializeField]
    protected float shootingSpeed = 150f;
    
    [SerializeField]
    protected float bullets = 5f;
    
    [SerializeField]
    private LayerMask hittableLayers;
    
    [SerializeField]
    private List<ImpactParticle> impactParticles;
    
    [SerializeField]
    private AudioClip missingBulletsSound;
    
    protected AudioSource gunAudioSource;
    protected float currentGunScatter;
    
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");

    protected virtual void Start()
    {
        gunAudioSource = GetComponent<AudioSource>();
        
        if (minGunScatter > maxGunScatter)
        {
            (minGunScatter, maxGunScatter) = (maxGunScatter, minGunScatter);
        }
    }

    protected virtual void Update()
    {
        UpdateCustomBehaviour();
        
        if (currentGunScatter > minGunScatter)
        {
            currentGunScatter -= 0.1f * Time.deltaTime;
        }

        else currentGunScatter = minGunScatter;
    }

    public virtual void UpdateCustomBehaviour()
    {
    }

    public virtual void Shoot()
    {
        if (bullets <= 0)
        {
            gunAudioSource.PlayOneShot(missingBulletsSound);
            
            return;
        }

        gunAnimator.SetTrigger(ShootTrigger);
        gunAudioSource.Play();
        
        var trail = Instantiate(shootTrail, shootingPoint.position, Quaternion.identity);
        if (Physics.Raycast(transform.position, GetDirection(), out var hit, maxDistance, hittableLayers))
        {
            StartCoroutine(SpawnTrail(trail, hit.point, hit));
        }
        else
        {
            var fakeHitPoint = transform.position + (GetDirection() * maxDistance);
            StartCoroutine(SpawnTrail(trail, fakeHitPoint));
        }
    
        currentGunScatter = maxGunScatter;
        muzzleFlash.Play();
        bullets -= 1;
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

        HandleDestinationReached(hit);
    }

    private void HandleDestinationReached(RaycastHit hit)
    {
        if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("Enemy"))
        {
            if (hit.transform.gameObject.TryGetComponent(out ICharacter character))
            {
                character.Hit(hit.collider.name);
            }
                
            else Debug.LogWarning("The hit target does not inherit from the ICharacter interface!");
        }
        
        SpawnImpactEffect(hit);
    }

    private void SpawnImpactEffect(RaycastHit hit)
    {
        if (hit.collider == null)
            return;
        
        var hitObjectLayer = hit.collider.gameObject.layer;
        var index = impactParticles.FindIndex(it => it.layerMask == (it.layerMask | (1 << hitObjectLayer)));
        if (index != -1)
        {
            Instantiate(impactParticles[index].particleSystem, hit.point, Quaternion.LookRotation(hit.normal), hit.transform);
        }
    }

    private Vector3 GetDirection()
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
