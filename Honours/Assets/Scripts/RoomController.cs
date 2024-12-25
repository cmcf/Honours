using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
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
    Room currentRoom;
    Queue<RoomInfo> loadRoomQueue = new Queue<RoomInfo>();

    public List<Room> loadedRooms = new List<Room>();

    bool isLoadingRoom = false;
    bool hasSpawnedBossRoom = false;
    bool updatedRooms = false;

    void Awake()
    {
        Instance = this;
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

        StartCoroutine(RoomCoroutine());

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

    void Start()
    {
        // LoadRoom("Main", 0, 0);
        //LoadRoom("Start", 1, 0);
        //LoadRoom("Start", -1, 0);
    }


    void Update()
    {
        UpdateRoomQueue();

    }

    public void UpdateRooms()
    {
        foreach (Room room in loadedRooms)
        {
            if (currentRoom != room)
            {
                MeleeEnemy[] enemies = room.GetComponentsInChildren<MeleeEnemy>();
                if (enemies != null)
                {
                    foreach (MeleeEnemy enemy in enemies)
                    {
                        enemy.notInRoom = true;
                        Debug.Log("Not in room");
                    }

                    foreach (Door door in room.GetComponentsInChildren<Door>())
                    {
                        door.doorCollider.SetActive(false);
                    }
                }
                else
                {
                    foreach (Door door in room.GetComponentsInChildren<Door>())
                    {
                        door.doorCollider.SetActive(false);
                    }
                }
            }
            else
            {
                MeleeEnemy[] enemies = room.GetComponentsInChildren<MeleeEnemy>();
                if (enemies.Length > 0)
                {
                    foreach (MeleeEnemy enemy in enemies)
                    {
                        enemy.notInRoom = false;
                        Debug.Log("In room");
                    }

                    foreach (Door door in room.GetComponentsInChildren<Door>())
                    {
                        door.doorCollider.SetActive(true);
                    }
                }
                else
                {
                    foreach (Door door in room.GetComponentsInChildren<Door>())
                    {
                        door.doorCollider.SetActive(false);
                    }
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
            if (!hasSpawnedBossRoom)
            {
                StartCoroutine(SpawnBossRoom());
            }
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
}