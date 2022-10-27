using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public GameObject model;
    public ParticleSystem explosion;
    
    [SerializeField]
    private AudioSource _audioSource;
    
    [SerializeField]
    private Transform rootTransform;
    
    [SerializeField]
    private Transform prefab;
    
    [SerializeField]
    private int hp = 200;
    
    [SerializeField]
    private float multiplier = 2f;
    
    [SerializeField]
    private float maxSpeed = 4;
    
    private float _currentSpeed;
    private bool _isDead = false;
    private bool _isStop = false;

    private Gun _gun;
    private Transform _target;

    private void Start()
    {
        _gun = GetComponent<Gun>();

        _target = GameObject.Find("StaticRoot").transform;
    }

    private void Update()
    {
        //Simple behaviour

        if (!_isDead)
        {
            if (hp <= 0)
            {
                StartCoroutine(DeadRoutine());
            }
            
            _isStop = false;
            
            var distance = Vector3.Distance(_target.position, rootTransform.position);
            if (distance <= 160f)
            {
                _gun.SetShooting(true);

                FaceToTarget();

                if (distance <= 30)
                {
                    _isStop = true;
                }
            }

            else _gun.SetShooting(false);
            
            //Remove if too far
            if (distance > 1100f)
            {
                Destroy(prefab.gameObject);
            }
        }
    }

    private void FaceToTarget()
    {
        var direction = (_target.position - rootTransform.position).normalized;
        var lookRotation = Quaternion.LookRotation(direction);
        rootTransform.rotation = Quaternion.Slerp(rootTransform.rotation, lookRotation, Time.deltaTime * 2f);
    }
    
    private void FixedUpdate()
    {
        if (!_isStop)
        {
            _currentSpeed += (multiplier * Time.fixedDeltaTime);
        }
        
        else _currentSpeed -= multiplier * Time.fixedDeltaTime;
        
        if (_currentSpeed > maxSpeed)
            _currentSpeed = maxSpeed;
        
        if (_currentSpeed < -maxSpeed)
            _currentSpeed = -maxSpeed;
        
        if (_isStop && Math.Abs(_currentSpeed) <= 0.1)
            _currentSpeed = 0;

        rootTransform.position += rootTransform.forward * (_currentSpeed * Time.fixedDeltaTime);
    }

    public void Hit(int damage)
    {
        hp -= damage;
    }

    private IEnumerator DeadRoutine()
    {
        _isDead = true;
        
        explosion.Play();
        _audioSource.Play();
        model.SetActive(false);

        yield return new WaitForSeconds(2);
        
        Destroy(prefab.gameObject);
    }
}
