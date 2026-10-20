using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private float minAngle = 0.0f;
    [SerializeField] private float maxAngle = 360.0f;
    [SerializeField] private float lerpDuration = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Demande au GameManager d'ajouter une pièce
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoin();
            }
            Destroy(gameObject);
        }
    }

    // TODO : Utiliser Mathf.LerpAngle pour faire tourner la pièce de minAngle à maxAngle
}

