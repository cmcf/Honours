using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFormManager : MonoBehaviour
{
    public GameObject firebirdForm;
    public GameObject pantherForm;
    Panther pantherScript;
    public float switchCooldown = 10f; // Prevents instant switching
    float lastSwitchTime;
    public GameObject boss;
    float bossStartDelay = 0.2f;
    public enum BossForm { Firebird, Panther }
    public BossForm currentForm = BossForm.Firebird;

    BossEnemy enemy;

    void Start()
    {
        enemy = GetComponent<BossEnemy>();
        pantherScript= GetComponentInChildren<Panther>();
        boss.gameObject.SetActive(true);
    }

    public void StartBossBattle()
    {
        // Ensure both forms are assigned
        if (firebirdForm == null || pantherForm == null)
        {
            return;
        }

        // Randomly choose between Firebird or Panther form
        float randomValue = Random.value;
        if (randomValue > 0.5f)
        {
            currentForm = BossForm.Firebird;
        }
        else
        {
            currentForm = BossForm.Panther;
        }

        // Activate the chosen form
        ActivateForm(currentForm, transform.position);
    }



    void Update()
    {
        if (Time.time - lastSwitchTime < switchCooldown) return;

        // Random Switching After Some Time
        if (Random.Range(0f, 1f) < 0.02f) // ~1% chance per frame to switch
        {
            SwitchForm();
        }

        SwitchBasedOnHealth();

        if (RoomController.Instance.startBossAttack)
        {
            boss.gameObject.SetActive(true);
        }
    }

    void SwitchForm()
    {
        lastSwitchTime = Time.time;

        // Check if the forms are assigned to avoid NullReferenceException
        if (firebirdForm == null || pantherForm == null)
        {
            return;
        }

        // Get the current active form's position before switching
        Vector3 lastPosition;
        if (currentForm == BossForm.Firebird)
        {
            lastPosition = firebirdForm.transform.position;
        }
        else
        {
            lastPosition = pantherForm.transform.position;
        }

        // Swap to the other form
        if (currentForm == BossForm.Firebird)
        {
            currentForm = BossForm.Panther;
        }
        else
        {
            currentForm = BossForm.Firebird;
        }

        // Call ActivateForm with the new form and the position
        ActivateForm(currentForm, lastPosition);

        // Update animator and Rigidbody2D references
        GetCurrentAnimator();
        GetCurrentRb();
    }

    void ActivateForm(BossForm newForm, Vector3 position)
    {
        firebirdForm.SetActive(newForm == BossForm.Firebird);
        pantherForm.SetActive(newForm == BossForm.Panther);

        if (newForm == BossForm.Firebird)
        {
            firebirdForm.transform.position = position;
            Firebird firebirdScript = firebirdForm.GetComponent<Firebird>();
            if (firebirdScript != null)
            {
                StopAllCoroutines(); // Stop any lingering coroutines
                firebirdScript.RestartBossRoutine(); // Restart Firebird's behaviour
            }
        }
        else
        {
            pantherForm.transform.position = position;
            pantherScript.StartAttacking();
        }
    }


    public Animator GetCurrentAnimator()
    {
        if (currentForm == BossForm.Firebird)
            return firebirdForm.GetComponent<Animator>();
        else
            return pantherForm.GetComponent<Animator>();
    }

   public Rigidbody2D GetCurrentRb()
   {
        if (currentForm == BossForm.Firebird)
            return firebirdForm.GetComponent<Rigidbody2D>();
        else
            return pantherForm.GetComponent<Rigidbody2D>();
   }

    public BossForm GetCurrentForm()
    {
        return currentForm;
    }

    void SwitchBasedOnHealth()
    {
        if (enemy != null)
        {
            float healthPercent = enemy.GetHealthPercentage();

            // Health-Based Switching
            if (healthPercent <= 0.65f && currentForm == BossForm.Firebird)
                SwitchForm();
            else if (healthPercent <= 0.50f && currentForm == BossForm.Panther)
                SwitchForm();
            else if (healthPercent <= 0.25f && currentForm == BossForm.Firebird)
                SwitchForm();
        }
    }
}
