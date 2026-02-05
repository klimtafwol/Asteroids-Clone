using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    // Inspector
    [SerializeField] protected Vector2 _scrollSpeed = Vector3.zero;         // We can Specify X and Y offset in the -/+ range

    // Internals
    Material _material;

    //------------------------------------------------------------------------------------------------
    // Name :   Awake
    // Desc :   Called when object is first created and enabled
    //------------------------------------------------------------------------------------------------
    private void Awake()
    {
        // Cache a renderence to the material of the renderer on this game object
        _material = GetComponent<Renderer>().material;  
    }

    //------------------------------------------------------------------------------------------------
    // Name :    Update
    // Desc :     Called once per frame by unity 
    //------------------------------------------------------------------------------------------------

    void Update()
    {
        // Get the current texture offsets of the materiuals main albedo texture
        Vector2 offset = _material.GetTextureOffset("_MainTex");

        // Re-set the offsets with out scroll speed vector added scaed into a per second value
        _material.SetTextureOffset("_MainTex", offset + (_scrollSpeed * Time.deltaTime));
    }
}
