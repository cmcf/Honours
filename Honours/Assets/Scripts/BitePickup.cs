using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitePickup : MonoBehaviour
{
    // Array of possible weapons to randomly pick from
    public BiteModifier[] possibleModifiers;
    SpriteRenderer spriteRenderer;
    // The selected random weapon
    BiteModifier selectedBiteModifier;

    public float floatSpeed = 2f;
    public float floatAmount = 0.1f;

    private Vector3 startPos;

    void Start()
    {
        // Randomly select a weapon from the array when the pickup is created
        selectedBiteModifier = possibleModifiers[Random.Range(0, possibleModifiers.Length)];
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = selectedBiteModifier.biteSprite;

        startPos = transform.position;
    }

    void Update()
    {
        // Moves the pickup up and down smoothly
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = startPos + new Vector3(0, yOffset, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ensure the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            // Get the Switcher script to check the current character state
            Switcher switcher = FindObjectOfType<Switcher>();
            if (switcher == null)
            {
                return;
            }

            // Only allow pickup if the player is in wolf form
            if (switcher.currentCharacterState == CharacterState.Wolf)
            {
                other.GetComponent<Wolf>().EquipBiteEffect(selectedBiteModifier);

                // Destroy the pickup object after it is collected
                Destroy(gameObject);
            }
        }
    }


    public BiteModifier GetBiteModifier()
    {
        return selectedBiteModifier;
    }
}
