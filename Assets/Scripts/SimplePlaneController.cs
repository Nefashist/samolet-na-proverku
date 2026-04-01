using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SimplePlaneController : MonoBehaviour
{
    [Header("Rigidbody")]
    private Rigidbody _rb;

    [Header("Engine")]
    [SerializeField] private float _maxThrust = 50000f;
    [SerializeField] private float _throttleSpeed = 0.5f;
    private float _currentThrottle = 0f;

    [Header("Control Surfaces")]
    [SerializeField] private float _pitchPower = 5000f;
    [SerializeField] private float _rollPower = 4000f;
    [SerializeField] private float _yawPower = 3000f;

    [Header("Aerodynamics")]
    [SerializeField] private float _liftPower = 1.5f;
    [SerializeField] private float _dragPower = 0.1f;

    private float _pitch;
    private float _roll;
    private float _yaw;
    private float _throttle;

    private PlaneControls _controls;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _controls = new PlaneControls();

        _rb.mass = 500f;
        _rb.linearDamping = 0.1f;
        _rb.angularDamping = 0.5f;
        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnEnable()
    {
        _controls.Plane.Enable();
        _controls.Plane.Pitch.performed += ctx => _pitch = ctx.ReadValue<Vector2>().y;
        _controls.Plane.Pitch.canceled += ctx => _pitch = 0;
        _controls.Plane.Roll.performed += ctx => _roll = ctx.ReadValue<Vector2>().x;
        _controls.Plane.Roll.canceled += ctx => _roll = 0;
        _controls.Plane.Yaw.performed += ctx => _yaw = ctx.ReadValue<Vector2>().x;
        _controls.Plane.Yaw.canceled += ctx => _yaw = 0;
        _controls.Plane.Throttle.performed += ctx => _throttle = ctx.ReadValue<Vector2>().y;
        _controls.Plane.Throttle.canceled += ctx => _throttle = 0;
    }

    private void OnDisable()
    {
        _controls.Plane.Disable();
    }

    private void FixedUpdate()
    {
        _currentThrottle = Mathf.Clamp01(_currentThrottle + _throttle * _throttleSpeed * Time.fixedDeltaTime);
        Vector3 thrustForce = transform.forward * (_currentThrottle * _maxThrust);
        _rb.AddForce(thrustForce);

        Vector3 controlTorque = Vector3.zero;
        controlTorque += transform.right * (_pitch * _pitchPower * Time.fixedDeltaTime);    // Тангаж
        controlTorque += transform.forward * (_roll * _rollPower * Time.fixedDeltaTime);    // Крен
        controlTorque += transform.up * (_yaw * _yawPower * Time.fixedDeltaTime);           // Рыскание
        _rb.AddTorque(controlTorque);

        Vector3 velocity = _rb.linearVelocity;
        float speed = velocity.magnitude;

        if (speed > 1f)
        {
            Vector3 liftForce = transform.up * (speed * speed * _liftPower * 0.001f);
            _rb.AddForce(liftForce);

            Vector3 dragForce = -velocity.normalized * (speed * speed * _dragPower * 0.1f);
            _rb.AddForce(dragForce);

            float rollAngle = transform.eulerAngles.z;
            if (rollAngle > 180) rollAngle -= 360;
            if (Mathf.Abs(_roll) < 0.1f) 
            {
                float stabilizeTorque = -rollAngle * 10f * Time.fixedDeltaTime;
                _rb.AddTorque(transform.forward * stabilizeTorque);
            }
        }
    }
    public float GetThrottle() => _currentThrottle;
    public float GetSpeed() => _rb.linearVelocity.magnitude;
    public float GetSpeedKMH() => _rb.linearVelocity.magnitude * 3.6f;
    public float GetAltitude() => transform.position.y;
    public float GetAngleOfAttack()
    {
        Vector3 localVel = transform.InverseTransformDirection(_rb.linearVelocity);
        if (localVel.z > 0.1f)
            return Mathf.Atan2(-localVel.y, localVel.z) * Mathf.Rad2Deg;
        return 0;
    }
    public float GetVerticalSpeed() => _rb.linearVelocity.y;
    public Vector3 GetAngles() => transform.eulerAngles;
    public float GetGForce() => _rb.linearVelocity.magnitude / 9.81f;
}