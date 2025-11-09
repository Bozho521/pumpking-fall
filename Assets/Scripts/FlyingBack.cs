using UnityEngine;

public class FlyingBack : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 5f;      // how far to walk before turning
    public float moveSpeed = 2f;         // units per second
    public float turnSpeed = 180f;       // degrees per second (for turning)

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool goingForward = true;
    private bool turning = false;
    private Quaternion targetRotation;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + transform.forward * moveDistance;
    }

    void Update()
    {
        if (turning)
        {
            // Smoothly rotate towards targetRotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // Check if finished turning
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
            {
                turning = false;

                // Swap movement direction
                goingForward = !goingForward;

                // Set next target position
                targetPosition = goingForward
                    ? startPosition + transform.forward * moveDistance
                    : startPosition;
            }
        }
        else
        {
            // Move towards the current target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // If reached the destination start turning around
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                turning = true;

                // Turn exactly 180° around the objects *own up axis*
                targetRotation = Quaternion.AngleAxis(180f, transform.up) * transform.rotation;
            }
        }
    }
}
