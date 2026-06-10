using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Fades a full-screen opaque panel in when every barnacle has been scraped,
// showing a win message, the barnacle count, and a play-again button.
public class WinScreen : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public Text barnaclesText;

    [Header("Settings")]
    [Tooltip("Delay after winning before the screen starts fading in.")]
    public float fadeDelay = 0.5f;
    public float fadeDuration = 1.5f;
    public string barnaclesFormat = "You scraped {0} barnacles and cleaned all the boats!";

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
        if (!_shown && GameManager.instance != null && GameManager.instance.HasWon)
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
            int total = GameManager.instance != null ? GameManager.instance.TotalBarnacles : 0;
            barnaclesText.text = string.Format(barnaclesFormat, total);
        }

        // Free the cursor so the button is clickable.
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

    // Hooked to the play-again button's OnClick — restarts the level from the beginning.
    public void ResetGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
