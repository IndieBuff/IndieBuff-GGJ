using UnityEngine;

public class EndPortal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.LevelComplete();
        }
    }
}
