using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//------------------------------------------------------------------------------------------------------------------------------------------
// Class :  Bullet
// Desc  :  Controller script used for Player and Alien bullets. Should exist on PREFAB of bullet as Bullets are instantiated when needed
//------------------------------------------------------------------------------------------------------------------------------------------
public class Bullet : MonoBehaviour, IPlayerKillable
{
    // Inspector
    [SerializeField] protected float            _speed = 50f;              // Speed of bullet
    [SerializeField] protected Rigidbody    _rigidbody = null;             // We hook up rigidbody via inspector for efficiency
    [SerializeField] protected int _lifeDuration = 5;
    //------------------------------------------------------------------------------------------------------------------------------------------
    // Name :   Kill
    // Desc :   Called when the bullet hits something, parameter of the IPlayerKillable interface is not used in this case
    //------------------------------------------------------------------------------------------------------------------------------------------
    public void Kill(bool byPlayer)
    {
        // Destroy this Game Object from the scene
        Destroy(gameObject);
    }

    //------------------------------------------------------------------------------------------------------------------------------------------
    // Name :   SetDirection
    // Desc :   Called by whoever just instantiated this bullet (Alien or Player) to notify the bullet of the direction in which it should travel
    //------------------------------------------------------------------------------------------------------------------------------------------

    public void SetDirection(Vector3 direction)
    {
        // The bullet only needs a force to be added once since they habe no drag to make them stop moving
        _rigidbody.AddForce(direction * _speed, ForceMode.Impulse);

        // Destroy this game Object automatically when the life duration expires
        Destroy(gameObject, _lifeDuration);
    }

    //------------------------------------------------------------------------------------------------------------------------------------------
    // Name :   OnCollisionEnter
    // Desc :   If a bullet collides with ANYTHING it dies
    //------------------------------------------------------------------------------------------------------------------------------------------

    private void OnCollisionEnter(Collision collision)
    {
        Kill(true);
    }
}
