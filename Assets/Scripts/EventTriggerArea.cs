using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class EventTriggerArea : MonoBehaviour
{
    public UnityEvent actions;

    private BoxCollider _collider;
    
    private void Start()
    {
        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        _collider.enabled = false;
        actions.Invoke();
    }
}
