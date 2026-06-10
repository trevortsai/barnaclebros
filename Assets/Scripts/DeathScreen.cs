using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Fades a full-screen opaque panel in when the diver dies, showing "You Died",
// the number of barnacles collected, and a reset button that restarts the level.
public class DeathScreen : MonoBehaviour
{
    [Header("References")]
    public DiverDeath diver;
    public CanvasGroup canvasGroup;
    public Text barnaclesText;

    [Header("Settings")]
    [Tooltip("Delay after death before the screen starts fading in (lets the death animation play).")]
    public float fadeDelay = 1.5f;
    public float fadeDuration = 1.5f;
    public string barnaclesFormat = "You scraped {0} barnacles";

    private bool _shown;

    void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    void Update()
    {
        if (!_shown && diver != null && diver.IsDead)
        {
            _shown = true;
            StartCoroutine(ShowSequence());
        }
    }

    IEnumerator ShowSequence()
    {
        if (fadeDelay > 0f)
            yield return new WaitForSeconds(fadeDelay);

        if (barnaclesText != null)
        {
            int collected = GameManager.instance != null ? GameManager.instance.RemovedBarnacles : 0;
            barnaclesText.text = string.Format(barnaclesFormat, collected);
        }

        // Free the cursor so the reset button is clickable.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float e = 0f;
        while (e < fadeDuration)
        {
            e += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Clamp01(e / fadeDuration);
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    // Hooked to the reset button's OnClick — restarts the level from the beginning.
    public void ResetGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
