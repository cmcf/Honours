using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdentityTrigger : MonoBehaviour
{
    public Enemy enemyToTransformInto;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TransformIntoEnemy(enemyToTransformInto);
            }
        }
    }
}
