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
    public enum BossForm { Firebird, Panther }
    public BossForm currentForm = BossForm.Panther;

    BossEnemy enemy;

    void Start()
    {
        enemy = GetComponent<BossEnemy>();
        pantherScript= GetComponentInChildren<Panther>();

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
        // Allow random switching only if cooldown has passed
        if (Time.time - lastSwitchTime >= switchCooldown)
        {
            if (Random.Range(0f, 1f) < 0.05f) 
            {
                SwitchForm();
            }
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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.bossSwitch);
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

        if (lastPosition != null)
        {
            // Call ActivateForm with the new form and the position
            ActivateForm(currentForm, lastPosition);
        } 

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
            Panther pantherScript = pantherForm.GetComponent<Panther>();
            if (pantherScript != null)
            {
                StopAllCoroutines(); // Stop any lingering coroutines
                pantherScript.RestartBossRoutine(); // Restart Firebird's behaviour
            }
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

}
