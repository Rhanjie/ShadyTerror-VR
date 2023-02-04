using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulbDetector : MonoBehaviour
{
    private TorchBehaviour _torchBehaviour;
    
    public void Init(TorchBehaviour torchBehaviour)
    {
        _torchBehaviour = torchBehaviour;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (_torchBehaviour == null)
            throw new Exception($"Init method was not called for {name}");
        
        if (collision.collider.gameObject.CompareTag("LightBulb") && !_torchBehaviour.IsLit)
        {
            var foundTorchBehaviour = collision.collider.transform.parent.GetComponent<TorchBehaviour>();
            if (foundTorchBehaviour == null || !foundTorchBehaviour.IsLit)
                return;
            
            _torchBehaviour.Light();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_torchBehaviour == null)
            throw new Exception($"Init method was not called for {name}");

        if (other.gameObject.CompareTag("Water") && _torchBehaviour.IsLit)
        {
            _torchBehaviour.Unlight();
        }
    }
}
