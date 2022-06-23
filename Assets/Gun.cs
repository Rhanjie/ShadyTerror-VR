using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gun : MonoBehaviour
{
    [SerializeField]
    private bool AddBulletSpread = true;
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private Transform BulletSpawnPoint;
    [SerializeField]
    private ParticleSystem ImpactParticleSystem;
    [SerializeField]
    private TrailRenderer BulletTrail;
    [SerializeField]
    private float ShootDelay = 0.5f;
    [SerializeField]
    private LayerMask Mask;
    
    [SerializeField]
    private float BulletSpeed = 100;
    
    [SerializeField]
    private float totalDistance = 200;

    //private Animator Animator;
    private float _lastShootTime;
    
    private void Awake()
    {
        //Animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Shoot();
    }

    public void Shoot()
    {
        if (_lastShootTime + ShootDelay < Time.time)
        {
            //Animator.SetBool("IsShooting", true);
            ShootingSystem.Play();

            var trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
            if (!Physics.Raycast(BulletSpawnPoint.position, GetDirection(), out RaycastHit hit, totalDistance, Mask))
            {
                var transformReference = transform;
                var fakeHitPoint = transformReference.position + (GetDirection() * totalDistance);
                
                StartCoroutine(SpawnTrail(trail, fakeHitPoint, false));
            }
            
            else StartCoroutine(SpawnTrail(trail, hit.point, true));

            _lastShootTime = Time.time;
        }
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;

        if (!AddBulletSpread)
        {
            return direction;
        }
        
        direction += new Vector3(
            Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
            Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
            Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
        );

        direction.Normalize();

        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, bool madeImpact)
    {
        var transformReference = trail.transform;
        var startPosition = transformReference.position;
        
        var distance = Vector3.Distance(transformReference.position, hitPoint);
        var remainingDistance = distance;

        while (remainingDistance > 0)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (remainingDistance / distance));

            remainingDistance -= BulletSpeed * Time.deltaTime;

            yield return null;
        }
        
        //Animator.SetBool("IsShooting", false);
        trail.transform.position = hitPoint;
        if (madeImpact)
        {
            Instantiate(ImpactParticleSystem, hitPoint, Quaternion.LookRotation(hitPoint));
        }

        Destroy(trail.gameObject, trail.time);
    }
}
