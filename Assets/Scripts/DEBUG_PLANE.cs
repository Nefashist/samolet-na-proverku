using UnityEngine;
using UnityEngine.InputSystem;

public class DEBUG_PLANE : MonoBehaviour
{
    private Rigidbody _rb;

    [Header("Test Controls")]
    public float forceMultiplier = 100f;

    private float _hInput;
    private float _vInput;

    private PlaneControls _controls;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _controls = new PlaneControls();

        // Минимальные настройки
        _rb.mass = 1f;
        _rb.linearDamping = 0f;
        _rb.angularDamping = 0f;
        _rb.useGravity = false; // Отключаем гравитацию для теста
    }

    private void OnEnable()
    {
        _controls.Plane.Enable();
        _controls.Plane.Roll.performed += ctx => _hInput = ctx.ReadValue<Vector2>().x;
        _controls.Plane.Roll.canceled += ctx => _hInput = 0;
        _controls.Plane.Pitch.performed += ctx => _vInput = ctx.ReadValue<Vector2>().y;
        _controls.Plane.Pitch.canceled += ctx => _vInput = 0;
    }

    private void OnDisable()
    {
        _controls.Plane.Disable();
    }

    private void FixedUpdate()
    {
        // Прямое применение силы для теста
        Vector3 force = new Vector3(_hInput, _vInput, 0) * forceMultiplier;
        _rb.AddForce(force, ForceMode.Force);

        // Вывод в консоль для отладки
        if (_hInput != 0 || _vInput != 0)
            Debug.Log($"Input: H={_hInput}, V={_vInput}");
    }
}