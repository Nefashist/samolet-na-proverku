using UnityEngine;
using TMPro;

public class AircraftHUD : MonoBehaviour
{
    [SerializeField] private AircraftController _aircraft;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private TextMeshProUGUI aoaText;
    [SerializeField] private TextMeshProUGUI throttleText;
    [SerializeField] private TextMeshProUGUI vspeedText;
    [SerializeField] private TextMeshProUGUI attitudeText;
    [SerializeField] private TextMeshProUGUI gforceText;
    [SerializeField] private TextMeshProUGUI stallWarningText;

    private void Update()
    {
        if (_aircraft == null) return;

        // Скорость (м/с и км/ч)
        speedText.text = $"Speed: {_aircraft.GetSpeed():F1} m/s | {_aircraft.GetSpeedKMH():F0} km/h";

        // Высота
        altitudeText.text = $"Alt: {_aircraft.GetAltitude():F1} m";

        // Угол 
        aoaText.text = $"AoA: {_aircraft.GetAngleOfAttack():F1}°";

        // Тяга 
        throttleText.text = $"Throttle: {_aircraft.GetThrottle() * 100:F0}%";

        // Вертикальная скорость
        vspeedText.text = $"V/S: {_aircraft.GetVerticalSpeed():F1} m/s";

        // Углы крена, тангажа, рыскания
        Vector3 angles = _aircraft.GetAngles();
        attitudeText.text = $"R:{angles.z:F0}° P:{angles.x:F0}° Y:{angles.y:F0}°";

        // Перегрузка
        gforceText.text = $"G: {_aircraft.GetGForce():F1}";

    }
}