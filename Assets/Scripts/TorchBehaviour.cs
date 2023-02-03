using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchBehaviour : MonoBehaviour
{
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private MeshRenderer bulbRenderer;
    
    [SerializeField] private Material lightMaterial;
    [SerializeField] private Material unlightMaterial;

    private Light _light;

    private bool _isLit = true;
    private float _remainingValue = 100;

    private float _totalBulbVerticalScale;
    
    private void Start()
    {
        _light = bulbRenderer.GetComponentInChildren<Light>();
        _totalBulbVerticalScale = bulbRenderer.transform.localScale.y;
        
        Unlight();
    }
    
    private void Update()
    {
        if (!_isLit)
            return;

        //TODO: Update particle rotation

        _remainingValue -= Time.deltaTime;
        if (_remainingValue <= 0f)
        {
            _remainingValue = 0f;
            
            Unlight();
        }

        var bulbTransform = bulbRenderer.transform;
        var newScale = bulbTransform.localScale;
        newScale.y = _totalBulbVerticalScale * (_remainingValue / 100f);
        bulbTransform.localScale = newScale;
    }

    public void Light()
    {
        if (_isLit || _remainingValue <= 0)
            return;
        
        ToggleLight(true);
    }
    
    public void Unlight()
    {
        if (!_isLit)
            return;

        ToggleLight(false);
    }

    private void ToggleLight(bool isActive)
    {
        _isLit = isActive;
        
        _light.enabled = _isLit;
        bulbRenderer.material = _isLit
            ? lightMaterial
            : unlightMaterial;
        
        if (_isLit)
            fireParticles.Play();
        else fireParticles.Stop();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("LightBulb") && !_isLit)
        {
            Light();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Water") && _isLit)
        {
            Unlight();
        }
    }
}
