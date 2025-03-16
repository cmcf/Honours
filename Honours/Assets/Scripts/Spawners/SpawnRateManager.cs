using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRateManager : MonoBehaviour
{
    public static SpawnRateManager Instance;

    public int minAmountOfEnemies = 1;
    public int maxAmountOfEnemies = 3;

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

    public void IncreaseSpawnRate()
    {
        minAmountOfEnemies++;
        maxAmountOfEnemies++;

        minAmountOfEnemies = Mathf.Min(minAmountOfEnemies + 1, 4); // Cap min
        maxAmountOfEnemies = Mathf.Min(maxAmountOfEnemies + 2, 5); // Cap max
    }
}
