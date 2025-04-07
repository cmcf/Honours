using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    public int baseDifficultyLevel = 0;  
    public int currentDifficulty;
    public bool hardModeEnabled = false;
    [SerializeField] int damageTakenThisRoom = 0;

    int minDifficulty = 2;
    int maxDifficulty = 6;
    public int difficultyIncreaseRate = 1;
    public int maxDeathsBeforeLowering = 3;

    public float damageThreshold = 0.5f;
    int decreaseDamageAmount = 30;
    int increaseDamageAmount = 40;


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

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (hardModeEnabled)
        {
            // Increase the starting diffiuclty level if hard mode is enabled
            currentDifficulty = baseDifficultyLevel + 1;
        }
        else
        {
            // Initialize the current difficulty level to the base level
            currentDifficulty = baseDifficultyLevel;
        }

        // Hard mode min and max difficulty is increased
        if (hardModeEnabled)
        {
            minDifficulty += 2;
            maxDifficulty += 2;
        }
    }

    public int GetCurrentDifficultyLevel()
    {
        return currentDifficulty;
    }

    public void RegisterDamageTaken(int damage)
    {
        damageTakenThisRoom += damage;
    }

    public void OnRoomStart()
    {
        damageTakenThisRoom = 0;
    }

    public void IncreaseDifficulty()
    {
        if (currentDifficulty < maxDifficulty)
        {
            currentDifficulty++;
            ApplyDifficultyScaling();
        }
        Debug.Log("Increased level - current diffiuclty is " + currentDifficulty);
    }

    public void DecreaseDifficulty()
    {

        if (currentDifficulty > minDifficulty)
        {
            currentDifficulty--;
        }
        Debug.Log("Difficulty decreased to: " + currentDifficulty);
    }

    public void AdjustDifficultyAfterRoom()
    {
        // Gradually scale thresholds based on current difficulty
        int increaseThreshold = 40 - (currentDifficulty * 2);
        int decreaseThreshold = 30 + (currentDifficulty * 3);

        // Clamp to reasonable ranges
        if (increaseThreshold < 10)
        {
            increaseThreshold = 10;
        }

        if (decreaseThreshold > 60)
        {
            decreaseThreshold = 60;
        }

        if (damageTakenThisRoom < increaseThreshold)
        {
            if (currentDifficulty < maxDifficulty)
            {
                IncreaseDifficulty();
            }
        }
        else if (damageTakenThisRoom > decreaseThreshold && currentDifficulty > minDifficulty)
        {
            DecreaseDifficulty();
        }
        // Reset room damage taken
        damageTakenThisRoom = 0;
    }

    void ApplyDifficultyScaling()
    {
        float scalingFactor;

        if (hardModeEnabled)
        {
            scalingFactor = 0.15f;
        }
        else
        {
            scalingFactor = 0.1f;
        }
        // Increase enemy spawn rate
        SpawnRateManager.Instance.IncreaseAmountOfEnemies(hardModeEnabled);
        float healthMultiplier = 1.0f + (scalingFactor * (currentDifficulty - 1));

        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.UpdateHealthScaling(healthMultiplier);
        }
    }


    public bool IsChallengeMode()
    {
        return hardModeEnabled;
    }

    public void ResetDifficultyLevel()
    {
        currentDifficulty = baseDifficultyLevel;
        SpawnRateManager.Instance.ResetSpawnerValues();
    }
}
