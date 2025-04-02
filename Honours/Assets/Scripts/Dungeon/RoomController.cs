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
    
    [Header("Weapons")]
    public List<Weapon> availableWeapons;

    [Header("Rooms")]
    public List<RoomSO> availableRooms;
    public RoomSO bossRoom;
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

    public float healthThreshold = 0.4f; // 40% of max health

    void Awake()
    {
        Instance = this;

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
    }

    public void SetLastUsedDoor(Door.DoorType doorType)
    {
        lastUsedDoor = doorType;
    }

    public void LoadNextRoom(Door door)
    {
        if (availableRooms.Count == 0)
        {
            return;
        }

        // Check if it's time to spawn a reward room
        if (roomsCompleted % 3 == 0 && rewardRoom != null && rewardRoom.roomPrefab != null)
        {
            // Check if player health is low
            if (playerHealth != null && playerHealth.currentHealth <= playerHealth.maxHealth * healthThreshold)
            {
                LoadRoom(rewardRoom, door.doorType);
                return;
            }
        }

        RoomSO nextRoomSO;
        // Loads a room from the list that is not the current room
        do
        {
            nextRoomSO = availableRooms[Random.Range(0, availableRooms.Count)];
        }
        while (nextRoomSO == currentRoom.roomSO && availableRooms.Count > 1);

        LoadRoom(nextRoomSO, door.doorType);
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
        else
        {
            Debug.Log(" skipping difficulty adjustment.");
        }
    }


    public bool DoesRoomExist(RoomSO roomSO)
    {
        return currentRoom != null && currentRoom.roomSO == roomSO;
    }

    public void OnEnterRoom(Room room)
    {
        currentRoom = room;
        if (room.isBossRoom)
        {
            Invoke(nameof(DelayedActivateBoss), 1.6f);
        }

        StartCoroutine(RoomCoroutine());
    }


    void DelayedActivateBoss()
    {
        ActivateBoss(currentRoom);
    }

    void ActivateBoss(Room bossRoom)
    {
        BossFormManager boss = bossRoom.GetComponentInChildren<BossFormManager>();
        if (boss != null)
        {
            boss.gameObject.SetActive(true);
            boss.StartBossBattle();
        }
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

    void SpawnRewardRoom()
    {
        if (rewardRoom == null || rewardRoom.roomPrefab == null)
        {
            return;
        }

        // Spawn position based on last used door direction
        Vector3 spawnRoomPosition = currentRoom.transform.position;

        switch (currentDirection) // Use the last movement direction
        {
            case RoomDirection.Up: spawnRoomPosition += new Vector3(0, currentRoom.height, 0); break;
            case RoomDirection.Down: spawnRoomPosition += new Vector3(0, -currentRoom.height, 0); break;
            case RoomDirection.Left: spawnRoomPosition += new Vector3(-currentRoom.width, 0, 0); break;
            case RoomDirection.Right: spawnRoomPosition += new Vector3(currentRoom.width, 0, 0); break;
        }

        // Instantiate the boss room
        Room newRoom = Instantiate(rewardRoom.roomPrefab, spawnRoomPosition, Quaternion.identity).GetComponent<Room>();
        newRoom.roomSO = rewardRoom;
        newRoom.InitializeRoom(roomsCompleted);

        // Set up previous and current room references
        previousRoom = currentRoom.gameObject;
        nextRoom = newRoom.gameObject;
        currentRoom = newRoom;
        currentRoomPosition = newRoom.transform.position;

        newRoom.gameObject.SetActive(true); 
    }

    void SpawnBossRoom()
    {
        if (bossRoom == null || bossRoom.roomPrefab == null)
        {
            Debug.LogError("Boss room or its prefab is missing!");
            return;
        }

        // Spawn position based on last used door direction
        Vector3 bossRoomPosition = currentRoom.transform.position;

        switch (currentDirection) // Use the last movement direction
        {
            case RoomDirection.Up: bossRoomPosition += new Vector3(0, currentRoom.height, 0); break;
            case RoomDirection.Down: bossRoomPosition += new Vector3(0, -currentRoom.height, 0); break;
            case RoomDirection.Left: bossRoomPosition += new Vector3(-currentRoom.width, 0, 0); break;
            case RoomDirection.Right: bossRoomPosition += new Vector3(currentRoom.width, 0, 0); break;
        }

        // Instantiate the boss room
        Room newRoom = Instantiate(bossRoom.roomPrefab, bossRoomPosition, Quaternion.identity).GetComponent<Room>();
        newRoom.roomSO = bossRoom;
        newRoom.InitializeRoom(roomsCompleted);
        newRoom.isBossRoom = true;

        // Set up previous and current room references
        previousRoom = currentRoom.gameObject;
        nextRoom = newRoom.gameObject;
        currentRoom = newRoom;
        currentRoomPosition = newRoom.transform.position;
        hasBossRoomSpawned = true;

        newRoom.gameObject.SetActive(true);

        Invoke("StartBossBattle", 0.5f);
    }



    void StartBossBattle()
    {
        startBossAttack = true;
    }

}
