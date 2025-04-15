using UnityEngine;
using TMPro;
[CreateAssetMenu(fileName = "WeaponUpgrade", menuName = "Weapons/Upgrade")]
public class WeaponUpgrade : ScriptableObject
{
    public TMP_FontAsset customFont;
    public enum UpgradeType
    {
        IncreaseSpreadCount,
        IncreaseBulletSpeed,
        IncreaseDamage,
    }

    public UpgradeType upgradeType;

    // Upgrade parameters
    public int spreadIncreaseAmount;
    public float fireRateMultiplier = 1f;
    public float bulletSpeedMultiplier = 1f;
    public int damageMultiplier = 2;

    public void Apply(Weapon weapon, WeaponUpgradePickup pickup)
    {
        // Apply the upgrade effect to the weapon
        switch (upgradeType)
        {
            case UpgradeType.IncreaseSpreadCount:
                weapon.spreadCount += spreadIncreaseAmount;
                break;

            case UpgradeType.IncreaseDamage:
                weapon.minDamage += damageMultiplier;
                weapon.maxDamage += damageMultiplier;
                break;

            case UpgradeType.IncreaseBulletSpeed:
                weapon.bulletSpeed *= bulletSpeedMultiplier;
                break;
        }

        // Show the upgrade UI feedback above the pickup
        ShowUpgradeMessage(pickup);

        // Destroy the pickup object but keep the text visible
        Destroy(pickup.gameObject);
    }

    void ShowUpgradeMessage(WeaponUpgradePickup pickup)
    {
        // Adjust the position to be above the pickup
        Vector3 messagePosition = pickup.transform.position + Vector3.up * 0.5f;

        // Instantiate a new GameObject to hold the TMP Text
        GameObject message = new GameObject("UpgradeMessage");

        // Add the TextMeshPro component
        TextMeshPro textMeshPro = message.AddComponent<TextMeshPro>();

        // Set the custom font
        if (customFont != null)
        {
            textMeshPro.font = customFont;
        }

        // Set the message text
        textMeshPro.text = GetUpgradeMessage(); 

        // Adjust text properties
        textMeshPro.fontSize = 2; // font size
        textMeshPro.color = Color.white;

        // Set the position of the text above the pickup
        message.transform.position = messagePosition;

        // Set the text alignment to be centered
        textMeshPro.alignment = TextAlignmentOptions.Center;

        // Destroy the message after a delay
        Destroy(message, 1.5f);
    }


    // Generate the appropriate message based on the upgrade type
    string GetUpgradeMessage()
    {
        switch (upgradeType)
        {
            case UpgradeType.IncreaseSpreadCount:
                return "BULLETS INCREASED BY " + spreadIncreaseAmount + "!";
            case UpgradeType.IncreaseDamage:
                return "GUN DAMAGE INCREASED";
            case UpgradeType.IncreaseBulletSpeed:
                return "BULLET SPEED INCREASED";
            default:
                return "Upgrade applied!";
        }
    }
}
