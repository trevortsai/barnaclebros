using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int totalBarnacles;
    private int removedBarnacles = 0;

    public Slider progressBar;
    public GameObject winScreen;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
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