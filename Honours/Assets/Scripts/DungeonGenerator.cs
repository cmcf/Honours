using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public DungeonGenerationData dungeonGenerationData;

    List<Vector2Int> dungeonRooms;

    private void Start()
    {
        dungeonRooms = DungeonController.GenerateDungeon(dungeonGenerationData);
        SpawnRooms(dungeonRooms);
    }

    void SpawnRooms(IEnumerable<Vector2Int> rooms)
    {
        RoomController.Instance.LoadRoom("Main", 0, 0);
        foreach(Vector2Int roomLocation in rooms)
        {
            if (roomLocation == dungeonRooms[dungeonRooms.Count - 1] && !(roomLocation == Vector2Int.zero))
            {
                RoomController.Instance.LoadRoom("WinScene", roomLocation.x, roomLocation.y);
            }
            else
            {
                RoomController.Instance.LoadRoom("Start", roomLocation.x, roomLocation.y);
            }
            
        }
    }
}
