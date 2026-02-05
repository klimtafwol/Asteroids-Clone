using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuShipRotator : MonoBehaviour
{
    // Inspector
    [SerializeField] protected AnimationCurve _xRotationCurve;      // The animation curve
    [SerializeField] protected float _speed = 0;                    // Speed to process the curve
    [SerializeField] protected float _scale = 10;                   // Scale to magnify the curve

    // Internals
    protected Vector3 _initialRotation;
    protected float _time = 0;


    //------------------------------------------------------------------------------------------------
    // Name :   Awake
    // Desc :   Cache the initial local rotation of the object
    //------------------------------------------------------------------------------------------------

    void Awake()
    {
        _initialRotation = transform.localEulerAngles;
    }

    //------------------------------------------------------------------------------------------------
    // Name :   Update
    // Desc :   Called onced per frame by unity
    //------------------------------------------------------------------------------------------------

    void Update()
    {
        // Calculate the current local X rotation of the object by first using time to evaluate
        // the curve. We will then scale the returned value into our desired range and add this to
        // the initial X rotation
        transform.localEulerAngles = new Vector3(_initialRotation.x + (_xRotationCurve.Evaluate(_time % 6) * _scale), _initialRotation.y, _initialRotation.z);

        // Accumulate time by multiplied by speed so we can control the speed at which we step through the curve
        _time += Time.deltaTime * _speed;
    }
}
