using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public TMP_Text timerText;
    [SerializeField] float timeElapsed;
    bool pauseTimer;

    public static GameTimer Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        timeElapsed = 0f;
        pauseTimer = true; 
    }

    void Update()
    {
        if (!pauseTimer)
        {
            timeElapsed += Time.deltaTime; // Increment the time by the time passed each frame
        }

        DisplayTime(timeElapsed);
    }

    void DisplayTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("TIME TAKEN: {0:00}:{1:00}", minutes, seconds); // Display in MM:SS format
    }

    public void StartTimer()
    {
        pauseTimer = false; // Unpause the timer
    }


    public void PauseTimer()
    {
        pauseTimer = true;
    }

}
