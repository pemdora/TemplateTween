using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score")]
    private int coinCount = 0;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float respawnHeight = 10f;
    [SerializeField] private OrbitCamera orbitCamera;

    [Header("Game Over Settings")]
    [SerializeField] private float fallThreshold = -10f;

    private Vector3 initialPosition;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (playerTransform != null && playerTransform.position.y < fallThreshold)
        {
            GameOver();
        }
    }

    public void AddCoin()
    {
        coinCount++;
        Debug.Log("Pièces ramassées : " + coinCount);
    }

    public void GameOver()
    {
        Debug.Log("Game Over ! ");

        Vector3 respawnPosition = initialPosition;
        respawnPosition.y += respawnHeight;
        if (playerTransform.TryGetComponent<Rigidbody>(out Rigidbody body))
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }


        playerTransform.position = respawnPosition;
        
        // TODO Add Camera Shake 
        // orbitCamera.AddTrauma(0.9f);
    }
}