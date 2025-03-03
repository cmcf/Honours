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
    SpawnRateManager spawnRate;
    string currentWorldName = "Game";
    public Weapon[] availableWeapons;

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

    bool leftSpawnRoom = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        UpdateRooms();
        UpdateRoomQueue();
    }

    [System.Obsolete]
    void Start()
    {
        roomsCompleted = -1;
        spawnRate = FindObjectOfType<SpawnRateManager>();
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
        leftSpawnRoom = true;
        roomsCompleted++;
        Debug.Log("Rooms Completed: " + roomsCompleted);

        // Increase enemy spawn rate
        if (SpawnRateManager.Instance != null)
        {
            spawnRate.IncreaseSpawnRate();
        }

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.IncreaseDifficulty();
        }

        // Ensure the condition triggers after rooms are complete
        if (roomsCompleted >= roomsBeforeBoss)
        {
            StartCoroutine(SpawnBossRoom());
        }

        // Spawn a weapon pickup in the current room
        currentRoom.SpawnPickups();
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
        // Only spawn enemies in the room if it is not the spawn or boss room
        if (room.isBossRoom)
        {
            Invoke(nameof(DelayedActivateBoss), 0.5f);
        }
        else
        {
            if (leftSpawnRoom)
            {
                currentRoom.SpawnEnemies();
            }
        }

        StartCoroutine(RoomCoroutine());

        if (roomsCompleted >= 1)
        {
            // Trigger new weapon after entering a room
            //TriggerRandomWeapon();
        }

    }

    void DelayedActivateBoss()
    {
        ActivateBoss(currentRoom);
    }

    void ActivateBoss(Room bossRoom)
    {
        // Find and activate the boss inside the room
        BossFormManager boss = bossRoom.GetComponentInChildren<BossFormManager>(true);
        if (boss != null)
        {
            boss.gameObject.SetActive(true);
            boss.StartBossBattle();
        }
    }


        public void TriggerRandomWeapon()
    {
        int randomIndex = Random.Range(0, availableWeapons.Length);
        Weapon selectedWeapon = availableWeapons[randomIndex];

        Debug.Log("Selected weapon: " + selectedWeapon.weaponName);
        Debug.Log("Weapon sprite: " + selectedWeapon.weaponSprite);

        Player player = FindObjectOfType<Player>();
        player.EquipWeapon(selectedWeapon);
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
        HandlesDoorFunctionality();
    }

    void HandlesDoorFunctionality()
    {
        foreach (Room room in loadedRooms)
            ChecksIfRoomIsCleared(room);
    }

    void ChecksIfRoomIsCleared(Room room)
    {
        bool isCurrentRoom = (currentRoom == room);
        bool enemiesDefeated = room.AreAllEnemiesDefeated();

        foreach (Door door in room.doorList)
        {
            Vector2Int doorPosition = door.GetGridPosition();
            Room adjacentRoom = FindRoom(doorPosition.x, doorPosition.y);

            // Disable door if no adjacent room
            if (adjacentRoom == null)
            {
                door.gameObject.SetActive(false);
            }
            else
            {
                // Enable door if adjacent room exists
                door.gameObject.SetActive(true);
            }

            if (door.wallCollider != null)
            {
                Collider2D collider = door.wallCollider.GetComponent<Collider2D>();

                if (collider != null)
                {
                    // If current room and enemies are defeated, allow passing
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
        }

        // Reset isCompleted when entering a new room but enemies are still alive
        if (isCurrentRoom && !enemiesDefeated)
        {
            room.isCompleted = false;
        }

        // Checks if the room is cleared
        if (isCurrentRoom && enemiesDefeated && !room.isCompleted)
        {
            Debug.Log("Room cleared: " + room.name);
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