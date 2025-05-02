using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameObject pauseMenu;
    bool isPaused = false;

    void Start()
    {
        if (pauseMenu!= null)
        {
            pauseMenu.SetActive(false);
        }
        
    }
    public void PlayGame()
    {
        // Load the difficulty selection scene 
        SceneManager.LoadScene("DisfficultySelection");
    }

    public void ResumeGame()
    {
        // Resumes time and hides pause menu
        pauseMenu.SetActive(false);
        Time.timeScale = 1;

        // Enables input
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }

    }

    public void Normal()
    {
        // Set difficulty to normal
        DifficultyManager.Instance.hardModeEnabled = false;
        // Load the game scene 
        SceneManager.LoadScene("GameMain");
    }

    public void Hard()
    {
        // Set difficulty to normal
        DifficultyManager.Instance.hardModeEnabled = true;
        // Load the game scene 
        SceneManager.LoadScene("GameMain");
    }


    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        // Quit the game
        Application.Quit();

        Debug.Log("Quit Game");
    }

    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    void OnEnable()
    {
        var playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["Pause"].performed += OnPausePressed;
        }
    }

    void OnDisable()
    {
        var playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["Pause"].performed -= OnPausePressed;
        }
    }

    void OnPausePressed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            GameTimer.Instance.PauseTimer();
            pauseMenu.SetActive(true);
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            GameTimer.Instance.StartTimer();
            pauseMenu.SetActive(false);
            Cursor.visible = false;
        }

        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = !isPaused;
        }
    }



}
