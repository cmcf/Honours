using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public enum RoomDirection
{
    Up,
    Down,
    Left,
    Right
}

public class RoomController : MonoBehaviour
{
    [Header("References")]
    public SceneTransition sceneTransition;
    public static RoomController Instance;
    public PlayerHealth playerHealth;
    public GameObject bossHealthBarUI;

    [Header("Weapons")]
    public List<Weapon> availableWeapons;

    [Header("Rooms")]
    public List<RoomSO> availableRooms;
    public List<RoomSO> advancedRooms;
    List<RoomSO> usedRooms = new List<RoomSO>();

    public RoomSO bossRoom;
    public RoomSO advancedBossRoom;
    public RoomSO spawnRoom;
    public RoomSO rewardRoom;
    public Room currentRoom;
    GameObject previousRoom;
    GameObject nextRoom; // Store next room reference

    [Header("Room position")]
    public Vector3 currentRoomPosition; // Track the position of the last room
    public RoomDirection currentDirection; // Track the current direction to spawn rooms in
    Door.DoorType lastUsedDoor;

    public int roomsCompleted = 0;
    [SerializeField] int roomsBeforeBoss = 6;
    bool leftSpawnRoom = false;
    bool hasBossRoomSpawned = false;
    public bool startBossAttack = false;

    public bool canSpawnUpgrade = true;
    public float healthThreshold = 0.4f; // 40% of max health

    void Awake()
    {
        Instance = this;

        bossHealthBarUI.SetActive(false);

        // Load the spawn room when the game starts
        LoadSpawnRoom();
    }

    void Update()
    {
        UpdateRooms();
    }

    void LoadSpawnRoom()
    {
        if (spawnRoom == null || spawnRoom.roomPrefab == null)
        {
            return;
        }

        // Instantiate the spawn room at a fixed position
        Room newRoom = Instantiate(spawnRoom.roomPrefab, Vector3.zero, Quaternion.identity).GetComponent<Room>();

        newRoom.roomSO = spawnRoom;
        newRoom.InitializeRoom(roomsCompleted);

        currentRoom = newRoom;
        currentRoomPosition = newRoom.transform.position;
        leftSpawnRoom = true;
    }

    public void SetLastUsedDoor(Door.DoorType doorType)
    {
        lastUsedDoor = doorType;
    }

    public void LoadNextRoom(Door door)
    {
        List<RoomSO> roomPool;

        // Check if it's time to spawn a reward room
        if ((roomsCompleted + 1) % 3 == 0 && rewardRoom != null && rewardRoom.roomPrefab != null)
        {
            if (playerHealth != null && playerHealth.currentHealth <= playerHealth.maxHealth * healthThreshold)
            {
                LoadRoom(rewardRoom, door.doorType);
                return;
            }
        }

        // Decide which pool to use based on difficulty level
        if (DifficultyManager.Instance.currentDifficulty >= 4 && advancedRooms != null && advancedRooms.Count > 0)
        {
            roomPool = advancedRooms;
        }
        else
        {
            roomPool = availableRooms;
        }

        if (roomPool.Count == 0)
        {
            return;
        }
   
        // Create a filtered pool that excludes already used rooms
        List<RoomSO> filteredPool = roomPool.FindAll(room => !usedRooms.Contains(room));

        if (filteredPool.Count == 0)
        {
            Debug.LogWarning("All rooms have been used");
            filteredPool = new List<RoomSO>(roomPool); // Reset if needed
            usedRooms.Clear(); 
        }

        RoomSO nextRoomSO = filteredPool[Random.Range(0, filteredPool.Count)];
        LoadRoom(nextRoomSO, door.doorType);
        usedRooms.Add(nextRoomSO);
    }




    public void StartRoomTransition(GameObject player)
    {
        Switcher switcher = FindObjectOfType<Switcher>();
        // Disable switching during the transition
        switcher.canSwitch = false;

        // Fade out first
        sceneTransition.FadeOut(() =>
        {
            // Destroy the previous room right when the fade out completes
            if (previousRoom != null)
            {
                Destroy(previousRoom);
            }

            // Ensure the player is placed at the correct spawn point
            PlacePlayerAtSpawnPoint(player, lastUsedDoor);

            // Fade in and make the next room active after the fade
            sceneTransition.FadeIn(() =>
            {
                // Ensures next room is active and ready after fade in
                if (nextRoom != null)
                {
                    nextRoom.SetActive(true);
                }

                // Spawn enemies and enable player movement
                if (currentRoom != null)
                {
                    currentRoom.SpawnEnemies();
                }

                if (switcher != null)
                {
                    switcher.EnableActiveCharacter();
                    if (switcher.currentCharacterState == CharacterState.Player)
                    {
                        EnableCharacterMovement(switcher.playerObject);
                    }
                    else if (switcher.currentCharacterState == CharacterState.Wolf)
                    {
                        EnableCharacterMovement(switcher.wolfObject);
                    }
                }
            });
        });
    }




    void PlacePlayerAtSpawnPoint(GameObject player, Door.DoorType lastUsedDoor)
    {
        if (player == null)
        {
            return;
        }
        // Spawns player at the correct spawn point in the room

        Transform spawnPoint = currentRoom.GetSpawnPoint(lastUsedDoor);
        if (spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
        }
        else
        {
            player.transform.position = currentRoom.transform.position; 
        }
    }


    void EnableCharacterMovement(GameObject character)
    {
        if (character != null)
        {
            // Re-enable PlayerInput
            PlayerInput input = character.GetComponent<PlayerInput>();
            if (input != null)
            {
                input.enabled = true;
            }

            // Re-enable movement script
            PlayerMovement movement = character.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.enabled = true;
            }

            Wolf wolfMovement = character.GetComponent<Wolf>();
            if (wolfMovement != null)
            {
                wolfMovement.enabled = true;
            }
        }
    }



    public void LoadRoom(RoomSO roomSO, Door.DoorType previousDoor)
    {
        if (leftSpawnRoom)
        {
            GameTimer.Instance.StartTimer();
        }
        
        if (roomsCompleted >= roomsBeforeBoss && !hasBossRoomSpawned)
        {
            ReplaceWithBossRoom(previousDoor);
            return;
        }

        if (roomSO == null || roomSO.roomPrefab == null)
        {
            return;
        }

        // Create new room and initialize it
        Room newRoom = Instantiate(roomSO.roomPrefab).GetComponent<Room>();
        newRoom.InitializeRoom(roomsCompleted);

        // Set position and update direction based on previous door
        Vector3 spawnPosition = GetSpawnPosition(previousDoor);
        newRoom.transform.position = spawnPosition;

        // Enable a single exit door that does not allow backtracking
        newRoom.EnableSingleExitDoor(previousDoor);

        previousRoom = currentRoom.gameObject;
        nextRoom = newRoom.gameObject;
        currentRoom = newRoom;
    }

    void ReplaceWithBossRoom(Door.DoorType previousDoor)
    {
        if (roomsCompleted >= roomsBeforeBoss && !hasBossRoomSpawned)
        {
            if (DifficultyManager.Instance.currentDifficulty >= 4 && advancedBossRoom != null && advancedBossRoom.roomPrefab != null)
            {
                // Spawn the advanced boss room if difficulty is 4 or higher
                SpawnAdvancedBossRoom(previousDoor);
            }
            else if (bossRoom != null && bossRoom.roomPrefab != null)
            {
                // Otherwise, spawn the regular boss room
                SpawnBossRoom(previousDoor);
            }
        }

        bossHealthBarUI.SetActive(true);
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bossMusic);
    }

    Vector3 GetSpawnPosition(Door.DoorType previousDoor)
    {
        Vector3 spawnPosition = currentRoom.transform.position;
        switch (previousDoor)
        {
            case Door.DoorType.left: spawnPosition.x -= currentRoom.width; break;
            case Door.DoorType.right: spawnPosition.x += currentRoom.width; break;
            case Door.DoorType.top: spawnPosition.y += currentRoom.height; break;
            case Door.DoorType.bottom: spawnPosition.y -= currentRoom.height; break;
        }
        return spawnPosition;
    }



    public void OnRoomCompleted()
    {
        roomsCompleted++;

        // Check if the current room is not the spawn room or the reward room before adjusting difficulty
        if (currentRoom != null && currentRoom.roomSO != rewardRoom && roomsCompleted > 2)
        {
            DifficultyManager.Instance.AdjustDifficultyAfterRoom();
        }
    }


    public bool DoesRoomExist(RoomSO roomSO)
    {
        return currentRoom != null && currentRoom.roomSO == roomSO;
    }

    public void OnEnterRoom(Room room)
    {
        currentRoom = room;
        
        StartCoroutine(RoomCoroutine());
    }


    IEnumerator RoomCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        UpdateRooms();
    }

    void UpdateRooms()
    {
        if (currentRoom != null)
        {
            currentRoom.CheckRoomCompletion();
        }
    }

    void SpawnBossRoom(Door.DoorType previousDoor)
    {
        if (bossRoom == null || bossRoom.roomPrefab == null)
        {
            Debug.LogError("Boss room or its prefab is missing!");
            return;
        }

        // Set the boss room to spawn where the next room should be
        Vector3 bossRoomPosition = GetSpawnPosition(previousDoor);

        Room newRoom = Instantiate(bossRoom.roomPrefab, bossRoomPosition, Quaternion.identity).GetComponent<Room>();
        newRoom.roomSO = bossRoom;
        newRoom.InitializeRoom(roomsCompleted);

        previousRoom = currentRoom.gameObject;
        nextRoom = newRoom.gameObject;
        currentRoom = newRoom;
        hasBossRoomSpawned = true;

        Invoke("StartBossBattle", 1f);
    }

    void SpawnAdvancedBossRoom(Door.DoorType previousDoor)
    {
        if (advancedBossRoom == null || advancedBossRoom.roomPrefab == null)
        {
            Debug.LogError("Advanced Boss room or its prefab is missing!");
            return;
        }

        // Set the advanced boss room to spawn where the next room should be
        Vector3 bossRoomPosition = GetSpawnPosition(previousDoor);

        Room newRoom = Instantiate(advancedBossRoom.roomPrefab, bossRoomPosition, Quaternion.identity).GetComponent<Room>();
        newRoom.roomSO = advancedBossRoom;
        newRoom.InitializeRoom(roomsCompleted);

        previousRoom = currentRoom.gameObject;
        nextRoom = newRoom.gameObject;
        currentRoom = newRoom;
        hasBossRoomSpawned = true;

        Invoke("StartBossBattle", 1f);
    }

    void StartBossBattle()
    {
        startBossAttack = true;
    }

}
