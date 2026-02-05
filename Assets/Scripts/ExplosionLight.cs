using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionLight : MonoBehaviour
{
    // Inspector
    [SerializeField] protected Light _light;

    // Internals
    float _intensity = float.MinValue;
    float _showDuration = 3.0f;


    public void ShowLight(float range)
    {
        if (_intensity == float.MinValue)
            _intensity = _light.intensity;

        _light.range = range;
        _light.intensity = _intensity;
        _light.enabled = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        _light.intensity = Mathf.Lerp(_light.intensity, 0, Time.deltaTime * 5.0f);
    }

    private void OnEnable()
    {
        Invoke("DisableMe", _showDuration);
    }

    private void DisableMe()
    {
        gameObject.SetActive(false);    
    }
}
