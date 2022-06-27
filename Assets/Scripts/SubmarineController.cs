using System;
using System.Collections;
using Autohand;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class SubmarineController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PhysicsGadgetConfigurableLimitReader moveHandler;
    
    [SerializeField]
    private UiDisplay uiDisplay;

    [Header("Values")]
    public float maxSpeed = 6;
    public float multiplier = 2f;
    
    public int maxHp = 500;

    private int _hp;
    private float _currentSpeed;
    private bool _isDead = false;

    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        _hp = maxHp;
        
        uiDisplay.SetHp(_hp, maxHp);
    }

    public void Hit(int damage)
    {
        _hp -= damage;
        
        //PlaySound
        
        uiDisplay.SetHp(_hp, maxHp);

        if (_hp <= 0 && !_isDead)
        {
            StartCoroutine(DeadRoutine());
        }
    }

    private IEnumerator DeadRoutine()
    {
        //TODO: Add better dead
        _isDead = true;

        _rigidbody.useGravity = true;
        yield return new WaitForSeconds(3);
            
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void FixedUpdate()
    {
        var forwardMove = _currentSpeed >= 0;
        var handlerOnDefault = moveHandler.GetValue() == 0;
        
        if (handlerOnDefault)
        {
            _currentSpeed += (multiplier * Time.fixedDeltaTime) * (forwardMove ? -1 : 1);
        }
        
        else _currentSpeed += moveHandler.GetValue() * multiplier * Time.fixedDeltaTime;
        
        if (_currentSpeed > maxSpeed)
            _currentSpeed = maxSpeed;
        
        if (_currentSpeed < -maxSpeed)
            _currentSpeed = -maxSpeed;
        
        if (handlerOnDefault && Math.Abs(_currentSpeed) <= 0.1)
            _currentSpeed = 0;

        transform.position += transform.forward * (_currentSpeed * Time.fixedDeltaTime);
    }
}
