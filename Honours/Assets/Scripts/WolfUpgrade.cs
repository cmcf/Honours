using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WolfUpgrade : MonoBehaviour
{
    bool playerInRange = false;
    InputAction interactAction;

    public GameObject promptPrefab;
    GameObject controlPromptInstance;

    public float cooldownDecreaseAmount = 5f;   
    public float durationIncreaseAmount = 2f;

    public TMP_FontAsset customFont;

    void Start()
    {
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        interactAction = playerInput.actions["Interact"];

        if (promptPrefab != null)
        {
            promptPrefab.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && interactAction.triggered)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                Switcher switcher = FindObjectOfType<Switcher>();
                if (switcher != null)
                {
                    string message = "";

                    int choice = Random.Range(0, 2);
                    if (choice == 0)
                    {
                        switcher.wolfCooldownTime = Mathf.Max(1f, switcher.wolfCooldownTime - cooldownDecreaseAmount);
                        message = "WOLF COOLDOWN DECREASED!";
                    }
                    else
                    {
                        switcher.wolfFormDuration += durationIncreaseAmount;
                        message = "WOLF DURATION INCREASED!";
                    }

                    ShowWolfUpgradeMessage(message);
                }

                Destroy(controlPromptInstance);
                Destroy(gameObject);
            }
        }
    }

    void ShowWolfUpgradeMessage(string messageText)
    {
        Vector3 messagePosition = transform.position + Vector3.up * 0.5f;

        GameObject message = new GameObject("WolfUpgradeMessage");

        TextMeshPro textMeshPro = message.AddComponent<TextMeshPro>();

        if (customFont != null)
        {
            textMeshPro.font = customFont;
        }

        textMeshPro.text = messageText;
        textMeshPro.fontSize = 2;
        textMeshPro.color = Color.white;
        textMeshPro.alignment = TextAlignmentOptions.Center;

        message.transform.position = messagePosition;

        Destroy(message, 1.5f);
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

            if (controlPromptInstance != null)
            {
                Destroy(controlPromptInstance);
            }
        }
    }
}
