using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public List<Room> loadedRooms= new List<Room>();

    bool isLoadingRoom = false;

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
        string roomName = currentWorldName+ info.name;
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
            room.RemoveUnConnectedDoors();
        }
        else
        {
            Destroy(room.gameObject);
            isLoadingRoom = false;
        }
        
    }

    public bool DoesRoomExist(int x,  int y)
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

    void UpdateRoomQueue()
    {
        if (isLoadingRoom)
        {
            return;
        }

        if (loadRoomQueue.Count == 0) 
        {
            return;
        }

        currentLoadRoomData = loadRoomQueue.Dequeue();
        isLoadingRoom = true;

        StartCoroutine(LoadRoomRoutine(currentLoadRoomData));
    }
}