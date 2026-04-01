using UnityEngine;

public class TailAerodynamics : MonoBehaviour
{
    public enum TailType
    {
        Horizontal, // для управления тангажом
        Vertical    // для управления рысканием
    }

    [Header("Tail Type")]
    [SerializeField] private TailType _tailType = TailType.Horizontal;

    [Header("Parameters")]
    [SerializeField] private float _area = 2f; // площадь стабилизатора (м²)
    [SerializeField] private float _effectiveness = 0.8f;
    [SerializeField] private float _maxForceCoefficient = 0.5f;

    private float _controlSurfaceDeflection = 0f;

    public void SetControlSurface(float deflection)
    {
        _controlSurfaceDeflection = Mathf.Clamp(deflection, -1f, 1f);
    }

    public Vector3 CalculateForce(Vector3 velocity, float airDensity, Transform planeTransform)
    {
        Vector3 localVelocity = planeTransform.InverseTransformDirection(velocity);
        float speed = localVelocity.magnitude;

        if (speed < 1f) return Vector3.zero;

        float angleOfAttack = 0f;
        Vector3 forceDirection = Vector3.zero;

        if (_tailType == TailType.Horizontal)
        {
            // Для горизонтального стабилизатора - сила вверх/вниз
            angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;
            forceDirection = planeTransform.up;
        }
        else 
        {
            // Для вертикального стабилизатора - сила влево/вправо
            angleOfAttack = Mathf.Atan2(localVelocity.x, localVelocity.z) * Mathf.Rad2Deg;
            forceDirection = planeTransform.right;
        }

        float effectiveAoA = angleOfAttack + _controlSurfaceDeflection * 20f;

        // Коэффициент силы (линейно зависит от угла)
        float forceCoefficient = Mathf.Clamp(effectiveAoA / 20f, -_maxForceCoefficient, _maxForceCoefficient) * _effectiveness;

        // Динамическое давление
        float q = 0.5f * airDensity * speed * speed;

        // Сила
        float forceMagnitude = q * _area * forceCoefficient;

        return forceDirection * forceMagnitude;
    }
}