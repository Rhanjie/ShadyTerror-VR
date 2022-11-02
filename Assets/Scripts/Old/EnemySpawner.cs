using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private Enemy enemyPrefab;
    
    [SerializeField]
    private float radius = 10f;
    
    [SerializeField]
    private float minHeight = 1.6f;
    
    [SerializeField]
    private float maxHeight = 1.6f;
    
    [SerializeField]
    private float minSpawnTime = 1f;
    
    [SerializeField]
    private float maxSpawnTime = 3f;

    [SerializeField]
    private bool randomizeTargetPosition;
    
    private GameObject _target;
    
    private void Start()
    {
        _target = GameObject.FindWithTag("Player"); /*.GetComponent<PlayerBehaviour>()*/;
        if (_target == null)
        {
            throw new NotSupportedException("Not found player in the scene!");
        }
        
        StartCoroutine(SpawningRoutine());
    }
    
    private IEnumerator SpawningRoutine()
    {
        while (true)
        {
            SpawnEnemy();

            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));
        }
    }

    private void SpawnEnemy()
    {
        var randomPosition = GetRandomPositionInRadiusEdge();
        var direction = (_target.transform.position - randomPosition).normalized;

        var spawnedEnemy = Instantiate(enemyPrefab, randomPosition, Quaternion.LookRotation(direction), gameObject.transform);
        spawnedEnemy.Init(_target, randomizeTargetPosition ? GetRandomPositionInRadiusEdge : null);
    }

    private Vector3 GetRandomPositionInRadiusEdge()
    {
        var targetTransform = _target.transform;
        var targetPosition = targetTransform.position;
        var targetForward = targetTransform.forward;
        
        var randomPosition = targetPosition + (Random.insideUnitSphere * radius);
        randomPosition.y = Random.Range(minHeight, maxHeight);

        var direction = (targetPosition - randomPosition).normalized;
        var dotProduct = Vector3.Dot(targetForward, direction);
        var dotProductAngle = Mathf.Acos(dotProduct / targetForward.magnitude * direction.magnitude);
        
        randomPosition.x = Mathf.Cos(dotProductAngle) * radius + transform.position.x;
        randomPosition.z = Mathf.Sin(dotProductAngle * (Random.value > 0.5f ? 1f : -1f)) * radius + targetPosition.z;

        return randomPosition;
    }
}
