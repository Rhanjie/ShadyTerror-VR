using System.Collections;
using System.Collections.Generic;
using Autohand;
using UnityEngine;

public class SubmarineController : MonoBehaviour
{
    [SerializeField]
    private PhysicsGadgetConfigurableLimitReader moveHandler;

    public float speed = 10;

    private void Start()
    {
    }
    
    private void FixedUpdate()
    {
        transform.position += transform.forward * (moveHandler.GetValue() * speed * Time.fixedDeltaTime);
    }
}
