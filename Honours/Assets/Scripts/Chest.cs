using UnityEngine.InputSystem;
using UnityEngine;

public class Chest : MonoBehaviour
{
    Animator animator;
    public Room room;
    bool playerInRange = false;
    public bool enemiesDefeated = false;

    public bool isRewardRoom = false;
    InputAction interactAction;

    void Awake()
    {
        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.Enable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player is in range of the chest.");

            if (isRewardRoom)
            {
                animator.SetTrigger("openChest");
            }
            else if (!isRewardRoom && room.AreAllEnemiesDefeated())
            {
                enemiesDefeated = room.AreAllEnemiesDefeated();
                if (enemiesDefeated && playerInRange)
                {
                    animator.SetTrigger("openChest");
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player left the chest's range.");
        }
    }

    public void SetEnemiesDefeated(bool status)
    {
        enemiesDefeated = status;
        // Only attempt to open chest if player is in range
        if (enemiesDefeated && playerInRange)
        {
            animator.SetTrigger("openChest");
        }
    }

    void OpenChest()
    {
        Debug.Log("Chest Opened!");
    }
}
