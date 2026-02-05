using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    static GameSceneManager _instance = null;
    static public GameSceneManager instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<GameSceneManager>();

            return _instance;
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    // Embedded Class - SpeedSettings
    // Contains settings fpr Asteroids
    [System.Serializable]
    protected class SpeedSettings
    {
        public float BaseSpeed = 13.0f;     // Base Speed fpr Asteroids at game start
        public float MaximumSpeed = 18.0f;       // Maximum Speed for Asteroids at game start
        public float BaseSpeedMultiplier = 1.1f;     // Growth Rate of Base Speed each level
        public float MaximumSpeedMultiplier = 1.15f;    // Growth Rate of Maximum Speed each level
    }

    // Embedded Class - MassSettings
    // Contains settings for asteroids
    [System.Serializable]

    protected class MassSettings
    {
        public float MinimumMass = 0.36f;                       // Minimum size of an asteroid at game start
        public float MaximumMass = 2.6f;                        // Maximum size of an asteroid at game start
        public float MinimumMassMultiplier = 0.9f;              // Shrink rate of minimum asteroid size each level
        public float MinimumMassLimit = 0.18f;                  // The absolute smallest an asteroid can be in the entire game
    }

    [System.Serializable]
    protected class AlienSettings
    {
        public Alien Alien;                         // Reference to the one and only disabled Alien ship in the scene
        public float SpawnTime = 40.0f;             // Frequency of alien ship appearance
        public float SpawnTimeMultiplier = 0.9f;    // Shrink rate each level of frequency of Alien ship appearance
        public float Range = 75.0f;                 // initial firing range to the Player
        public float RangMultiplier = 1.1f;         // Growth rate of firing range each level
        public float Speed = 5;                     // Initial movement speed of Alien ship
        public float SpeedMultiplier = 1.2f;        // Speed multiplier with each level
        public float SpeedHighLimit = 20;           // Maximum Speed limit the Alien can ever move. Gets CRAZY if there is no upper limit
        public float FireDelay = 2.0f;              // Initial delay between firing shot at player
        public float FireDelayMultiplier = 0.9f;    // Shrink Rate if the firing delay
        public float FireDelayLowLimit = 0.25f;     // Minimum time between shots
        public int Points = 500;                    // points for killing and Alien ship

    }

    // Inspector
    [Header("player")]
    [SerializeField] protected Player _player;           // Reference to player ship in scene
    [SerializeField] protected int _lives = 3;        // Initial lives the player has at game start

    [Header("Asteroid Settings")]
    [SerializeField] protected Asteroid[]       _asteroids;                                // An array of asteroid prefabs to use for spawning
    [SerializeField] protected int              _startSpawnAmount = 7;                     // Initial number of asteroids spawned in a scene
    [SerializeField] protected MassSettings     _massSettings = new MassSettings();        // Mass settings for asteroids
    [SerializeField] protected SpeedSettings    _speedSettings = new SpeedSettings();      // Speed Settings for asteroids
    [SerializeField] protected AudioClip[]      _explosionSounds;                          // An array of explosion sounds to be used when blown up
    [SerializeField] protected AudioClip[]      _collisionSounds;                          // An array of collision sounds to be used when asteroids collide

    [Header("Particle Effects")]
    [SerializeField] protected GameObject       _asteroidExplosion   = null;        // Game Object of effect ysed for exploding
    [SerializeField] protected ExplosionLight   _asteroidExplosionLight = null;     // A light used to temporarily light up the environment
    [SerializeField] protected GameObject       _asteroidCollision = null;          // Game object of effect used for colliding

    [Header("Alien Settings")]
    [SerializeField] protected AlienSettings  _alienSettings = new AlienSettings();     // Settings for Alien ship

    [Header("UI Settings")]
    [SerializeField] protected Text _messageUI;             // UI Element to display messages (such as "Get READY")
    [SerializeField] protected Text _scoreUI;               // UI Element to display score
    [SerializeField] protected Text _livesUI;               // UI Element to display remaining lives
    [SerializeField] protected Text _levelUI;               // UI Element to display Current Level number
    [SerializeField] protected Text _hyperspaceUI;          // UI Element to signify hyperspace is ready



    protected Camera _camera;                                   // Main Scene Camera
    protected int _asteroidsRemaining;                          // Number of asteroids remaining
    protected int _currentLevel = -1;                           // Current Level
    protected int _score;                                       // Current Score
    protected int _nextJumpScore = 3000;                        // Next Score to reach to gain another Hyperspace
    protected int _jumpsAvailable;                              // Are there jumps available
    protected ParticleSystem[] _asteroidExplosionSystems;       // Array of particle systems used by the Asteroid Explosion Effect   
    protected ParticleSystem[] _asteroidCollisionSystems;       // Array of particle systems used by the Asteroid Collision Effect
    protected AudioSource _audioSource;                         // Audio source on this game object used for lots of things :D
    protected float _nextAlienSpawnTime;                        // Next time an Alien can appear


    // Properties
    // Returns a reference to the player component in the scene
    public Player player
    {
        get { return _player; }
    }

    // Return a random asteroid Prefab from the array
    public Asteroid RandomAsteroid
    {
        get { return _asteroids[Random.Range(0, _asteroids.Length)]; }
    }

    // Get the base speed of an asteroid based on current level

    public float baseSpeed
    {
        get { return _speedSettings.BaseSpeed * Mathf.Pow(_speedSettings.BaseSpeedMultiplier, _currentLevel); }
    }

    // Get the maximum speed of an asteroid based on current level

    public float maximumAsteroidSpeed
    {
        get { return _speedSettings.MaximumSpeed * Mathf.Pow(_speedSettings.MaximumSpeedMultiplier, _currentLevel); }
    }

    // Get the minimum speed of an asteroid based on current level

    public float minimumAsteroidmass
    {
        get { return Mathf.Max(_massSettings.MinimumMass * Mathf.Pow(_massSettings.MinimumMassMultiplier, _currentLevel), _massSettings.MinimumMassLimit); }
    }

    public float maximumAsteroidMass
    {
        get { return _massSettings.MaximumMass; }
    }

    // Does the player have hyperspace available
    public bool hyperspaceAvailable
    {
        get
        {
            return _jumpsAvailable > 0;
        }
    }

    // Is Alien in rang eof the player based on current level
    public bool alienInRange
    {
        get
        {
            float distance = Vector3.Distance(_player.transform.position, _alienSettings.Alien.transform.position);
            float currentRange = _alienSettings.Range * Mathf.Pow(_alienSettings.RangMultiplier, _currentLevel);
            return distance <= currentRange;
        }
    }

    // Get the movement speed of the alien based on current level
    public float alienSpeed
    {
        get
        {
            return Mathf.Min(_alienSettings.Speed * Mathf.Pow(_alienSettings.SpeedMultiplier, _currentLevel), _alienSettings.SpeedHighLimit);
        }
    }

    // get the Alien fire delay based on current level
    public float alienFireDelay
    {
        get
        {
            return Mathf.Max(_alienSettings.FireDelay * Mathf.Pow(_alienSettings.FireDelayMultiplier, _currentLevel), _alienSettings.FireDelayLowLimit);
        }
    }

    // get the amount the alien can fire infront of the player based on level
    public float alienPrediction
    {
        get
        {
            return Mathf.Min(_currentLevel / 5.0f, 1.0f);
        }
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :    Start
    // Desc :    Called by Unity prior to first update
    //---------------------------------------------------------------------------------------------------------------

    protected void Start()
    {
        // Cache reference to Main camera in the scene
        _camera = Camera.main;

        // Cache all the particle systems that are children of the Asteroid Explosion and Collision Effects
        _asteroidExplosionSystems = _asteroidExplosion.GetComponentsInChildren<ParticleSystem>();
        _asteroidCollisionSystems = _asteroidCollision.GetComponentsInChildren<ParticleSystem>();

        // Cache all the references to this object's audio source
        _audioSource = GetComponent<AudioSource>();

        // Set the random seed to something I like
        Random.InitState(0);

        // Start New level
        NewLevel();

    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   NewLevel
    // Desc :   Initialize things for a new level
    //---------------------------------------------------------------------------------------------------------------

    protected void NewLevel()
    {
        // Increment the current level (this starts at -1 so will be increments to zero for first level
        _currentLevel++;

        // Set the next alien spawn time
        _nextAlienSpawnTime = Time.time + _alienSettings.SpawnTime; 

        // Spawn some random asteroids
        SpawnAsteroids (_startSpawnAmount);

        // Refresh the UI components
        InvalidateUI();

        // If this isn't the first level then we have just completed a level to display a message for three seconds
        if(_currentLevel != 0)
        {
            //Display the message 
            DisplayMessage("LEVEL COMPLETE");
            Invoke("ClearMessage", 3); 

            // Clear the message
        }

        // Increment  the start spawn amount so next level we will spawn one more asteroids
        _startSpawnAmount++;
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Update
    // Desc :   Called each frame by Unity
    //---------------------------------------------------------------------------------------------------------------
    protected void Update()
    {
        // If the alien is not currently enabled in the scene, then lets see if it is time for it to make another 
        // appearance

        if (!_alienSettings.Alien.gameObject.activeInHierarchy)
        {
            // Have we surpassed the next spawn time? if so, spawn the alien
            if (Time.time >= _nextAlienSpawnTime)
                SpawnAlien();
            
            // Otherwise, if the alien isn't enabled, check to see if all the asteroids have been destroyed and if so,
            // start a new level
            else
            if (_asteroidsRemaining <= 0)
                NewLevel();
        }
        
    }


    //---------------------------------------------------------------------------------------------------------------
    // Name :   Spawn Alien
    // Desc :   Position the Alien randomly offscreen and then enables it
    //---------------------------------------------------------------------------------------------------------------

    protected void SpawnAlien()
    {
        // Create a random screen position around the outside of the screen
        Vector2 halfScreenSize = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 screenPos = (Random.insideUnitCircle.normalized * halfScreenSize) + halfScreenSize;

        // Convert to world space
        Vector3 position = new Vector3(screenPos.x, screenPos.y, _camera.transform.position.y);

        // Scale by 1.2 to push it out of view a little bit initially
        Vector3 worldPos = _camera.ScreenToWorldPoint(position) * 1.2f;

        // Instruct the alien to enable itself and start searching for the player
        _alienSettings.Alien.Show(worldPos);

    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   AlienDestroyed
    // Desc :   this is called by the Alien component when it has been destroyed by the player
    //---------------------------------------------------------------------------------------------------------------
    public void AlienDestroyed()
    {
        // Calculate the next time the alien shouled be allowed to show up factoring in the current level
        _nextAlienSpawnTime = Time.time + (_alienSettings.SpawnTime * Mathf.Pow(_alienSettings.SpawnTimeMultiplier, _currentLevel));

        // as the alien becomes more challenging with each level, we award more points as the level increases
        _score += _alienSettings.Points * (_currentLevel + 1);

        // If the increase in score has surpassed the next jump score, then add another jump capability to the player
        // and icnrease the next jump score
        if (_score > _nextJumpScore)
        {
            _jumpsAvailable++;
            _nextJumpScore *= 2;
        }

        // as we haved added to the score recresh the UI to reflect this
        InvalidateUI();
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Spawn Asteroids
    // Desc :   Called at the beginning of each levle to spawn the requested amount of asteroids
    //          randomly around the outside of the viewable area
    //---------------------------------------------------------------------------------------------------------------
    protected void SpawnAsteroids (int amount)
    {
        // Get half the screen size
        Vector2 halfScreenSize = new Vector2(Screen.width / 2, Screen.height / 2);

        // Iterate for the amount of asteroids we wish to create
        for (int i = 0; i < amount; i++)
        {
            // Generate a random index into our array of asteroid prefabs
            int asteroidIndex = Random.Range(0, _asteroids.Length);

            // Generate a random vector around the OUTSIDE(normalized) of a Unit circle then
            // multiply by half screen size and add half screen size. This generate a random screen
            // position around the outside of the screen
            Vector2 screenPos = (Random.insideUnitCircle.normalized * halfScreenSize) + halfScreenSize;

            // Create a 3D vector that has the 2D screen coordinates in X and Y but has the distance
            // from the camer in Z (this is needed to conver into world space with a perspective camera
            Vector3 position = new Vector3(screenPos.x, screenPos.y, _camera.transform.position.y);

            // USe the camrea class to convert this screen position into a world position
            Vector3 worldPos = _camera.ScreenToWorldPoint(position);

            // Set the direction of the asteroid to the negative of its position so it travels
            // towarsd the center of screen initially
            Vector3 trajectory = -worldPos.normalized;

            // Initiate a random asteroid instance at the world space position. notice we are using the 
            // Random Index we generated to fetch the asteroid prefab from the array
            Asteroid asteroid = Instantiate(_asteroids[asteroidIndex], worldPos * 2.4f, Quaternion.identity);

            // Set the asteroid's properties so that is has a random mass / size and knows the direction
            // in which it should travel (Trajectory)
            asteroid.SetProperties(Random.Range(minimumAsteroidmass, maximumAsteroidMass), trajectory);
        }

    }
    //---------------------------------------------------------------------------------------------------------------
    // Name :    HyperspaceConsumed
    // Desc :   Called by the player component when the player activated a hyperspace. It decrements the players available hyperspace
    //---------------------------------------------------------------------------------------------------------------

    public void HyperspaceConsumed()
    {
        //
        if (_jumpsAvailable > 0)
        {
            _jumpsAvailable--;
            InvalidateUI();
        }
    }
   
    //---------------------------------------------------------------------------------------------------------------
    // Name :   AsteroidCreated
    // Desc :   Called by the ASteroid component each time a new asteroid is created
    //---------------------------------------------------------------------------------------------------------------
    public void AsteroidCreated()
    {
        // increment the number of asteroids in the scene
        _asteroidsRemaining++;
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   AsteroidDestroyed
    // Desc :   Called by an asteroid as it is destroyed
    //---------------------------------------------------------------------------------------------------------------
public void AsteroidDestroyed(float mass)
    {
        // Decrement the number of remaining
        _asteroidsRemaining--;

        // Score is basde on mass/size. smaller asteroids score more
        // as they are harder to hit. zero will be passed i the asteroid was not destroyed by the player
        if(mass > 0)
        {
            // multiply 1/mass by 100 so bigger asteroids generate a smaller score
            float temp = ((1.0f / mass) * 100);

            // Divide by 50, force to an integer, then multiply by 50, this means score will be in 50 increments. Wd dont want to award 46.7 points
            //So we did this to make sure points are either 50, 100, 150, 200, 250 based on mass
            temp = Mathf.Ceil(temp / 50.0f) * 50;

            // add to score 
            _score += (int)temp;

            // if the new score takes us passed our next jump score add another hyperspace and calculate next jump score
            if (_score > _nextJumpScore)
            {
                _jumpsAvailable++;
                _nextJumpScore *= 2;
            }

            // Refresh to the UI so reflect new score
            InvalidateUI();
        }
    }
   
    //---------------------------------------------------------------------------------------------------------------
    // Name :   PlayerDestroyed
    // Desc :   Called by the player component when the player is destroyed by an alien bullet or an asteroid
    //---------------------------------------------------------------------------------------------------------------
    public bool PlayerDestroyed()
    {
        // Decrement number of lives and invalidate UI
        _lives--;

        // Refresh to the UI so reflect new score
        InvalidateUI();

        // if lives has hit zero
        if (_lives <= 0)
        {
            // Display Game over message
            DisplayMessage("GAME OVER");
            

            // and load the Main menu scene in 5 seconds
            Invoke("LoadMainMenu", 5);
        }
        
        // Return the new number of lives, this is used by player script so for example if zero is returned it
        // knows not to respawn itself
        return _lives <= 0;
    }
    //---------------------------------------------------------------------------------------------------------------
    // Name :   PlayAsteroidExplosion
    // Desc :   Called by an asteroid when it has been destroyed. The Game Scene MAnager uses a single particle
    // system for all asteroids for efficiency
    //---------------------------------------------------------------------------------------------------------------
    public void PlayAsteroidExplosion(Vector3 position, float size)
    {
        // Calculate postion to places the partucle System. this is the asteroids position but moved up 20 on Y 
        // (towards the overhead camera) so particles appear infront of asteroids
        Vector3 overheadPosition = new Vector3(position.x, 20, position.z);

        // move the explosion transfomr to the new position and scale it based on size. Vector3.One is just a
        // vestor (1,1,1) so its shorthand for ascaling x,y,x and together. The *3 multiplier was gained from trial
        // and errore for what looked right
        _asteroidExplosion.transform.position = overheadPosition;
        _asteroidExplosion.transform.localScale = Vector3.one * size;

        // Iterate throa all particle systems in this effect and emit 10 particles from each emitter.
        // 10 looks about right during testing
        foreach (ParticleSystem system in _asteroidExplosionSystems)
            system.Emit(  (int)system.emission.rateOverTime.constant);

        // now move the explosion light to the same overhead position
        _asteroidExplosionLight.transform.position = overheadPosition;

        // and tell the light to enable itself. We pass in the range of the light so bigger asteroids and explosions
        // light up more of teh scene. the ExplosionLight component will turn itself off automatically have a short 
        // time
        _asteroidExplosionLight.ShowLight(75 * size);


        // PLay a random explosion sound from our array of explosion audio clips so not all explosions sound identical
        _audioSource.PlayOneShot(_explosionSounds[Random.Range(0, _explosionSounds.Length)], Mathf.Min(size, 1));

    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   PlayAsteroidCollision
    // Desc :   Called by an asteroid when it collides with (and bounces off) another asteroid. a single dust 
    //  particle effect is used for ALL asteroid collisions for efficiency
    //---------------------------------------------------------------------------------------------------------------

    public void PlayAsteroidCollision(Vector3 position, float size)
    {
        // Move the effect transform to just above the passed position (The asteroids position)
        Vector3 overheadPosition = new Vector3(position.x, 20, position.z);
        _asteroidCollision.transform.position = overheadPosition;

        // the Effect is scaled by size
        _asteroidCollision.transform.localScale = Vector3.one * size;


        // Emit particles from each system in thsi effect
        foreach( ParticleSystem system in _asteroidCollisionSystems)
            system.Emit( (int)system.emission.rateOverTime.constant);

        // Play random collision sound
        _audioSource.PlayOneShot(_collisionSounds[Random.Range(0,_collisionSounds.Length)], Mathf.Max(size / 5, 0.45f));
        
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   InvalidateUI
    // Desc :   Called when the UI elements need to have there contents refreches such as when the score changes, the
    //          player loses a life or the current leve changes.
    //---------------------------------------------------------------------------------------------------------------

    public void InvalidateUI()
    {
        // Set the score, live and current level in there respective UI text elements.
        _scoreUI.text = _score.ToString();
        _livesUI.text = _lives.ToString();
        _levelUI.text = "level : " + (_currentLevel + 1).ToString();

        // if the player has jumps available then enable the Hyperspace UI element
        // if not already enabled, other disable it if not already disabled
        if (_jumpsAvailable >  0 && !_hyperspaceUI.gameObject.activeInHierarchy)
            _hyperspaceUI.gameObject.SetActive(true);
        else
        if (_jumpsAvailable == 0 && _hyperspaceUI.gameObject.activeInHierarchy)
            _hyperspaceUI.gameObject.SetActive(false);
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Display Message
    // Desc :   Used to set the text of the Message UI element so we can display messages such as "Get Ready" or
    //          "Game Over"
    //---------------------------------------------------------------------------------------------------------------

    public void DisplayMessage(string message = "")
    {
        _messageUI.text = message;
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   Clear Message
    // Desc :   Sets the text of the message UI element to an empty string
    //---------------------------------------------------------------------------------------------------------------
    
    public void ClearMessage()
    {
        _messageUI.text = "";
    }

    //---------------------------------------------------------------------------------------------------------------
    // Name :   LoadMainMenu
    // Desc :   Loads the Main Menu scene;
    //---------------------------------------------------------------------------------------------------------------

    protected void LoadMainMenu ()
    {
        SceneManager.LoadScene("Main Menu");
    }

}

