using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private static Vector3 lastCheckpointPosition;
    private static bool hasCheckpoint = false;

    private void Start()
    {
        // Initialize first checkpoint as spawn position
        lastCheckpointPosition = transform.position;
        hasCheckpoint = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            lastCheckpointPosition = other.transform.position;
            hasCheckpoint = true;
            Debug.Log("Checkpoint reached!");
        }
    }

    public static void ResetToCheckpoint(GameObject player)
    {
        if (!hasCheckpoint) return;

        // Reset position
        player.transform.position = lastCheckpointPosition;

        // Reset velocity
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
