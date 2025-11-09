using System.Collections;
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
    [HideInInspector] public float externalSpeedMultiplier;

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

    [Header("Controls")]
    public bool invertControls = false;

    [Header("Player Graphics")]
    public Transform playerGraphics;
    public float graphicsYawAngle = 25f;
    public float graphicsBankAngle = 15f;
    public float graphicsTurnSpeed = 540f;

    [Header("Spin")]
    public float spinDuration = 0.35f;
    public float spinOppositeWindow = 2f;
    public GameObject spinTrailObject;

    [Header("Idle")]
    public float idleYawAmplitude = 8f;
    public float idleBankAmplitude = 8f;
    public float idleOscillationSpeed = 2f;
    public float idleThreshold = 0.15f;
    public float idleBlendSpeed = 6f;

    private CharacterController _cc;
    private Vector3 _vel;
    private float _gravT;
    private Vector2 _moveInput;
    private InputAction _moveAction;

    private float _targetYaw;
    private float _currentYaw;
    private float _targetBank;
    private float _currentBank;
    private float _spinAdditiveYaw;
    private bool _spinning;

    private float _moveBlend;

    private int _prevActiveDir; 
    private int _lastPressDir; 
    private float _lastPressTime;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (playerInput != null) _moveAction = playerInput.actions["Move"];
        if (lockZ) lockedZ = transform.position.z;
        if (playerGraphics != null)
        {
            var e = playerGraphics.localEulerAngles;
            _currentYaw = e.y;
            _currentBank = NormalizeSignedAngle(e.z);
        }
        if (spinTrailObject != null) spinTrailObject.SetActive(false);
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
        float x = 0f;
        if (_moveAction != null)
        {
            x = _moveAction.ReadValue<Vector2>().x;
            if (invertControls) x = -x;
        }
        _moveInput = new Vector2(Mathf.Clamp(x, -1f, 1f), 0f);

        DetectPressAndMaybeSpin();

        float targetVx = _moveInput.x * horizontalSpeed;
        float accel = Mathf.Abs(targetVx) > Mathf.Epsilon ? horizontalAcceleration : horizontalDeceleration;
        _vel.x = Mathf.MoveTowards(_vel.x, targetVx, accel * Time.deltaTime);

        if (timeToMaxGravity > 0f) _gravT = Mathf.Clamp01(_gravT + Time.deltaTime / timeToMaxGravity);
        float currentGravity = Mathf.Lerp(startGravity, maxGravity, _gravT) + extraDownforce;
        _vel.y += currentGravity * externalSpeedMultiplier * Time.deltaTime;
        _vel.z = 0f;

        Vector3 delta = _vel * Time.deltaTime;
        _cc.Move(delta);

        var p = transform.position;
        if (xClamp > 0f)
        {
            p.x = Mathf.Clamp(p.x, -xClamp, xClamp);
            transform.position = p;
        }

        if (lockZ)
        {
            p = transform.position;
            p.z = zLockSmoothing > 0f
                ? Mathf.Lerp(p.z, lockedZ, 1f - Mathf.Exp(-zLockSmoothing * Time.deltaTime))
                : lockedZ;
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

        UpdateGraphicsRotation();
    }

    private void DetectPressAndMaybeSpin()
    {
        int dir = 0;
        if (_moveInput.x > idleThreshold) dir = +1;
        else if (_moveInput.x < -idleThreshold) dir = -1;

        bool pressedThisFrame = dir != 0 && _prevActiveDir == 0;
        if (pressedThisFrame)
        {
            if (_lastPressDir != 0 && _lastPressDir == -dir && (Time.time - _lastPressTime) <= spinOppositeWindow && !_spinning)
            {
                float spin = dir > 0 ? -360f : +360f;
                StartCoroutine(SpinYawCoroutine(spin, spinDuration));
            }
            _lastPressDir = dir;
            _lastPressTime = Time.time;
        }
        _prevActiveDir = dir;

        float targetBlend = Mathf.InverseLerp(idleThreshold, 1f, Mathf.Abs(_moveInput.x));
        _moveBlend = Mathf.MoveTowards(_moveBlend, targetBlend, idleBlendSpeed * Time.deltaTime);
    }

    private void UpdateGraphicsRotation()
    {
        if (playerGraphics == null) return;

        float tOsc = Time.time * idleOscillationSpeed;
        float idleYaw = Mathf.Sin(tOsc) * idleYawAmplitude;
        float idleBank = Mathf.Sin(tOsc * 1.2f) * idleBankAmplitude;

        float inputSign = Mathf.Sign(_moveInput.x == 0 ? 0 : _moveInput.x);
        float speedAbs = Mathf.Abs(_vel.x);
        float moveYaw = graphicsYawAngle * inputSign;
        float bankT = Mathf.Clamp01(speedAbs / Mathf.Max(0.01f, horizontalSpeed));
        float moveBank = graphicsBankAngle * Mathf.Sign(_vel.x == 0 ? 0 : _vel.x) * bankT;

        _targetYaw = Mathf.Lerp(idleYaw, moveYaw, _moveBlend);
        _targetBank = Mathf.Lerp(idleBank, moveBank, _moveBlend);

        _currentYaw = Mathf.MoveTowardsAngle(_currentYaw, _targetYaw, graphicsTurnSpeed * Time.deltaTime);
        _currentBank = Mathf.MoveTowardsAngle(_currentBank, _targetBank, graphicsTurnSpeed * Time.deltaTime);

        Vector3 e = playerGraphics.localEulerAngles;
        float yaw = _currentYaw + _spinAdditiveYaw;
        float bank = _currentBank;
        playerGraphics.localRotation = Quaternion.Euler(e.x, yaw, bank);
    }

    private IEnumerator SpinYawCoroutine(float spinDegrees, float duration)
    {
        _spinning = true;
        if (spinTrailObject != null) spinTrailObject.SetActive(true);

        float elapsed = 0f;
        float start = NormalizeSignedAngle(_spinAdditiveYaw);
        float end = start + spinDegrees;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            _spinAdditiveYaw = Mathf.Lerp(start, end, t);
            yield return null;
        }

        _spinAdditiveYaw = 0f;
        _spinning = false;
        if (spinTrailObject != null) spinTrailObject.SetActive(false);
    }

    public void ResetFall(float verticalVelocity = 0f)
    {
        _vel = new Vector3(0f, verticalVelocity, 0f);
        _gravT = 0f;
        if (trail != null) trail.Clear();
        _currentYaw = 0f;
        _currentBank = 0f;
        _spinAdditiveYaw = 0f;
        _spinning = false;
        _moveBlend = 0f;
        _prevActiveDir = 0;
        _lastPressDir = 0;
        _lastPressTime = 0f;
        if (spinTrailObject != null) spinTrailObject.SetActive(false);
        if (playerGraphics != null) playerGraphics.localRotation = Quaternion.identity;
    }

    private static float NormalizeSignedAngle(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        if (a < -180f) a += 360f;
        return a;
    }
}
