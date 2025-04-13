using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRotator : MonoBehaviour
{
    public float currentAngle;
    public float startAngle;
    public bool rotating;
    public float rotateDuration;
    // Serializefiel makes private variables visible in the inspectior
    // public variable can be accessed from outside of it's class
    // private you know wait why am I typing this? I know this stuff
    //you type to not get bored or smth idk
    [SerializeField]
    private float counter;

    private float _xAngle;
    public float xAngle 
    { 
        get
        {
            return _xAngle;
        }
        set
        {
            // Here happens some magic.
            // We store the angle to startAngle. _xAngle will be our goal angle
            startAngle = transform.localRotation.eulerAngles.x;
            _xAngle = value;
            rotating = true;
            counter = 0;
        }
    
    }


    // Update is called once per frame
    void Update()
    {
        counter += Time.deltaTime;
        if (counter > rotateDuration && rotating == true)
        {
            rotating = false;
        }
        // rotating code, we "Animate" currentAngle value
        currentAngle = Mathf.LerpAngle(startAngle, xAngle, counter / rotateDuration); // Quternion tomfuckery
        transform.localEulerAngles = new Vector3(currentAngle,0,0);
    }
}
