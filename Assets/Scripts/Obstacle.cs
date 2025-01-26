using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CheckpointManager.ResetToCheckpoint(other.gameObject);
        }
    }
}
