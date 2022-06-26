using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public GameObject model;
    public ParticleSystem explosion;
    
    private AudioSource _audioSource;
    
    [SerializeField]
    private int _hp = 200;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        //Simple behaviour
        
        if (_hp <= 0)
        {
            StartCoroutine(DeadRoutine());
        }
    }

    public void Hit(int damage)
    {
        _hp -= damage;
    }

    private IEnumerator DeadRoutine()
    {
        explosion.Play();
        _audioSource.Play();
        model.SetActive(false);

        yield return new WaitForSeconds(1);
        
        Destroy(gameObject);
    }
}
