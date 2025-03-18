using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRateManager : MonoBehaviour
{
    public static SpawnRateManager Instance;

    public int minAmountOfEnemies = 1;
    public int maxAmountOfEnemies = 1;

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

    public void IncreaseMinAmountOfEnemies()
    {
        // Increases the amount of enemies
        minAmountOfEnemies++;
        maxAmountOfEnemies++;

        // Caps the amount of enemies
        minAmountOfEnemies = Mathf.Min(minAmountOfEnemies, 4); 
        maxAmountOfEnemies = Mathf.Min(maxAmountOfEnemies, 5); 
    }
}
