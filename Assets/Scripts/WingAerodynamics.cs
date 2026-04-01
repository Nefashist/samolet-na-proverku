using UnityEngine;

public class WingAerodynamics : MonoBehaviour
{
    [Header("Wing Parameters")]
    [SerializeField] private float _wingArea = 8f;
    [SerializeField] private float _maxLiftCoefficient = 1.2f;
    [SerializeField] private float _stallAngle = 15f;

    [Header("Aileron")]
    [SerializeField] private bool _hasAileron = true;
    private float _aileronDeflection = 0f;

    public void SetAileronDeflection(float deflection)
    {
        _aileronDeflection = Mathf.Clamp(deflection, -1f, 1f);
    }

    public Vector3 CalculateForce(Vector3 velocity, float airDensity, Transform planeTransform)
    {
        // Локальная скорость относительно самолета
        Vector3 localVelocity = planeTransform.InverseTransformDirection(velocity);
        float speed = localVelocity.magnitude;

        if (speed < 1f) return Vector3.zero;

        // формула из теории
        float angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;

        float effectiveAoA = angleOfAttack + _aileronDeflection * 10f;

        // Коэффициент подъемной силы 
        float liftCoefficient;
        if (Mathf.Abs(effectiveAoA) < _stallAngle)
            liftCoefficient = _maxLiftCoefficient * (effectiveAoA / _stallAngle);
        else
            liftCoefficient = _maxLiftCoefficient * Mathf.Sign(effectiveAoA) * 0.5f; // сваливание

        // Коэффициент сопротивления
        float dragCoefficient = 0.02f + 0.1f * Mathf.Abs(effectiveAoA / 45f);

        // Динамическое давление: q = 0.5 * ρ * V²
        float q = 0.5f * airDensity * speed * speed;

        // Подъемная сила: L = q * S * CL 
        float liftMagnitude = q * _wingArea * liftCoefficient;
        Vector3 liftForce = planeTransform.up * liftMagnitude;

        // Лобовое сопротивление: D = q * S * CD 
        float dragMagnitude = q * _wingArea * dragCoefficient;
        Vector3 dragForce = -planeTransform.forward * dragMagnitude;

        return liftForce + dragForce;
    }
}