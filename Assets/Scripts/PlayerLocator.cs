using UnityEngine;

public class PlayerLocator : MonoBehaviour
{
    public static PlayerLocator Instance { get; private set; }

    public Vector3 CurrentPosition => transform.position;
    public Quaternion CurrentRotation => transform.rotation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
