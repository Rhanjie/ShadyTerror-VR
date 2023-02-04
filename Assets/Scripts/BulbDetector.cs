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
        
        if (collision.collider.gameObject.CompareTag("LightBulb") && !_torchBehaviour.IsLit)
        {
            var foundTorchBehaviour = collision.collider.transform.GetComponentInParent<TorchBehaviour>();
            if (foundTorchBehaviour == null || !foundTorchBehaviour.IsLit || foundTorchBehaviour == _torchBehaviour)
                return;
            
            _torchBehaviour.Light();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_torchBehaviour == null)
            throw new Exception($"Init method was not called for {name}");

        if (other.gameObject.CompareTag("Water"))
        {
            _torchBehaviour.Unlight();
        }
    }
}
