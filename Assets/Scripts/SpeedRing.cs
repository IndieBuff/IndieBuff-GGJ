using UnityEngine;

public class SpeedRing : MonoBehaviour
{
    [SerializeField] private float boostForce = 10f;
    [SerializeField] private AudioSource speedBoost;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Apply force in the direction the player is already moving
                playerRb.AddForce(playerRb.linearVelocity.normalized * boostForce, ForceMode.Impulse);
                speedBoost.Play();

            }
        }
    }
}
