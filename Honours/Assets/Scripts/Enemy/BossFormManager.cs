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
    enum BossForm { Firebird, Panther }
    BossForm currentForm = BossForm.Firebird;

    BossEnemy enemy;

    void Start()
    {
        enemy = GetComponent<BossEnemy>();
        pantherScript= GetComponentInChildren<Panther>();
        // Randomly start as Firebird or Panther
        //currentForm = (Random.value > 0.5f) ? BossForm.Firebird : BossForm.Panther;
        ActivateForm(currentForm, transform.position);
    }

    void Update()
    {
        if (Time.time - lastSwitchTime < switchCooldown) return;

        if (enemy != null)
        {
            float healthPercent = enemy.GetHealthPercentage();
        }

        // Proximity-Based Switching
        if (ShouldSwitchBasedOnPlayerDistance())
        {
            SwitchForm();
        }

        // Random Switching After Some Time
        if (Random.Range(0f, 1f) < 0.01f) // ~1% chance per frame to switch
        {
            //SwitchForm();
        }
    }

    bool ShouldSwitchBasedOnPlayerDistance()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);

        if (currentForm == BossForm.Firebird && distance < 3f) return true; // Player is too close
        if (currentForm == BossForm.Panther && distance > 6f) return true; // Player is too far

        return false;
    }

    void SwitchForm()
    {
        lastSwitchTime = Time.time;

        // Get the current active form's position before switching
        Vector3 lastPosition = (currentForm == BossForm.Firebird) ? firebirdForm.transform.position : pantherForm.transform.position;

        // Swap to the other form
        currentForm = (currentForm == BossForm.Firebird) ? BossForm.Panther : BossForm.Firebird;
        ActivateForm(currentForm, lastPosition);
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

    void TempUpdateCode()
    {
        if (enemy != null)
        {
            float healthPercent = enemy.GetHealthPercentage();

            // Health-Based Switching
            if (healthPercent <= 0.75f && currentForm == BossForm.Firebird)
                SwitchForm();
            else if (healthPercent <= 0.50f && currentForm == BossForm.Panther)
                SwitchForm();
            else if (healthPercent <= 0.25f && currentForm == BossForm.Firebird)
                SwitchForm();
        }
    }
}
