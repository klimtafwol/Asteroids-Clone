using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CreditsSceneManager : MonoBehaviour
{
    // Inspector
    [SerializeField] protected RectTransform _creditsContainer;         // The Rect transform of the container we wish to scroll
    [SerializeField] protected Image _fade;                             // Black square image component for Fade-out effect
    [SerializeField] protected float _finalScrollPos;                   // The final Y position we wish the container to be at
    [SerializeField] protected float _creditsDuration;                  // The speed we wish it to take to get there
    [SerializeField] protected float _fadeDuration;                     // How long music and image fade effects should take

    // Internals
    protected AudioSource _audioSource;         // Audio Source playing the music
    protected float _audioVolume;               // Used to cache initial audio volume we would like it to fade into 
    protected Vector3 _initialScrollPos;        // Record the initial position of the container

    //------------------------------------------------------------------------------------------------
    // Name :  Start 
    // Desc :  Called by Unity prior to first update. We will initialize the sequence
    //------------------------------------------------------------------------------------------------

   void Start()
    {
        // Cache audio source Component
        _audioSource = GetComponent<AudioSource>();
        
        // Assume the current volume of audio source is the Volume we wish to fade into
        _audioVolume = _audioSource.volume;

        // Set it to zero initially, we want to fade music in
        _audioSource.volume = 0;

        // Record the initial position of the container
        _initialScrollPos = _creditsContainer.localPosition;

        // Start the scrolling credits coroutine AND the music fading Coroutine
        StartCoroutine(CreditsRoll());
        StartCoroutine(Music());
    }

    //------------------------------------------------------------------------------------------------
    // Name :   CreditsRoll (Coroutine)
    // Desc :   Animates the position of the credits container
    //------------------------------------------------------------------------------------------------

    IEnumerator CreditsRoll()
    {
        // Set initial timer to zero
        float timer = 0;
        bool canSkip = PlayerPrefs.GetInt("Can Skip Credits", 0) > 0;
        PlayerPrefs.SetInt("Can Skip Credits", 1);

        // Loops while timer is less than the duration of our scrolling sequence
        while (timer < _creditsDuration)
        {
            // Make a local copy of the initial position of the container
            Vector3 containerPos = _initialScrollPos;

            // Animate the Y position between the initial position and the final position using the 
            // time factor as our T value
            containerPos.y = Mathf.Lerp(_initialScrollPos.y, _finalScrollPos, timer / _creditsDuration);

            // Set the position of the credits container
            _creditsContainer.localPosition = containerPos;

            // Add time to our timer
            timer += Time.deltaTime;

            if (Input.anyKeyDown && canSkip)
            {
                Application.Quit();

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }

            // YIELD YIELD YIELD!
            yield return null;
        }
    }
    //------------------------------------------------------------------------------------------------
    // Name :   Music (coroutine)
    // Desc :   Animate the volume of the music over the sequence. Also, animates the oppacity of the
    //          fade object after the music has finished to create a fade to black
    //------------------------------------------------------------------------------------------------
    IEnumerator Music()
    {
        // set timer to zero
        float timer = 0;

        // Fade the music in over the fade duration
        while (timer <= _fadeDuration)
        {
            // Lerp Audio source volume from 0 to its initial cached volume over the fade duration
            _audioSource.volume = Mathf.Lerp(0,_audioVolume, timer / _fadeDuration);

            // accumulate time
            timer += Time.deltaTime;

            // YIELD
            yield return null;
        }

        // now just let the music play for the entire duration of the credits sequence minus the
        // music fade duration
        yield return new WaitForSeconds(_creditsDuration -_fadeDuration);

        // Okay, at this point the credits haved reached their final position so its time to fade our 
        // music and screen
        timer = 0;

        // cache a copy of the color of the fade image
        Color fadeColor = _fade.color;  

        // Iterate for the fade duration
        while (timer <= _fadeDuration)
        {
            // animate volume back to zero over the fade duration
            _audioSource.volume = Mathf.Lerp(0, _audioVolume,1 - (timer / _fadeDuration));

            // Animate the opacity of the fade image form 0 - 1 over the fade duration
            fadeColor.a = timer / _fadeDuration;

            // Re-set this modified color as the new color of our fade image
            _fade.color = fadeColor;

            // accumulate time
            timer += Time.deltaTime;


            // YIELD
            yield return null;
        }

        // quit the application
        Application.Quit ();

        // If we are running in the editor then Application.Quit has no effect so
        // we will tell the editor to exit playmode thus simulating a real quit
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

    }
}

