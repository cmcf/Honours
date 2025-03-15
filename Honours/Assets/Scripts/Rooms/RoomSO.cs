using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Room/RoomData", order = 1)]
public class RoomSO : ScriptableObject
{
    public string roomName;       // Name of the room
    public GameObject roomPrefab; // Prefab of the room
    public string description;    // Optional: Description of the room's purpose or mechanics
}
