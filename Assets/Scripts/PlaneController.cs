using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlaneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _physicsRoot;

    [Header("Control Surfaces")]
    [SerializeField] private WingAerodynamics _leftWing;
    [SerializeField] private WingAerodynamics _rightWing;
    [SerializeField] private TailAerodynamics _horizontalStab;
    [SerializeField] private TailAerodynamics _verticalStab;

    [Header("Engine")]
    [SerializeField] private float _maxThrust = 50000f;  // 50 kN
    [SerializeField] private float _throttleSensitivity = 0.5f;
    private float _currentThrottle = 0f;

    [Header("Control Torques")]
    [SerializeField] private float _pitchTorque = 8000f;
    [SerializeField] private float _rollTorque = 6000f;
    [SerializeField] private float _yawTorque = 4000f;

    [Header("Aerodynamics")]
    [SerializeField] private float _liftFactor = 1.2f;
    [SerializeField] private float _stabilizationStrength = 15f;

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

        // Настройка Rigidbody
        _rb.mass = 500f;
        _rb.linearDamping = 0.1f;
        _rb.angularDamping = 2f;
        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
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
        // 1. Управление двигателем
        HandleEngine();

        // 2. Управляющие моменты (поворот самолета)
        ApplyControlTorque();

        // 3. Аэродинамические силы
        HandleAerodynamics();
    }

    private void HandleEngine()
    {
        _currentThrottle = Mathf.Clamp01(_currentThrottle + _throttleInput * _throttleSensitivity * Time.fixedDeltaTime);

        // Тяга вперед
        Vector3 thrust = _physicsRoot.forward * (_currentThrottle * _maxThrust);
        _rb.AddForce(thrust, ForceMode.Force);
    }

    private void ApplyControlTorque()
    {
        Vector3 torque = Vector3.zero;

        // Pitch (нос вверх/вниз)
        torque += _physicsRoot.right * (_pitchInput * _pitchTorque * Time.fixedDeltaTime);

        // Roll (крен)
        torque += _physicsRoot.forward * (_rollInput * _rollTorque * Time.fixedDeltaTime);

        // Yaw (рыскание)
        torque += _physicsRoot.up * (_yawInput * _yawTorque * Time.fixedDeltaTime);

        _rb.AddTorque(torque, ForceMode.Force);
    }

    private void HandleAerodynamics()
    {
        Vector3 velocity = _rb.linearVelocity;
        float speed = velocity.magnitude;

        if (speed > 1f)  // Минимальная скорость для аэродинамики
        {
            // Подъемная сила от крыльев
            Vector3 localVelocity = _physicsRoot.InverseTransformDirection(velocity);
            float angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z);

            // Подъемная сила
            float liftMagnitude = speed * speed * Mathf.Sin(angleOfAttack) * _liftFactor;
            Vector3 liftForce = _physicsRoot.up * liftMagnitude;
            _rb.AddForce(liftForce, ForceMode.Force);

            // Лобовое сопротивление
            float dragMagnitude = speed * speed * 0.1f;  // Простое сопротивление
            Vector3 dragForce = -velocity.normalized * dragMagnitude;
            _rb.AddForce(dragForce, ForceMode.Force);

            // Банковский поворот
            HandleBankedTurn();
        }

        // Стабилизация
        HandleStabilization();
    }

    private void HandleBankedTurn()
    {
        float bankAngle = transform.eulerAngles.z;
        if (bankAngle > 180f) bankAngle -= 360f;

        if (Mathf.Abs(bankAngle) > 5f && _rb.linearVelocity.magnitude > 10f)
        {
            Vector3 turnDirection = Vector3.Cross(_physicsRoot.up, Vector3.up).normalized;
            float turnStrength = Mathf.Abs(bankAngle) / 90f * _rb.linearVelocity.magnitude * 50f;
            Vector3 turnForce = turnDirection * Mathf.Sign(bankAngle) * turnStrength;

            _rb.AddForce(turnForce, ForceMode.Force);
        }
    }

    private void HandleStabilization()
    {
        // Стабилизация крена при отсутствии ввода
        if (Mathf.Abs(_rollInput) < 0.1f)
        {
            float currentRoll = transform.eulerAngles.z;
            if (currentRoll > 180f) currentRoll -= 360f;

            float stabilizationTorque = -currentRoll * _stabilizationStrength * Time.fixedDeltaTime;
            _rb.AddTorque(_physicsRoot.forward * stabilizationTorque, ForceMode.Force);
        }
    }

    // Public methods for HUD
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
}