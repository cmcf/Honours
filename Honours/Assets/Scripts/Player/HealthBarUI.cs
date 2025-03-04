using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthFill; // Reference to the fill image

    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = GetComponentInParent<PlayerHealth>();

        if (playerHealth != null && healthFill != null)
        {
            UpdateHealthBar();
        }
    }

    void Update()
    {
        if (playerHealth != null && healthFill != null)
        {
            UpdateHealthBar();
        }
    }

    private void UpdateHealthBar()
    {
        healthFill.fillAmount = playerHealth.currentHealth / playerHealth.maxHealth;
    }
}
