using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRateManager : MonoBehaviour
{
    public static SpawnRateManager Instance;

    public int minAmountOfEnemies = 1;
    public int maxAmountOfEnemies = 1;
    public int maxAmountOfEnemiesInRoom = 2;
    public int spawnMultiplier = 2;
    int maxEnemiesPerRoomCap = 8;
    int minEnemiesPerRoomCap = 6;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void IncreaseAmountOfEnemies(bool isHardMode)
    {
        // Set spawn rate boost based on difficulty mode
        int spawnRateBoost = 1;

        if (isHardMode)
        {
            spawnRateBoost = 2;
        }
        // Increase the amount of enemies based on the spawn rate amount
        minAmountOfEnemies += spawnRateBoost;
        maxAmountOfEnemies += spawnRateBoost;
        maxAmountOfEnemiesInRoom += spawnMultiplier * spawnRateBoost;

        // Do not allow max amount of enemies to increase above the cap
        if (maxAmountOfEnemiesInRoom > maxEnemiesPerRoomCap)
        {
            maxAmountOfEnemiesInRoom = maxEnemiesPerRoomCap;
        }

        // Clamps the min amount of enemies

        if (minAmountOfEnemies > minEnemiesPerRoomCap)
        {
            minAmountOfEnemies = minEnemiesPerRoomCap;
        }
    }

    public void ResetSpawnerValues()
    {
      minAmountOfEnemies = 1;
      maxAmountOfEnemies = 1;
      maxAmountOfEnemiesInRoom = 2;
      spawnMultiplier = 2;
    }

}
