using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    // Inspector
    [SerializeField] protected float _scrollSpeed = 1000.0f;

    // Internals
    protected Material _material;
    protected Vector3 _playerVelocity;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }

    public void onSetScrollSpeed(Vector3 velocity)
    {
        _playerVelocity = velocity;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 offset = _material.GetTextureOffset("_MainTex");
        offset += new Vector2((_playerVelocity.x / _scrollSpeed) * Time.deltaTime, (_playerVelocity.z / _scrollSpeed) * Time.deltaTime);
        _material.SetTextureOffset("_MainTex", offset);
    }
}
