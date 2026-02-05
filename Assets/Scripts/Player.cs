using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
//------------------------------------------------------------------------------------------------------------------------
// Class    :   Player
// Desc     :   Script that controls the player's ship
//------------------------------------------------------------------------------------------------------------------------


public class Player : MonoBehaviour
{
    // Inspector
    [Header("Control")]
    [SerializeField] protected float _thrustAmount = 40f;           // Force to add when thrust is pressed
    [SerializeField] protected float _turnRate = 105f;              // Torque to add when turning
    [SerializeField] protected bool _allowReverseThrust = false;    // Do we allow the reverse key
    [SerializeField] protected ParticleSystem _thrusterSystem = null;    // Particle System used for Player's Rocket engines


    [Header("Weapons")]
    [SerializeField] protected Bullet _bulletPrefab;                // Prefab to instantiate for player bullet
    [SerializeField] protected GameObject _explosionSystem = null;  // Game Object of Explosion Effect when player is destroyed
    [SerializeField] protected AudioClip _blasterSound = null;      // Audio clip of Player firing sound

    [Header("Hyperspace")]
    [SerializeField] protected GameObject       _hyperspaceEffect = null;               // Game object of Hyperspace Particle effect
    [SerializeField] protected AudioClip        _hyperspaceSound = null;                // Audio clip of player Hyperspace sound
    [SerializeField] protected float            _hyperspaceAudioOffset = 1.1f;          // Used to offset sound and effect so they sync up
    [SerializeField] protected float            _hyperspaceDestructionRadius = 60;      // Radius of destruction to asteroids when invoke

    // Event to notify of players velocity
    public UnityEvent<Vector3> OnVelocity;

    //Internals
    protected float         _thrustAxis;            // Thrust Input -1 to +1
    protected float         _turnAxis;              // Turn Input -1 to +1
    protected Camera        _camera;                // Scene Camera
    protected Rigidbody     _rigidBody;             // Rigidbody
    protected Renderer[]    _renderers;             // All renders contain in player's hierarchy
    protected bool          _isHyperSpacing;        // Are we in the process of Hyperspacing?
    protected bool          _inputDisabled;         // Is player input used for playing sounds
    protected AudioSource   _audioSource;           // Audio Source used for playing sounds
    protected AudioSource   _thrusterAudioSources;  // Thruster has its own audio source as its always playing

    //Properties
    public Vector3 velocity
    {
        get { return _rigidBody.velocity; }
    }

    protected bool isVisible
    {
        get
        {
            // Iterate through child renders
            for( int i = 0; i < _renderers.Length; i++ )
            {
                //  Return true if any renderer is visible
                if(_renderers[i].isVisible)
                    return true;
            }
           
            return false;
        }
    }
    public bool isInputDisabled
    {
        get { return _inputDisabled; }
    }

    //----------------------------------------------------------------------------------------------------------------
    // Name :   Awake
    // Desc :   Called by unity when the object is first constructed
    //----------------------------------------------------------------------------------------------------------------

    protected void Awake()
    {
        //Cache frequently used components for efficiency
        _camera = Camera.main;
        _rigidBody = GetComponent<Rigidbody>();

        // Get all renderers in hierarchy
        _renderers = GetComponentsInChildren<Renderer>();

        // Fetch and cache audio sources
        AudioSource[] audioSources = GetComponents<AudioSource>();
        _audioSource                = audioSources[0];
        _thrusterAudioSources       = audioSources[1];
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Start
    // Desc :   Called by unity prior to veryt first Update. we will run a coroutine to hyperspace the player into the level
    //---------------------------------------------------------------------------------------------------------------
    protected void Start()
    {
        // Start Corountin to put player into the level in 2 seconds
        StartCoroutine(Reset(2));
    }

    //----------------------------------------------------------------------------------------------------------------
    // Name :   Update
    // Desc :   Called every frame by unity to update non-physics stuff
    //----------------------------------------------------------------------------------------------------------------

    protected void Update()
    {
        // while input is disabled make sure we cancel out any linear or angular velocity being appplied to the the rigidbody
        if (_inputDisabled)
        {
            
            // Stop us from moving
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;

            // Turn down the thruster sound volume
            _thrusterAudioSources.volume = 0.0f;

            // do nothing else
            return;
        }

        _thrustAxis = Input.GetAxisRaw("Vertical");
        _turnAxis = Input.GetAxisRaw("Horizontal");

        // Clamp to zero if we disallow reverse thrust
        _thrustAxis = _thrustAxis<0 && !_allowReverseThrust ? 0.0f : _thrustAxis;

        // If we have at least some movement in our thrust axis
        if(Mathf.Abs(_thrustAxis) > 0.1f)
        {
           
            // Lerp up the volume of the thruster audio source
            _thrusterAudioSources.volume = Mathf.Lerp(_thrusterAudioSources.volume, 1, Time.deltaTime * 15);

            // if the thruster particle system isnt currently playing then play it.
            if (!_thrusterSystem.isPlaying)
                _thrusterSystem.Play();
        }
        else
        // Otherwise, if we are not applying any thrust
        {
            // Lerp the volume of the thruster audio source down to zero
            _thrusterAudioSources.volume = Mathf.Lerp(_thrusterAudioSources.volume, 0, Time.deltaTime * 15);
            
            // Stop the trhuster particle system playing
            if( _thrusterSystem.isPlaying)
                _thrusterSystem.Stop();
        }

        // Is the fire button pressed?
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }

        // if the Hyperspace button pressed?
        if (Input.GetButtonDown("Hyperspace") && !_isHyperSpacing && GameSceneManager.instance.hyperspaceAvailable)
        {
            StartCoroutine(Hyperspace());
        }
    }
    //----------------------------------------------------------------------------------------------------------------
    // Name :   FixedUpdate
    // Desc :   Called by unity with each TICK of the physics system prior to performing the physics update.
    //----------------------------------------------------------------------------------------------------------------

    protected void FixedUpdate()
    {
        // Are we currently invisible? Must have gone off the screen
        if(!isVisible)
        {
            // Get our position in the world
            Vector3 position = _rigidBody.position;

            // Convert the world space position into a 2D point on the screen (pixel Coordinates)
            Vector3 screenPos =  _camera.WorldToScreenPoint(position);

            // If we are off the left side and facing down negative X then flip to positive side of screen
            // otherwise, if we are off the right of the screen and facing down the positive x flip to negative side of screen
            if (screenPos.x < 0 && Vector3.Dot(_rigidBody.velocity, Vector3.right) < 0)
            {
                position.x = -position.x;
            }
            else if (screenPos.x > Screen.width && Vector3.Dot(_rigidBody.velocity, Vector3.right) >= 0)
            {
                position.x = -position.x;
            }
            
            // Do the same for top and bottom of screen aswell. camera is looking down negative Y in world space
            // So its actually the X and Z componenets of the players position that maps X/Y screen coordinates
            if (screenPos.y < 0 && Vector3.Dot(_rigidBody.velocity, Vector3.forward) < 0)
            {
                position.z = -position.z;
            }
            else if (screenPos.y > Screen.height && Vector3.Dot(_rigidBody.velocity, Vector3.forward) >= 0)
            {
                position.z = -position.z;
            }

            // Update the rigidbody to that new flipped position
            _rigidBody.MovePosition(position);
        }
        
        
        // Is the player applying thrust
        if (_thrustAxis != 0)
        {
            _rigidBody.AddForce(transform.forward * _thrustAxis * _thrustAmount);
        }

        // Is the player appling rotation
        if(_turnRate != 0f)
        {
            // Add torque around the player's Y axis by our turn rate scaled by our turn axis
            _rigidBody.AddTorque(0, _turnRate * _turnAxis,0);
        }

        // Invoke the OnVelocity event so any listeneres can get our updated velocity.
        OnVelocity.Invoke(_rigidBody.velocity);

    }

    //----------------------------------------------------------------------------------------------------------------
    // Name :   Shoot
    // Desc :   Project a round form the player's blaster
    //----------------------------------------------------------------------------------------------------------------
    protected void Shoot()
    {
        // Play the blaster sound clip
        _audioSource.PlayOneShot(_blasterSound);
        
        // Instantiate an instance of the Player Bullet and set its direction equal to the direct the player is facing
        Bullet bullet = Instantiate(_bulletPrefab, transform.position, transform.rotation);
        bullet.SetDirection(transform.forward);
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   CollisionsEnabled
    // Desc :   Moves the player from the player layer to the Default layer and back again. it is assumed collision
    //          matrix has been setup such that the default layer is not collidable
    //---------------------------------------------------------------------------------------------------------------
    protected void CollisionsEnabled(bool enable)
    {
        if (enable)
            gameObject.layer = LayerMask.NameToLayer("Player");
        else
            gameObject.layer = LayerMask.NameToLayer("Default");
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   EnableRenderers
    // Desc :   Enables or Disables rendering pf the player ship
    //---------------------------------------------------------------------------------------------------------------
    protected void RenderingEnabled(bool enable)
    {
        foreach (Renderer r in _renderers)
        {
            r.enabled = enable;
        }
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   OnCollisionEnter
    // Desc :   Called by unity when this object collides with another collider
    //---------------------------------------------------------------------------------------------------------------

    protected void OnCollisionEnter(Collision collision)
    {
        // Move the explosion particle effect game object to the player's position by 20 units towards the camera
        // so it appear in front
        _explosionSystem.transform.position = new Vector3(transform.position.x, 20,transform.position.z);

        // Activate the game object some of the particle effects starts playing
        _explosionSystem.SetActive(true);
        
        // If we have more lives left, reste the player ship for next life in 4 seconds
        if( !GameSceneManager.instance.PlayerDestroyed())
        {
            StartCoroutine(Reset(4));
        }
        else
        {
            RenderingEnabled(false);
            CollisionsEnabled(false);
            _inputDisabled = true;
        }
    }
    //---------------------------------------------------------------------------------------------------------------
    // Name :   HyperspaceEffect
    // Desc :   Helper Function that is called during HyperSpace AND Player reset to play the Hyperspace particle
    //          effect at the players postion, enabling/disabling player collisions and rendering and destroying
    //          any objects in the sacene that fall within the radius of the Hyperspace wake
    //---------------------------------------------------------------------------------------------------------------
    protected void HyperspaceEffect(bool arriving)
    {
        // Move the Hyperspace efffect to the players position (but 20 units above it, it always renders infront
        // of everything from the cameras Perspective) And activate the effect
        _hyperspaceEffect.transform.position = new Vector3(transform.position.x, 20, transform.position.z);
        _hyperspaceEffect.SetActive(true);

        // Disable pr enable the rendering of the player's ship and disable collisions depending on whether we are 
        // arriving or departing
        RenderingEnabled(arriving);
        CollisionsEnabled(arriving);

        // at the location of the Hyperspace... Simulate a destructive wale by casting a sphere in 20 unit radius around
        // the effect location and tell each object to destroy itself
        Collider[] colliders = Physics.OverlapSphere(transform.position, _hyperspaceDestructionRadius, LayerMask.GetMask("Alien Bullet", "Alien", "Asteroid"));
        foreach (Collider collider in colliders)
        {
            IPlayerKillable killableThing = collider.GetComponent<IPlayerKillable>();
            if (killableThing != null)
            {
                killableThing.Kill(true);
            }
        }
    }


    //---------------------------------------------------------------------------------------------------------------
    // Name :   Hyperspace
    // Desc :   You're one and only 'Get Out Of Jail' feature per level. Randomly teleport somewhere else
    //---------------------------------------------------------------------------------------------------------------

    protected IEnumerator Hyperspace()
    {
        // We are currently Hyperspacing
        _isHyperSpacing = true;

        // Inform  Game Scene manager we have consumed one of our hyperspaces
        GameSceneManager.instance.HyperspaceConsumed();

        // Play the Hyperspace sound
        _audioSource.PlayOneShot(_hyperspaceSound);

        // wait for the correct offset into the sound where the hyperspace effect should begin playing
        yield return new WaitForSeconds(_hyperspaceAudioOffset);

        // Disable input
        _inputDisabled = true;

        // Play Hyperspace effect passing false so player is disabled from both rendering and collisions... this is
        // a departure (we are jumping OUT)
        HyperspaceEffect(false);

        // Wait until the hyperspace effect has played out and disabled itself
        while (_hyperspaceEffect.activeInHierarchy)
            yield return null;

        // Wait for a further 2 seconds (Wondering where we will re-appear)
        yield return new WaitForSeconds(2);

        // Play hyperspace sound again
        _audioSource.PlayOneShot(_hyperspaceSound);

        // Wait for the correct offset time into the sound before playing the particle effect again
        yield return new WaitForSeconds(_hyperspaceAudioOffset);

        // Calculate a new random position that is on screen and not too close to the edge. First get the half screen size
        // (or center point of the screeen)
        Vector2 halfScreenSize = new Vector2(Screen.width / 2, Screen.height / 2);

        // Generate a random 2D position in a circle with ald size radius and then add the half size raidus to put the
        // center of the the circle at the center of the screen
        Vector2 screenPos = (Random.insideUnitCircle.normalized * halfScreenSize) + halfScreenSize;

        // Create a new 3D screen position that has the offset from the camera in the Z component. This is needed
        // to convert this 2D screen position into a 3D world position
        Vector3 position = new Vector3(screenPos.x, screenPos.y,_camera.transform.position.y);

        // Convert to world space and assign to our transform. I scale position by 0.75 so we are not too close to edge
        // of screen (no more than 75% out form centr of screen
        transform.position = _camera.ScreenToWorldPoint(position) * 0.75f;

        // Hyperspace Effect again but this time passing true so we re-appear (Arrival warp)
        HyperspaceEffect(true);

        // Re-enable input
        _inputDisabled = false;

        // wait until the hyperspace effect has played out and disabled itself
        while (_hyperspaceEffect.activeInHierarchy)
            yield return null;

        // We are no longer hyperspacing so feature can be used again at some point
        _isHyperSpacing = false;

    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Reset
    // Desc :   Called at Startup and to Rest the player when they lose a life.
    //---------------------------------------------------------------------------------------------------------------

    protected IEnumerator Reset(float delay = 0)
    {
        // Disable Input, Collisions and renderers we should be initially invisible
        _inputDisabled = true;
        CollisionsEnabled(false);
        RenderingEnabled(false);

        // Re-position the player at the center of the sceeen / world
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        // Wait one and a half seconds
        yield return new WaitForSeconds(1.5f);

        // The instruct game scene manager to display Get Ready message
        GameSceneManager.instance.DisplayMessage("GET READY");

        // Wait for the delay minus the hyperspace audio offset becase we need the sound to play but BEFORE we play the hyperspace effect
        yield return new WaitForSeconds(delay - _hyperspaceAudioOffset);

        // play Hyperspace audio
        _audioSource.PlayOneShot(_hyperspaceSound);

        // Now clear the Get Ready message
        GameSceneManager.instance.DisplayMessage("");

        // HyperSpace the player into view
        HyperspaceEffect(true);


        // Renable normal input processing
        _inputDisabled = false;
        

    }
}
