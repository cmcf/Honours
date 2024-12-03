using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;  // Stores different type of enemies to spawn in room
    public Transform[] spawnPoints;    // Possible spawn locations
    public int minEnemies = 3;         
    public int maxEnemies = 5;         

    private int enemyCount;

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        enemyCount = Random.Range(minEnemies, maxEnemies + 1);

        // Spawn the enemies at random spawn points
        for (int i = 0; i < enemyCount; i++)
        {
            // Selects a random enemy type and spawn point
            int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length); 
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
            // Spawns the enemy at selected spawn point
            Instantiate(enemyPrefabs[randomEnemyIndex], spawnPoints[randomSpawnIndex].position, Quaternion.identity);
        }

        // Set the enemy count
        var enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.enemyCount = enemyCount;
        }
    }
}
