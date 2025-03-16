using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    public int baseDifficultyLevel = -1;  
    public int currentDifficultyLevel;

    public int difficultyIncreaseRate = 1; 

    void Awake()
    {
        // Ensures only one instance of DifficultyManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize the current difficulty level to the base level
        currentDifficultyLevel = baseDifficultyLevel;
    }

    public int GetCurrentDifficultyLevel()
    {
        return currentDifficultyLevel;
    }

    public void IncreaseDifficulty()
    {
        // Increase the difficulty level
        currentDifficultyLevel += difficultyIncreaseRate;

        // Cap difficulty
        if (currentDifficultyLevel > 10) 
        {
            currentDifficultyLevel = 10;
        }

        // Increase enemy spawn rate
        SpawnRateManager.Instance.IncreaseSpawnRate();

        Debug.Log("Current Difficulty Level: " + currentDifficultyLevel);
    }
}
