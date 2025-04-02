using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    public int baseDifficultyLevel = 0;  
    public int currentDifficulty;
    public bool isChallengeMode = false;
    [SerializeField] int damageTakenThisRoom = 0;

    int minDifficulty = 2;
    int maxDifficulty = 6;
    public int difficultyIncreaseRate = 1;
    public int maxDeathsBeforeLowering = 3;

    float roomStartHealth;

    public float damageThreshold = 0.5f;
    int damageAmountBeforeAdjustment = 40;


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
        // Initialize the current difficulty level to the base level
        currentDifficulty = baseDifficultyLevel;
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
            ApplyDifficultyScaling();
        }
        Debug.Log("Difficulty decreased to: " + currentDifficulty);
    }

    public void AdjustDifficultyAfterRoom()
    {
        if (damageTakenThisRoom < damageAmountBeforeAdjustment) 
        {
            // Increase difficulty if little damage was taken
            if (currentDifficulty < maxDifficulty)
            {
                IncreaseDifficulty(); 
            }
        }
        else if (damageTakenThisRoom > damageAmountBeforeAdjustment && currentDifficulty > minDifficulty)
        {
            // Decrease difficulty if too much damage was taken
            DecreaseDifficulty(); 
        }

        damageTakenThisRoom = 0; // Reset for the next room
    }

    void ApplyDifficultyScaling()
    {
        // Increase enemy spawn rate
        SpawnRateManager.Instance.IncreaseAmountOfEnemies();
        float healthMultiplier = 1.0f + (0.1f * (currentDifficulty - 1));

        // Notify all enemies to update their health
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.UpdateHealthScaling(healthMultiplier);
        }
    }

    
    public void OnRoomStart(float playerHealth)
    {
        roomStartHealth = playerHealth;
    }

    public void OnRoomEnd(float playerHealth)
    {
        float healthLost = roomStartHealth - playerHealth;
        if (healthLost / roomStartHealth >= damageThreshold)
        {
            DecreaseDifficulty();
        }
    }

    public bool IsChallengeMode()
    {
        return isChallengeMode;
    }

    public void ResetDifficultyLevel()
    {
        currentDifficulty = baseDifficultyLevel;
        SpawnRateManager.Instance.ResetSpawnerValues();
    }
}
