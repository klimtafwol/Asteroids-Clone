using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//------------------------------------------------------------------------------------------------------------------------------------------
// Class :  Asteroid
// Desc  :   The controllers for our asteroids
//------------------------------------------------------------------------------------------------------------------------------------------
public class Asteroid : MonoBehaviour, IPlayerKillable
{
    // Internals
    protected Rigidbody _rigidbody = null;
    protected Renderer _renderer = null;   
    protected Vector3 _velocity = Vector3.zero;
    protected Vector3 _rotation = Vector3.zero;

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Awake
    // Desc :   Cache frequently used components
    //---------------------------------------------------------------------------------------------------------------

    protected void Awake()
    {
        //Cache renderer and rigidbody
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();

        // Register this asteroid with the game scene managers
        GameSceneManager.instance.AsteroidCreated();
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Kill
    // Desc :   Called when the Asteroid has been hit by a bullet. true will be passed if it was the player's bullet
    // that killed it
    //---------------------------------------------------------------------------------------------------------------
    public void Kill(bool byPlayer)
    {
        // Doe we have a game Scene Manager
        if (GameSceneManager.instance)
        {
            // Tell Game Scene Manager to emit some explosion particles at this asteroid's position and emit more
            // based on mass of object
            GameSceneManager.instance.PlayAsteroidExplosion(transform.position, _rigidbody.mass);

            // Notify the Game Scene Manager an asteroid has been destroyed so points can be awarded based on mass.
            // Note that a mass of zero is passed if this asteroid was not destroyed by the player so no points are
            // awarded
            GameSceneManager.instance.AsteroidDestroyed(byPlayer ? _rigidbody.mass : 0);
           
            // Now destroy ourselves
            Destroy(gameObject);
        }
        

    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   SetProperties
    // Desc :   Called just after an asteroid is instantiated to provide it with its operational
    //          settings
    //---------------------------------------------------------------------------------------------------------------
    public void SetProperties(float mass, Vector3 trajectory)
    {
        // Set a random initial rotation
        transform.eulerAngles = new Vector3(Random.value * 360, Random.value * 360, Random.value * 360);

        // Scale its size based on mass
        transform.localScale = Vector3.one * mass;

        // Set its mass
        _rigidbody.mass = mass;

        // Calculate the constant velocity we would like this asteroid to be travellign. bigger asteroids
        // Travel more slowly
        _velocity = (trajectory / mass) * GameSceneManager.instance.baseSpeed;

        // Clamp speed to greater than max speed for current level
        if(_velocity.magnitude > GameSceneManager.instance.maximumAsteroidSpeed)
            _velocity = _velocity.normalized * GameSceneManager.instance.maximumAsteroidSpeed;

        // Set this as initial velocity of rigidbody
        _rigidbody.velocity = _velocity;

        // Create a vector contain random rotations around each axis we would like to apply constantly
        _rotation = new Vector3(Random.Range(0.0f, 55), Random.Range(0.0f, 55), Random.Range(0.0f, 55));

        // Slow the rotation based on mass / size of asteroid
        _rotation *= 1 / mass; 
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Update
    // Desc :   Called each frame by unity
    //---------------------------------------------------------------------------------------------------------------

    private void Update()
    {
        // Apply some asthetic rotation to the asteroid
        transform.Rotate(_rotation *  Time.deltaTime);
    }


    //---------------------------------------------------------------------------------------------------------------
    // Name :   Fixed Update
    // Desc :   Called just prior to each tick of the physics engine
    //---------------------------------------------------------------------------------------------------------------

    private void FixedUpdate()
    {
        // Are we currently invisible? must have gone off screen
        if (!_renderer.isVisible)
        {
            // get our position in the world
            Vector3 position = _rigidbody.position;

            // Convert the world space position into a 2D point on the screen (Pixel Coordinates)
            Vector3 screeenPos = Camera.main.WorldToScreenPoint(position);

            // If we are off the left side and facing down negative X axis then flip to positive side of screen
            // Otherwise. if we are off the right of the screen and facing down the positive X axis flip to negative
            // side of screen
            if (screeenPos.x < 0 && Vector3.Dot(_rigidbody.velocity, Vector3.right) < 0 ||
                screeenPos.x > Screen.width && Vector3.Dot(_rigidbody.velocity, Vector3.right) >= 0)
            {
                position.x = -position.x;
            }

            // Do the same for top and bottom of screen also. Camera is looking down negative Y in world space
            // so its actually the X and Z components of the player's position that maps to X/Y screen coordinates.
            if (screeenPos.y < 0 && Vector3.Dot(_rigidbody.velocity, Vector3.forward) < 0 ||
                screeenPos.y > Screen.height && Vector3.Dot(_rigidbody.velocity, Vector3.forward) >= 0)
            {
                position.z = -position.z;
            }

            // Update the rigidbody to that new flipped position
            _rigidbody.MovePosition(position);

            // Set velocity direction to head towards center of world
            _rigidbody.velocity = -_rigidbody.position;

        }
           
        // Assure that whatever direction we are now heading, we set the velocity
            // Vector's lenght (a.k.a Speed) equal to the initial speed we set in SetProperties
            _rigidbody.velocity = _rigidbody.velocity.normalized * _velocity.magnitude;
        
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   OnCollisionEnter
    // Desc :   Called by unity when the asteroid collides with another collidor in our scene
    //---------------------------------------------------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        // Have we hit another asteroid
        if (collision.gameObject.layer == LayerMask.NameToLayer("Asteroid"))
        {
            // Get normal of impact as we will use this as the basic of our
            // new velocity vector
            Vector3 newVelocity = collision.GetContact(0).normal;

            // Cancel out any y velocity and re-normalize
            newVelocity.y = 0;
            newVelocity.Normalize();

            // If the new velocity has a shallow angle with regards to X axis then its moving to vetical so add some horizontal weight
            if (Mathf.Abs(newVelocity.x) < 0.4f)
            {
                newVelocity.x = 0.4f * Mathf.Sign(newVelocity.x);
                _rigidbody.velocity = newVelocity.normalized * _velocity.magnitude;
            }
            else
            // or we might be moving too horizontally so add some vertical weight
            if (Mathf.Abs(newVelocity.z) < 0.4f)
            {
                newVelocity.y = 0.4f * Mathf.Sign(newVelocity.y);
                _rigidbody.velocity = newVelocity.normalized * _velocity.magnitude;
            }

            // Otherwise, just make sure the new velocity has its length set to our initial speed
            else
            {
                _rigidbody.velocity = newVelocity.normalized * _velocity.magnitude;
            }

            // Tell the Game Scene Manager the asteroid has collided so play some dust particles based on
            // mass of asteroid at the collision point.
            GameSceneManager.instance.PlayAsteroidCollision(collision.GetContact(0).point, _rigidbody.mass);

        }
        else
        // Have we been hit by either a player or alien bullet
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player Bullet") || collision.gameObject.layer == LayerMask.NameToLayer("Alien Bullet"))
        {
            // If the asteroid isn't already tiny then split it
            if(_rigidbody.mass * 0.5f >= GameSceneManager.instance.minimumAsteroidmass)
            {
                // locals
                Vector2 randomXZ;
                Vector3 position;

                // Get the postion of this asteroid (the parent)
                position = transform.position;

                // Asteroid Child 1
                // Create a random 2D offset so we can seperate the 2 children a little bit
                randomXZ = Random.insideUnitCircle.normalized;

                // Add this 2D offset to the XZ of the parent postion, This will be the postion of the first child
                position += new Vector3(randomXZ.x, 0, randomXZ.y);

                // Create a ne random asteroid at this slightly offset postion
                Asteroid child1 = Instantiate(GameSceneManager.instance.RandomAsteroid, position, transform.rotation);

                // Asteroid Child 2
                // Offset the postion of the 2nd child in the oppostion direction by the same amount
                position = transform.position;
                position += new Vector3(-randomXZ.x, 0, -randomXZ.y);

                // Create a 2nd new random asteroid
                Asteroid child2 = Instantiate(GameSceneManager.instance.RandomAsteroid, position, transform.rotation);

                // Creat a random direction to travel in the XZ direction.
                randomXZ = Random.insideUnitCircle.normalized;
                Vector3 direction = new Vector3(randomXZ.x, 0.0f, randomXZ.y);

                // Set the properties of both children so they have half the mass of the parent and also start off travelling in opposit directions
                child1.SetProperties(_rigidbody.mass * 0.5f, direction);
                child2.SetProperties(_rigidbody.mass * 0.5f, -direction);
            }

            // Tell the asteroid to kill itself, we only pass in true to the kill function
            // If it is the PLAYER'S Bullet that has destroyed us
            Kill(collision.gameObject.layer == LayerMask.NameToLayer("Player Bullet"));
        }
    }

}
