using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Autohand;
using TMPro;

namespace Autohand.Demo{
    public class WheelRotator : PhysicsGadgetHingeAngleReader{
        public Transform move;
        public Vector3 angle;
        public bool useLocal = false;

        public TextMeshProUGUI text;
        
        private void Update(){
            text.text = GetValue().ToString(CultureInfo.CurrentCulture);
            
            if(useLocal)
                move.localRotation *= Quaternion.Euler(angle*Time.deltaTime*GetValue());
            else
                move.rotation *= Quaternion.Euler(angle*Time.deltaTime*GetValue());
        }
    }
}
