using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class CircleController : MonoBehaviour
{
    public bool invertValue;
    public float playRange = 0.05f;  //requider min. 5% move
    
    public Transform movingObject;
    public Vector3 angle;
    public bool useLocal = false;

    public TextMeshProUGUI text;
    
    private HingeJoint _joint;
    public float Value { get; private set; } = 0;

    private Quaternion _startRot;
    private Quaternion _deltaParentRotation;

    protected void Start(){
        _joint = GetComponent<HingeJoint>(); 
        _startRot = transform.localRotation;
    }
    
    private void FixedUpdate()
    {
        text.text = GetMultiplier().ToString(CultureInfo.CurrentCulture);

        if (useLocal)
        {
            movingObject.localRotation *= Quaternion.Euler(angle * (GetMultiplier() * Time.fixedDeltaTime));
        }

        else movingObject.rotation *= Quaternion.Euler(angle * (GetMultiplier() * Time.fixedDeltaTime));
    }
    
    public float GetMultiplier() 
    {
        var limits = _joint.limits;
        var rotation = Vector3.Scale(UnityEditor.TransformUtils.GetInspectorRotation(transform), _joint.axis);

        var usingRotation = GetUsingAxisValue(rotation);
        Value = usingRotation / (limits.max - limits.min) * (invertValue ? -2 : 2);

        if (Mathf.Abs(Value) < playRange)
        {
            Value = 0;
        }

        return Mathf.Clamp(Value, -1, 1);
    }

    public static float GetUsingAxisValue(Vector3 vector)
    {
        if (Mathf.Abs(vector.x) >= Mathf.Abs(vector.y))
        {
            return Mathf.Abs(vector.x) >= Mathf.Abs(vector.z) ? vector.x : vector.z;
        }
        
        return Mathf.Abs(vector.y) >= Mathf.Abs(vector.z) ? vector.y : vector.z;
    }
}
