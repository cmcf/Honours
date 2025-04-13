using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public void PlayGame()
    {
        // Load the difficulty selection scene 
        SceneManager.LoadScene("DifficultySelection");
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
}
