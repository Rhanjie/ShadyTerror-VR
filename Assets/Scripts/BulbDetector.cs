using System;
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

        IfTouchAnotherLight(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_torchBehaviour == null)
            throw new Exception($"Init method was not called for {name}");

        if (other.gameObject.CompareTag("Water"))
        {
            _torchBehaviour.Unlight();
        }
        
        else IfTouchAnotherLight(other);
    }

    private void IfTouchAnotherLight(Collider other)
    {
        if (!_torchBehaviour.IsLit && (other.gameObject.CompareTag("LightBulb") || other.gameObject.CompareTag("Fire")))
        {
            var foundTorchBehaviour = other.transform.GetComponentInParent<TorchBehaviour>();
            if (foundTorchBehaviour == null || !foundTorchBehaviour.IsLit || foundTorchBehaviour == _torchBehaviour)
                return;
            
            _torchBehaviour.Light();
        }
    }
}
