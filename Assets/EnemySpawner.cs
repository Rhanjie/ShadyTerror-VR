using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public float rotationOffset;

    private void Start()
    {
        StartCoroutine(UpdateRoutine());
    }

    private IEnumerator UpdateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(30, 50));

            var rotationXOffset = Random.Range(-rotationOffset, rotationOffset);
            var rotationYOffset = Random.Range(-rotationOffset, rotationOffset);
            var rotationZOffset = Random.Range(-rotationOffset, rotationOffset);

            var rotation = Quaternion.Euler(rotationXOffset, rotationYOffset, rotationZOffset);

            var spawnedObject = Instantiate(enemyPrefab, transform.position, rotation, transform).transform;
            spawnedObject.localRotation = rotation; //Fast dirty fix
        }
    }
}
