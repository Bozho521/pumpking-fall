using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerFreeFallController : MonoBehaviour
{
    [Header("Input")]
    public PlayerInput playerInput; 

    [Header("Horizontal")]
    public float horizontalSpeed = 8f;        
    public float horizontalAcceleration = 30f;
    public float horizontalDeceleration = 40f;
    public float xClamp = 10f;                

    [Header("Vertical - Free Fall")]
    public float startGravity = -12f;        
    public float maxGravity = -80f;          
    public float timeToMaxGravity = 6f;      
    public float extraDownforce = 0f;        

    [Header("Z Lock")]
    public bool lockZ = true;
    public float lockedZ = 0f;   
    public float zLockSmoothing = 0f; 

    [Header("Trail")]
    public TrailRenderer trail;
    public float trailMinTime = 0.05f;
    public float trailMaxTime = 0.35f;
    public float trailMinWidth = 0.05f;
    public float trailMaxWidth = 0.25f;
    public float speedForMaxTrail = 60f;

    private CharacterController _cc;
    private Vector3 _vel;            
    private float _gravT;            
    private Vector2 _moveInput;      

    private InputAction _moveAction;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();

        if (playerInput != null)
        {
            _moveAction = playerInput.actions["Move"];
        }

        if (lockZ) lockedZ = transform.position.z;
    }

    private void OnEnable()
    {
        if (playerInput != null) playerInput.enabled = true;
    }

    private void OnDisable()
    {
        if (playerInput != null) playerInput.enabled = false;
    }

    private void Update()
    {
        if (_moveAction != null)
        {
            var raw = _moveAction.ReadValue<Vector2>();
            _moveInput = new Vector2(Mathf.Clamp(raw.x, -1f, 1f), 0f);
        }

        float targetVx = _moveInput.x * horizontalSpeed;
        float accel = Mathf.Abs(targetVx) > Mathf.Epsilon ? horizontalAcceleration : horizontalDeceleration;
        _vel.x = Mathf.MoveTowards(_vel.x, targetVx, accel * Time.deltaTime);

        if (timeToMaxGravity > 0f)
        {
            _gravT = Mathf.Clamp01(_gravT + Time.deltaTime / timeToMaxGravity);
        }
        float currentGravity = Mathf.Lerp(startGravity, maxGravity, _gravT) + extraDownforce;

        
        _vel.y += currentGravity * Time.deltaTime;

        _vel.z = 0f;

        Vector3 delta = _vel * Time.deltaTime;
        _cc.Move(delta);

        if (xClamp > 0f)
        {
            var p = transform.position;
            p.x = Mathf.Clamp(p.x, -xClamp, xClamp);
            transform.position = p;
        }

        if (lockZ)
        {
            var p = transform.position;
            if (zLockSmoothing > 0f)
                p.z = Mathf.Lerp(p.z, lockedZ, 1f - Mathf.Exp(-zLockSmoothing * Time.deltaTime));
            else
                p.z = lockedZ;

            transform.position = p;
        }

        if (trail != null)
        {
            float speedMag = new Vector2(_vel.x, _vel.y).magnitude;
            float t = Mathf.Clamp01(speedMag / Mathf.Max(0.01f, speedForMaxTrail));

            trail.time = Mathf.Lerp(trailMinTime, trailMaxTime, t);
            trail.widthMultiplier = Mathf.Lerp(trailMinWidth, trailMaxWidth, t);

            trail.emitting = speedMag > 1.0f;
        }
    }

    public void ResetFall(float verticalVelocity = 0f)
    {
        _vel = new Vector3(0f, verticalVelocity, 0f);
        _gravT = 0f;
        if (trail != null) trail.Clear();
    }
}
