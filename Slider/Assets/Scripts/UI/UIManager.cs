
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public bool isGamePaused;
    public bool isGameOver;

    public GameObject pausePanel;
    public UnityEngine.UI.Slider volumeSlider;
    public GameObject gameOverPanel;
    public TextMeshProUGUI timeText;

    public Animator mainMenuAnimator;

    private float oldTimeScale = 1f;

    private void Awake()
    {
        volumeSlider.value = AudioManager.GetVolume();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGameOver)
            {
                return;
            }
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        Time.timeScale = oldTimeScale;
        isGamePaused = false;
    }

    void PauseGame()
    {
        pausePanel.SetActive(true);
        oldTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isGamePaused = true;
    }

    public void OpenGameOver(float secondsAlive)
    {
        gameOverPanel.SetActive(true);
        oldTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isGameOver = true;

        int mins = (int)(secondsAlive) / 60;
        int secs = (int)(secondsAlive) % 60;
        timeText.text = mins + ":" + secs.ToString("00");
    }

    public void UpdateVolume(float value)
    {
        AudioManager.SetVolume(value);
    }

    public void StartMainMenuTransitionOut()
    {
        StartCoroutine(MainMenuTransitionOut());
    }

    private IEnumerator MainMenuTransitionOut()
    {
        mainMenuAnimator.SetBool("shouldExit", true);

        yield return new WaitForSeconds(2);

        LoadGameInit();
    }

    public void LoadGameInit()
    {
        ResumeGame();
        SceneManager.LoadScene("GameInit");
    }

    public void LoadGame()
    {
        ResumeGame();
        SceneManager.LoadScene("Game");
    }

    public void LoadMainMenu()
    {
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game!");
    }
}