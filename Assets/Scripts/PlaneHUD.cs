using UnityEngine;
using TMPro;

public class PlaneHUD : MonoBehaviour
{
    [SerializeField] private PlaneController _plane;

    [Header("UI Text Elements")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private TextMeshProUGUI aoaText;
    [SerializeField] private TextMeshProUGUI throttleText;
    [SerializeField] private TextMeshProUGUI vspeedText;
    [SerializeField] private TextMeshProUGUI attitudeText;
    [SerializeField] private TextMeshProUGUI gforceText;

    private void Update()
    {
        if (_plane == null) return;

        // Скорость (м/с и км/ч)
        speedText.text = $"Speed: {_plane.GetSpeed():F1} m/s | {_plane.GetSpeedKMH():F0} km/h";

        // Высота
        altitudeText.text = $"Alt: {_plane.GetAltitude():F1} m";

        // Угол атаки
        aoaText.text = $"AoA: {_plane.GetAngleOfAttack():F1}°";

        // Тяга
        throttleText.text = $"Throttle: {_plane.GetThrottle() * 100:F0}%";

        // Вертикальная скорость
        vspeedText.text = $"V/S: {_plane.GetVerticalSpeed():F1} m/s";

        // Углы (крен, тангаж, рыскание)
        Vector3 angles = _plane.GetAngles();
        attitudeText.text = $"R:{angles.z:F0}° P:{angles.x:F0}° Y:{angles.y:F0}°";

        // Перегрузка
        gforceText.text = $"G: {_plane.GetGForce():F1}";
    }
}