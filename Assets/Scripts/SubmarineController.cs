using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using UnityEngine;

public class SubmarineController : MonoBehaviour
{
    [SerializeField]
    private PhysicsGadgetConfigurableLimitReader moveHandler;

    public float maxSpeed = 2;

    private float _currentSpeed;

    private void Start()
    {
    }
    
    private void FixedUpdate()
    {
        var forwardMove = _currentSpeed >= 0;
        var handlerOnDefault = moveHandler.GetValue() == 0;
        
        if (handlerOnDefault)
        {
            _currentSpeed += (0.5f * Time.fixedDeltaTime) * (forwardMove ? -1 : 1);
        }
        
        else _currentSpeed += moveHandler.GetValue() * Time.fixedDeltaTime;

        
        if (_currentSpeed > maxSpeed)
            _currentSpeed = maxSpeed;
        
        if (_currentSpeed < -maxSpeed)
            _currentSpeed = -maxSpeed;
        
        if (handlerOnDefault && Math.Abs(_currentSpeed) <= 0.1)
            _currentSpeed = 0;

        Debug.LogError(_currentSpeed);

        transform.position += transform.forward * (_currentSpeed * Time.fixedDeltaTime);
    }
}
