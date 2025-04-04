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

    public void IncreaseAmountOfEnemies()
    {
        // Increases the amount of enemies
        minAmountOfEnemies++;
        maxAmountOfEnemies++;

        // Increase maxAmountOfEnemiesInRoom by 2, and make sure it doesn't exceed the cap
        maxAmountOfEnemiesInRoom = Mathf.Min(maxAmountOfEnemiesInRoom + spawnMultiplier, maxEnemiesPerRoomCap);

        // Caps the amount of enemies
        minAmountOfEnemies = Mathf.Min(minAmountOfEnemies, 4);
        maxAmountOfEnemies = Mathf.Min(maxAmountOfEnemies, 5);
    }

    public void ResetSpawnerValues()
    {
      minAmountOfEnemies = 1;
      maxAmountOfEnemies = 1;
      maxAmountOfEnemiesInRoom = 2;
      spawnMultiplier = 2;
    }

}
