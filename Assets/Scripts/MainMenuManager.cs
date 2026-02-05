using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Inspector
    [SerializeField] protected Image _imageFade = null;         // Black Fullscreen UI image to use for fade in / out
    [SerializeField] protected float _fadeSpeed = 1.5f;         // Speed to fade

    // Internals
    protected AudioSource _audioSource;                         // Audio Source on THIS game object with music clip Assigned
    protected float _audioVolume;                               // Used to cache intitial volume

    //------------------------------------------------------------------------------------------------
    // Name :   Start
    // Desc :   Called by Unity prior to first Update
    //------------------------------------------------------------------------------------------------

    protected void Start()
    {
        // Make sure cursor is visible and unlocked because we might be arriving here from game scene
        // that made cursor invisible
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Make sure the Fade Quad is set to black anf fully opaque initially
        _imageFade.color = new Color(0, 0, 0, 1);

        // Cache Audio Source reference and cache initial volume as our target volume
        _audioSource = GetComponent<AudioSource>();
        _audioVolume = _audioSource.volume;

        // Start  the coroutine to fade the audio and visuals in
        StartCoroutine(Fade(true, _fadeSpeed));
    }

    //------------------------------------------------------------------------------------------------
    // Name :   Fade (Coroutine)
    // Desc :   Called to Fade In or Out the scene during transition
    //------------------------------------------------------------------------------------------------

    IEnumerator Fade(bool fadeIn, float speed)
    {
        // Set the source and target colors (opacity) of the Fade Image based on whether
        // we are fading in or out. "fade In" has aplha go from 1 to 0, "Fade Out" from 0 to 1
        Color targetColor = fadeIn ? new Color(0, 0, 0, 0) : new Color(0, 0, 0, 1);
        Color sourceColor = fadeIn ? new Color(0, 0, 0, 1) : new Color(0, 0, 0, 0);

        // iterate for the fade durations
        float timer = 0;
        while (timer <= _fadeSpeed)
        {
            // Set the alpha of the fade image to time factor (timer / speed)
            _imageFade.color = Color.Lerp(sourceColor, targetColor, timer / speed);

            // If we are fading in Lerp in the audio volume else Lerp out
            if (fadeIn )
            {
                _audioSource.volume  = Mathf.Lerp(0, _audioVolume, timer / speed);
            }
            else
            {
                _audioSource.volume = Mathf.Lerp(_audioSource.volume, 0, timer / speed);
            }

            // Acummulate time
            timer += Time.deltaTime;

            // YIELD YIELD YIELD!
            yield return null;  
        }
   
        // Fade is over so make sure it is set to target colors
        _imageFade.color = targetColor;
    }

    //------------------------------------------------------------------------------------------------
    // Name :   NewGame (UI Event Handler)
    // Desc :   Called when the user clicks the NewGame button on UI
    //------------------------------------------------------------------------------------------------

    public void NewGame()
    {
        // Make cursor invisible and lock it
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Start a coroutine to fade out this scene (half the fade in time works well)
        StartCoroutine(Fade(false, _fadeSpeed / 2));

        // Invoke a function to run once fade is complete to load the game scene
        Invoke("LoadGameScene", _fadeSpeed / 2);
    }

    //------------------------------------------------------------------------------------------------
    // Name :   LoadGameScene
    // Desc :   Loads the Game Scene :)
    //------------------------------------------------------------------------------------------------

    protected void LoadGameScene()
    {
        SceneManager.LoadScene("Main Game");
    }

    //------------------------------------------------------------------------------------------------
    // Name :   QuitGame (UI Event Handler)
    // Desc :   Called via UI button click when use click the Quit button
    //------------------------------------------------------------------------------------------------

    public void QuitGame()
    {
        // Make cursor invisible and lock it
        Cursor.visible=false;
        Cursor.lockState = CursorLockMode.Locked;

        // Fade out the scene
        StartCoroutine(Fade(false, _fadeSpeed / 2));

        // Invoke a function to load the closing credits scene once fast out is complete
        Invoke("LoadClosingCreditsScene", _fadeSpeed / 2);
    }

    //------------------------------------------------------------------------------------------------
    // Name :   LoadClosingCreditsScene
    // Desc :   Loads the close credits scene Wow!
    //------------------------------------------------------------------------------------------------

    protected void LoadClosingCreditsScene()
    {
        SceneManager.LoadScene("Closing Credits");
    }
}
