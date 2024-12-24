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
        RoomController.Instance.LoadRoom("Default", 0, 0);
        foreach(Vector2Int roomLocation in rooms)
        {
            RoomController.Instance.LoadRoom(RoomController.Instance.GetRandomRoomName(), roomLocation.x, roomLocation.y);
        }
            
    }
}
