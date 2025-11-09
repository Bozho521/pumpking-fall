using UnityEngine;

public class CrownMovement : MonoBehaviour
{
    [SerializeField] private GameObject crownPrefab;

    [Header("Movement Settings")]
    [SerializeField] private float distanceBelowPlayer = 15f;
    [SerializeField] private float horizontalMotion = 3f;
    [SerializeField] private float verticalMotion = 1f;
    [SerializeField] private float motionFrequency = 1.5f;
    [SerializeField] private float baseSmoothness = 5f;
    [SerializeField] private float rotationSpeed = 60f;

    [Header("Speed Settings")]
    [SerializeField] private float minFallSpeed = 1f;
    [SerializeField] private float maxFallSpeed = 10f;
    [SerializeField] private float speedSensitivity = 0.1f;
    [SerializeField] private float fallAcceleration = 0.2f;

    private GameObject crownInstance;
    private Vector3 velocity;
    private float driftSeed;
    private Vector3 previousPlayerPos;
    private float playerFallSpeed;
    private float timeMultiplier = 1f;

    private void Start()
    {
        if (crownPrefab == null || PlayerLocator.Instance == null) return;

        Vector3 spawnPos = PlayerLocator.Instance.CurrentPosition - new Vector3(0, distanceBelowPlayer, 0);
        crownInstance = Instantiate(crownPrefab, spawnPos, Quaternion.identity);
        driftSeed = Random.value * 100f;
        previousPlayerPos = PlayerLocator.Instance.CurrentPosition;
    }

    private void Update()
    {
        if (crownInstance == null || PlayerLocator.Instance == null) return;

        Vector3 playerPos = PlayerLocator.Instance.CurrentPosition;

        playerFallSpeed = Mathf.Lerp(
            playerFallSpeed,
            (previousPlayerPos.y - playerPos.y) / Mathf.Max(Time.deltaTime, 0.001f),
            Time.deltaTime * 5f
        );
        previousPlayerPos = playerPos;

        float speedFactor = Mathf.InverseLerp(0f, 100f, Mathf.Abs(playerFallSpeed) * speedSensitivity);
        float currentSpeed = Mathf.Lerp(minFallSpeed, maxFallSpeed, speedFactor);

        Vector3 targetPos = playerPos - new Vector3(0, distanceBelowPlayer, 0);

        float time = Time.time + driftSeed;
        Vector3 motionOffset = new Vector3(
            Mathf.Sin(time * motionFrequency) * horizontalMotion,
            Mathf.Sin(time * motionFrequency * 1.7f) * verticalMotion,
            Mathf.Cos(time * motionFrequency * 0.9f) * horizontalMotion
        );
        targetPos += motionOffset;

        timeMultiplier += fallAcceleration * Time.deltaTime;

        crownInstance.transform.position = Vector3.SmoothDamp(
            crownInstance.transform.position,
            targetPos,
            ref velocity,
            1f / ((baseSmoothness + currentSpeed) * timeMultiplier)
        );

        crownInstance.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
