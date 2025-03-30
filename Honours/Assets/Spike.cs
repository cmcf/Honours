using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    Shell shellOwner;

    public void SetShellOwner(Shell shell)
    {
        shellOwner = shell;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            shellOwner?.RegisterHit();
            Destroy(gameObject);
        }
    }
}
