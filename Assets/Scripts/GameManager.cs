using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private int totalBarnacles;
    private int removedBarnacles = 0;

    public Slider progressBar;
    public TMP_Text counterText;
    public GameObject winScreen;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        totalBarnacles = FindObjectsByType<Barnacle>(
            FindObjectsSortMode.None
        ).Length;

        Debug.Log("Barnacles found: " + totalBarnacles);

        progressBar.maxValue = totalBarnacles;
        progressBar.value = 0;

        UpdateCounter();

        winScreen.SetActive(false);
    }

    public void BarnacleRemoved()
    {
        removedBarnacles++;

        progressBar.value = removedBarnacles;

        UpdateCounter();

        if (removedBarnacles >= totalBarnacles)
        {
            WinGame();
        }
    }

    void UpdateCounter()
    {
        if (counterText != null)
        {
            counterText.text = removedBarnacles + " / " + totalBarnacles;
        }
    }

    void WinGame()
    {
        winScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }
}