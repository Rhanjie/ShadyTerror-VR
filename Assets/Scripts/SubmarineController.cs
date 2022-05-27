using System.Collections;
using System.Collections.Generic;
using Autohand;
using UnityEngine;

public class SubmarineController : MonoBehaviour
{
    [SerializeField]
    private PhysicsGadgetConfigurableLimitReader doubleSlider;

    private float maxSpeed = 10;
    
    private void Start()
    {
    }
    
    private void FixedUpdate()
    {
        Debug.LogError( doubleSlider.GetValue());

        transform.position += transform.forward * (doubleSlider.GetValue() * maxSpeed * Time.fixedDeltaTime);
    }
}
