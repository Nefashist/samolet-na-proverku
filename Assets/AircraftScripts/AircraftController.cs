
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class AircraftController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _physicsRoot;

    [Header("Engine Settings")]
    [SerializeField] private float _maxThrust = 5000f;
    [SerializeField] private float _throttleResponse = 2f;
    private float _currentThrottle = 0f;

    [Header("Control Settings")]
    [SerializeField] private float _pitchTorque = 8000f;
    [SerializeField] private float _rollTorque = 6000f;
    [SerializeField] private float _yawTorque = 4000f;

    [Header("Stabilization")]
    [SerializeField] private bool _enableStabilization = true;
    [SerializeField] private float _stabilizationStrength = 25f;
    [SerializeField] private float _maxStabilizationAngle = 45f; 

    [Header("Aerodynamics")]
    [SerializeField] private float _liftFactor = 1.5f;
    [SerializeField] private float _dragFactor = 0.1f;

    [Header("Control Smoothing")]
    [SerializeField][Range(1f, 20f)] private float _smoothness = 8f;
    [SerializeField][Range(0.1f, 2f)] private float _sensitivity = 1f;

    // Сглаженные значения
    private float _smoothPitch;
    private float _smoothRoll;
    private float _smoothYaw;

    // Input values
    private float _pitchInput;
    private float _rollInput;
    private float _yawInput;
    private float _throttleInput;

    private PlaneControls _controls;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_physicsRoot == null) _physicsRoot = transform;

        _controls = new PlaneControls();

        _rb.linearDamping = 0.2f;
        _rb.angularDamping = 3f;
        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.centerOfMass = new Vector3(0, -0.3f, 0);
    }

    private void OnEnable()
    {
        _controls.Plane.Enable();

        _controls.Plane.Pitch.performed += OnPitch;
        _controls.Plane.Pitch.canceled += OnPitch;
        _controls.Plane.Roll.performed += OnRoll;
        _controls.Plane.Roll.canceled += OnRoll;
        _controls.Plane.Yaw.performed += OnYaw;
        _controls.Plane.Yaw.canceled += OnYaw;
        _controls.Plane.Throttle.performed += OnThrottle;
        _controls.Plane.Throttle.canceled += OnThrottle;
    }

    private void OnDisable()
    {
        _controls.Plane.Disable();
    }

    private void OnPitch(InputAction.CallbackContext ctx) => _pitchInput = ctx.ReadValue<Vector2>().y;
    private void OnRoll(InputAction.CallbackContext ctx) => _rollInput = ctx.ReadValue<Vector2>().x;
    private void OnYaw(InputAction.CallbackContext ctx) => _yawInput = ctx.ReadValue<Vector2>().x;
    private void OnThrottle(InputAction.CallbackContext ctx) => _throttleInput = ctx.ReadValue<Vector2>().y;

    private void FixedUpdate()
    {
        SmoothInput();
        HandleEngine();
        ApplyAerodynamics();
        ApplyControlAndStabilization();
    }

    private void SmoothInput()
    {
        float smoothFactor = Time.fixedDeltaTime * _smoothness;

        _smoothPitch = Mathf.Lerp(_smoothPitch, _pitchInput * _sensitivity, smoothFactor);
        _smoothRoll = Mathf.Lerp(_smoothRoll, _rollInput * _sensitivity, smoothFactor);
        _smoothYaw = Mathf.Lerp(_smoothYaw, _yawInput * _sensitivity, smoothFactor);
    }

    private void HandleEngine()
    {
        _currentThrottle = Mathf.Clamp01(_currentThrottle + _throttleInput * Time.fixedDeltaTime * _throttleResponse);
        Vector3 thrust = transform.forward * (_currentThrottle * _maxThrust);
        _rb.AddForce(thrust, ForceMode.Force);
    }

    private void ApplyAerodynamics()
    {
        Vector3 velocity = _rb.linearVelocity;
        float speed = velocity.magnitude;

        if (speed > 1f)
        {
            // Подъемная сила
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z);
            float liftMagnitude = speed * speed * Mathf.Sin(angleOfAttack) * _liftFactor;
            Vector3 liftForce = transform.up * liftMagnitude;
            _rb.AddForce(liftForce, ForceMode.Force);

            // Сопротивление
            float dragMagnitude = speed * speed * _dragFactor;
            Vector3 dragForce = -velocity.normalized * dragMagnitude;
            _rb.AddForce(dragForce, ForceMode.Force);

            // Банковский поворот
            ApplyBankedTurn(speed);
        }
    }

    private void ApplyBankedTurn(float speed)
    {
        float bankAngle = transform.eulerAngles.z;
        if (bankAngle > 180f) bankAngle -= 360f;

        if (Mathf.Abs(bankAngle) > 5f && speed > 10f)
        {
            Vector3 turnDirection = Vector3.Cross(transform.up, Vector3.up).normalized;
            float turnStrength = Mathf.Abs(bankAngle) / 90f * speed * 30f;
            Vector3 turnForce = turnDirection * Mathf.Sign(bankAngle) * turnStrength;
            _rb.AddForce(turnForce, ForceMode.Force);
        }
    }

    private void ApplyControlAndStabilization()
    {
        Vector3 controlTorque = Vector3.zero;

        controlTorque += transform.right * (_smoothPitch * _pitchTorque * Time.fixedDeltaTime);
        controlTorque += transform.forward * (_smoothRoll * _rollTorque * Time.fixedDeltaTime);
        controlTorque += transform.up * (_smoothYaw * _yawTorque * Time.fixedDeltaTime);

        if (_enableStabilization)
        {
            // Получаем текущие углы
            float currentPitch = transform.eulerAngles.x;
            float currentRoll = transform.eulerAngles.z;
            float currentYaw = transform.eulerAngles.y;

            // Нормализуем углы
            if (currentPitch > 180f) currentPitch -= 360f;
            if (currentRoll > 180f) currentRoll -= 360f;

            // Стабилизация крена
            if (Mathf.Abs(_smoothRoll) < 0.1f) 
            {
                float rollStabilization = -currentRoll * _stabilizationStrength * Time.fixedDeltaTime;
                rollStabilization = Mathf.Clamp(rollStabilization, -_maxStabilizationAngle, _maxStabilizationAngle);
                controlTorque += transform.forward * rollStabilization;
            }

            // Стабилизация тангажа
            if (Mathf.Abs(_smoothPitch) < 0.1f && Mathf.Abs(currentPitch) > 5f)
            {
                float pitchStabilization = -currentPitch * _stabilizationStrength * 0.5f * Time.fixedDeltaTime;
                pitchStabilization = Mathf.Clamp(pitchStabilization, -_maxStabilizationAngle, _maxStabilizationAngle);
                controlTorque += transform.right * pitchStabilization;
            }

            // Гашение колебаний
            Vector3 angularVelocity = _rb.angularVelocity;
            controlTorque += -angularVelocity * 5f * Time.fixedDeltaTime;
        }

        _rb.AddTorque(controlTorque, ForceMode.Force);
    }

    // Для HUD
    public float GetThrottle() => _currentThrottle;
    public float GetSpeed() => _rb.linearVelocity.magnitude;
    public float GetSpeedKMH() => _rb.linearVelocity.magnitude * 3.6f;
    public float GetAltitude() => transform.position.y;

    public float GetAngleOfAttack()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(_rb.linearVelocity);
        if (localVelocity.z > 0.1f)
            return Mathf.Atan2(-localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;
        return 0f;
    }

    public float GetVerticalSpeed() => _rb.linearVelocity.y;
    public Vector3 GetAngles() => transform.eulerAngles;
    public float GetGForce() => _rb.linearVelocity.magnitude / 9.81f;
    public bool IsStalled() => Mathf.Abs(GetAngleOfAttack()) > 16f && GetSpeed() < 15f;
}

