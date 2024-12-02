using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;  
    public int enemyCount;


    void Awake()
    {
        Instance = this;
    }

    public void SpawnEnemies(GameObject[] enemyPrefabs, Transform[] spawnPoints)
    {
        foreach (var spawnPoint in spawnPoints)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
            enemyCount++;
        }
    }

    public void OnEnemyDefeated()
    {
        // Decrease the count when an enemy is defeated
        enemyCount--; 
        // Next room is unlocked when there are no enemies left in the room
        if (enemyCount <= 0)
        {
            RoomManager.Instance.OnRoomCleared();
        }
    }
}
