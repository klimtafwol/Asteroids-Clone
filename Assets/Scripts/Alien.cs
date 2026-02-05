using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour, IPlayerKillable
{
    // Inspector
    [SerializeField] protected float _rotationSpeed = 360;      // Graphical Rotation of the Ship Graphic
    [SerializeField] protected Bullet _bulletPrefab;            // The Alien Bullet prefab to instantiate
    [SerializeField] protected AudioClip _bulletSound;          // The sound when the Alien fires
    [SerializeField] protected GameObject _explosion;           // Game object containing the explosion when destroyed

    // Internals

    protected Rigidbody _rigidbody;
    protected float _nextFireTime = 0;
    protected Transform _child;
    protected AudioSource _audioSource;



    //---------------------------------------------------------------------------------------------------------------
    // Name :   Awake
    // Desc :    Called by unity when the game object is enabled for the first time
    //---------------------------------------------------------------------------------------------------------------
    protected void Awake()
    {
        // Cache the rigidbody and audio source components also fetch the transform of the child
        // which contains the flying saucer mesh we wish to rotate
        _rigidbody = GetComponent<Rigidbody>();
        _child = transform.GetChild(0);
        _audioSource = GetComponent<AudioSource>();
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Show
    // Desc :   Called by the Game Scene Manager to enable the Alien at a specific Location
    //---------------------------------------------------------------------------------------------------------------

    public void Show(Vector3 position)
    {
        // Move Alien to requested position
        transform.position = position;

        // Enable the Game object
        gameObject.SetActive(true);


    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Update
    // Desc :   Called each frame by unity
    //---------------------------------------------------------------------------------------------------------------
    protected void Update()
    {
        // Get the player component via the Game Scene Manager
        Player player = GameSceneManager.instance.player;
        if (player.isInputDisabled)
            _nextFireTime = Time.time + 4.0f;
        else
        // Otherwise, if its time to fire and the alien is in range of the player
        if (Time.time > _nextFireTime && GameSceneManager.instance.alienInRange)
        {
            // Calculate the direction vector wish which to fire the bullet. this is player position - alien position but
            // also with some added veloctiy to the player position so we fire ahead of the player.
            Vector3 directionToPlayer = ((player.transform.position + player.velocity * GameSceneManager.instance.alienPrediction) - transform.position);

            // Record distance to player and then normalize the direction vector
            float distanceToPlayer = directionToPlayer.magnitude;
            directionToPlayer.Normalize();

            // Creat a ray form alien position pointing in the direction of the player
            Ray ray = new Ray(transform.position, directionToPlayer);

            // Do a sphere cast with a thickness of 3 to see if we have line of sight of the player.
            // we check the asteroid Layer, only if the Ray does NOT hit an asteroid do we fire
            if (!Physics.SphereCast(ray, 3, distanceToPlayer, LayerMask.GetMask("Asteroid")))
                Shoot(transform.position, directionToPlayer);
        }

        // each frame, Rotate the child saucer graphics by the speed set via inspector
        _child.Rotate(0, 0, _rotationSpeed * Time.deltaTime, Space.Self);
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   FixedUpdate  
    //---------------------------------------------------------------------------------------------------------------
    protected void FixedUpdate()
    {
        // Get player from Game SCene Manager
        Player player = GameSceneManager.instance.player;

        // Get Direction from the alien to player
        Vector3 direction = (GameSceneManager.instance.player.transform.position - transform.position).normalized;

        // If input is disabled on the player then move the alien away from the player
        if (player.isInputDisabled)
            direction = -direction;

        // Move the Alien's rigidbody either towards or away from the player by the Alien's current speed. we chuck in a lerp
        // to smoth out direction changes so they look a bit more normal
        _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, direction * GameSceneManager.instance.alienSpeed, Time.deltaTime * 8);

    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Shoot
    // Desc :   Called to fire a bullet at the player
    //---------------------------------------------------------------------------------------------------------------

    protected void Shoot(Vector3 position, Vector3 direction)
    {
        // play the shooting sound of the alien 
        _audioSource.PlayOneShot(_bulletSound);

        // Make a rotation Quaternion to face the direction we shot in
        Quaternion rotation = Quaternion.LookRotation(direction);

        // Instantiate the bullet at the alien's location with a rotation that points towards the player
        Bullet bullet = Instantiate(_bulletPrefab, position, rotation);

        // Let the bullet know what direction it should move along
        bullet.SetDirection(direction);

        // update next fire time so there is a delay before alien will try to fire again
        _nextFireTime = Time.time + GameSceneManager.instance.alienFireDelay;
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   OnCollisionEnter
    // Desc :   Called by unity when a collision ha[[ens with this object's collider
    //---------------------------------------------------------------------------------------------------------------
    protected void OnCollisionEnter(Collision collision)
    {
        // Only the player bullet can kill the alien 
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player Bullet"))
        {
            Kill(true);
        }
    }
    //---------------------------------------------------------------------------------------------------------------
    // Name :   Kill
    // Desc :   Called when Alien has been destroyed by player
    //---------------------------------------------------------------------------------------------------------------

    public void Kill(bool byPlayer)
    {
        // Move the position of the aliene explosion effect to the alien's positoin but above it 20 units on y towards
        // camera. This makes sure explosion happens 'Infront" of the alien from the cameras perspective
        _explosion.transform.position = new Vector3(transform.position.x, 20, transform.position.z);

        // active explosion game object so particle system players
        _explosion.SetActive(true);

        // Disable this Alien ship
        gameObject.SetActive(false);

        // Let the game Scene Manager know that the players has killed another alien
        GameSceneManager.instance.AlienDestroyed();
    }


}
