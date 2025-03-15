using UnityEngine;

public class CorridorTrigger : MonoBehaviour
{
    public GameObject blockingWall; // Assign this in Inspector

    void Start()
    {
        blockingWall.SetActive(true); // Ensure it's visible at the start
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            blockingWall.SetActive(false); // Reveal next room when player enters corridor
        }
    }
}
