using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private int totalBarnacles;
    private int removedBarnacles = 0;

    public int RemovedBarnacles => removedBarnacles;
    public int TotalBarnacles => totalBarnacles;

    public Slider progressBar;
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

        winScreen.SetActive(false);
    }

    public void BarnacleRemoved()
    {
        removedBarnacles++;

        progressBar.value = removedBarnacles;

        if (removedBarnacles >= totalBarnacles)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        winScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }
}