using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor;
using static Player;

public class RoomInfo
{
    public string name;
    public int x;
    public int y;
}
public class RoomController : MonoBehaviour
{
    public static RoomController Instance;
    string currentWorldName = "Game";

    RoomInfo currentLoadRoomData;
    public Room currentRoom;
    Queue<RoomInfo> loadRoomQueue = new Queue<RoomInfo>();

    public List<Room> loadedRooms = new List<Room>();
    HashSet<Room> completedRooms = new HashSet<Room>();

    bool isLoadingRoom = false;
    bool hasSpawnedBossRoom = false;
    bool updatedRooms = false;

    int roomsCompleted = -1;
    const int roomsBeforeBoss = 2;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        UpdateRoomQueue();
    }

    void Start()
    {
        roomsCompleted = -1;
    }

    public void LoadRoom(string name, int x, int y)
    {
        if (DoesRoomExist(x, y))
        {
            return;
        }
        RoomInfo newRoomData = new RoomInfo();
        newRoomData.name = name;
        newRoomData.x = x;
        newRoomData.y = y;

        // Add room data to the queue 
        loadRoomQueue.Enqueue(newRoomData);
    }

    public void OnRoomCompleted()
    {
        roomsCompleted++;
        Debug.Log("Rooms Completed: " + roomsCompleted);
        // Ensure the condition triggers after rooms are complete
        if (roomsCompleted >= roomsBeforeBoss) 
        {
            StartCoroutine(SpawnBossRoom());
        }
    }

    IEnumerator LoadRoomRoutine(RoomInfo info)
    {
        string roomName = currentWorldName + info.name;
        AsyncOperation loadRoom = SceneManager.LoadSceneAsync(roomName, LoadSceneMode.Additive);
        while (loadRoom.isDone == false)
        {
            yield return null;
        }
    }

    public void RegisterRoom(Room room)
    {
        if (currentLoadRoomData == null)
        {
            return;
        }
        // Only spawn room if it does not already exist
        if (!DoesRoomExist(currentLoadRoomData.x, currentLoadRoomData.y))
        {

            room.transform.position = new Vector3(currentLoadRoomData.x * room.width, currentLoadRoomData.y * room.height, 0);

            room.x = currentLoadRoomData.x;
            room.y = currentLoadRoomData.y;
            room.name = currentWorldName + room.name + " " + room.x + ", " + room.y;
            room.transform.parent = transform;

            isLoadingRoom = false;

            if (loadedRooms.Count == 0)
            {
                CameraController.Instance.currentRoom = room;
            }

            loadedRooms.Add(room);
        }
        else
        {
            Destroy(room.gameObject);
            isLoadingRoom = false;
        }

    }

    public bool DoesRoomExist(int x, int y)
    {
        return loadedRooms.Find(item => item.x == x && item.y == y) != null;
    }

    public Room FindRoom(int x, int y)
    {
        return loadedRooms.Find(item => item.x == x && item.y == y);
    }

    public void OnEnterRoom(Room room)
    {
        CameraController.Instance.currentRoom = room;
        currentRoom = room;
        // Do not trigger random weapon in spawn room
        if (roomsCompleted >= 1)
        {
            // Trigger new weapon after entering a room
            TriggerRandomWeapon();
        }
  

        StartCoroutine(RoomCoroutine());

    }

    public void TriggerRandomWeapon()
    {
        // Randomly select a weapon type
        WeaponType newWeapon = (WeaponType)Random.Range(0, 4); // assuming 4 weapon types

        // Assign the selected weapon to the player
        Player player = FindObjectOfType<Player>(); // Assuming only one player
        player.currentWeaponType = newWeapon;

    }

    public IEnumerator RoomCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        UpdateRooms();
    }

    public string GetRandomRoomName()
    {
        string[] possibleRooms = new string[]
        {
            "Empty",
            "Default",
        };

        return possibleRooms[Random.Range(0, possibleRooms.Length)];
    }

    public void UpdateRooms()
    {
        foreach (Room room in loadedRooms)
        {
            bool isCurrentRoom = (currentRoom == room);
            // Checks if all enemies are defeated in the room
            bool enemiesDefeated = room.AreAllEnemiesDefeated();

            // Handle door activation
            foreach (Door door in room.GetComponentsInChildren<Door>())
            {
                Vector2Int doorPosition = door.GetGridPosition();
                Room adjacentRoom = FindRoom(doorPosition.x, doorPosition.y);

                // If no adjacent room exists, deactivate the door. Activate if so
                if (adjacentRoom == null)
                {
                    door.gameObject.SetActive(false);
                }
                else
                {
                    door.gameObject.SetActive(true);
                }


                if (door.wallCollider != null)
                {
                    Collider2D collider = door.wallCollider.GetComponent<Collider2D>();

                    if (collider != null)
                    {
                        // Allow the player to go through doors only if all enemies are defeated
                        if (isCurrentRoom && enemiesDefeated)
                        {
                            collider.enabled = false;
                        }
                        else
                        {
                            collider.enabled = true;
                        }
                    }

                }

                // Checks if the room is cleared
                if (isCurrentRoom && enemiesDefeated && !room.isCompleted)
                {
                    // Marks the room as completed
                    room.isCompleted = true;
                    OnRoomCompleted();
                }

                foreach (Collider2D collider in room.GetComponentsInChildren<Collider2D>())
                {
                    if (collider != null && collider.gameObject != null && !collider.gameObject.GetComponent<Door>())
                    {
                        if (isCurrentRoom)
                        {

                            collider.enabled = true;
                        }
                        else
                        {

                            collider.enabled = true;
                        }
                    }
                }

                foreach (Enemy enemy in room.GetComponentsInChildren<Enemy>())
                {
                    enemy.SetActiveState(isCurrentRoom);
                }
            }
        }
    }


    void UpdateRoomQueue()
    {
        if (isLoadingRoom)
        {
            return;
        }

        if (loadRoomQueue.Count == 0)
        {
            // Only spawn if all enemy rooms are cleared
            if (!hasSpawnedBossRoom && roomsCompleted >= roomsBeforeBoss) 
            {
                StartCoroutine(SpawnBossRoom());
            }
            // Removes any doors 
            else if (hasSpawnedBossRoom && !updatedRooms)
            {
                foreach (Room room in loadedRooms)
                {
                    room.RemoveUnConnectedDoors();
                }
                UpdateRooms();
                updatedRooms = true;
            }
            return;
        }

        currentLoadRoomData = loadRoomQueue.Dequeue();
        isLoadingRoom = true;

        StartCoroutine(LoadRoomRoutine(currentLoadRoomData));
    }

    IEnumerator SpawnBossRoom()
    {
        // Check if the boss room has already been spawned
        if (hasSpawnedBossRoom)
            yield break;

        hasSpawnedBossRoom = true;

        yield return new WaitForSeconds(0.5f);

        // Ensure the queue is empty before spawning the boss room.
        if (loadRoomQueue.Count == 0 && loadedRooms.Count > 0)
        {
            // Identify the last room to replace with the boss room
            Room lastRoom = loadedRooms.Last();
            Vector2Int tempRoom = new Vector2Int(lastRoom.x, lastRoom.y);

            // Remove the last room
            Destroy(lastRoom.gameObject);
            loadedRooms.Remove(lastRoom);

            // Load the boss room
            LoadRoom("Boss", tempRoom.x, tempRoom.y);
        }
    }

    private Vector3 GetRoomCentre(Room room)
    {
        return new Vector3(room.x * room.width, 0, room.y * room.height);
    }

    public Room GetRoomAtPosition(Vector3 position)
    {
        // Returns the rooms position
        foreach (Room room in loadedRooms)
        { 
            if (IsPositionWithinBounds(position))
            {
                return room;
            }
        }
        return null;
    }

    public bool IsPositionWithinBounds(Vector3 position)
    {
        // Checks if room is within bounds
        Vector3 roomCentre = GetRoomCentre(currentRoom);
        float halfWidth = currentRoom.width/ 2f;
        float halfHeight = currentRoom.height / 2f;

        return position.x >= roomCentre.x - halfWidth && position.x <= roomCentre.x + halfWidth &&
               position.y >= roomCentre.y - halfHeight && position.y <= roomCentre.y + halfHeight;
    }


}