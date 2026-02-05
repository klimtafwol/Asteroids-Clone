using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkText : MonoBehaviour
{
    // Inspector
    [SerializeField] protected float _blinkTime = 0.75f;  // blink period

    // Internals
    Text    _textToBlink;       // Text element to enable / disable during blinking
    float   _nextToggleTime;    // Next Time to enable / disable during blinking

    //---------------------------------------------------------------------------------------------------
    // Name :   Awake
    // Desc :   Called by Unity whne object is first created and enabled
    //---------------------------------------------------------------------------------------------------

    private void Awake()
    {
        // Cache a reference to the Text component on this object

        _textToBlink = GetComponent<Text>(); 

    }

    private void OnEnable()
    {
        _textToBlink.enabled = true;
        _nextToggleTime = Time.time + _blinkTime;

    }

    //---------------------------------------------------------------------------------------------------
    // Name :   Update
    // Desc :   Called once per frame by unity
    //---------------------------------------------------------------------------------------------------

    void Update()
    {
        // has the current time surpassed the next toggle time
        if (Time.time >= _nextToggleTime)
        {
            // Flipe the status of the UI text component from enable to disabked or vice versa
            _textToBlink.enabled = !_textToBlink.enabled;

            // Update the next toggle time so we dont do this again until the blink interval has passed
            _nextToggleTime = Time.time + _blinkTime;
        }
    }
}
