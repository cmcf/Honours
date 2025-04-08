using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    PlayerHealth playerHealth;
    InputAction interactAction;

    public GameObject promptPrefab;
    GameObject promptInstance;
    [SerializeField] int  minHealAmount = 20;
    [SerializeField] int maxHealAmount = 50;
    int healAmount = 20;

    bool playerInRange = false;
    void Start()
    {
        healAmount = Random.Range(healAmount, maxHealAmount);
        playerHealth = FindObjectOfType<PlayerHealth>();
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        interactAction = playerInput.actions["Interact"];

        // Ensure the prompt starts hidden
        if (promptPrefab != null)
        {
            promptPrefab.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && interactAction.triggered)
        {
            playerHealth.Heal(healAmount);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptPrefab != null)
            {
                promptPrefab.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptPrefab != null)
            {
                promptPrefab.SetActive(false);
            }
        }
    }

}
