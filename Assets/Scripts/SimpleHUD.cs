using UnityEngine;
using TMPro;

public class SimpleHUD : MonoBehaviour
{
    [SerializeField] private SimplePlaneController _plane;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI altText;
    [SerializeField] private TextMeshProUGUI aoaText;
    [SerializeField] private TextMeshProUGUI throttleText;
    [SerializeField] private TextMeshProUGUI vsText;
    [SerializeField] private TextMeshProUGUI attitudeText;
    [SerializeField] private TextMeshProUGUI gText;

    private void Update()
    {
        if (_plane == null) return;

        speedText.text = $"Speed: {_plane.GetSpeed():F1} m/s | {_plane.GetSpeedKMH():F0} km/h";
        altText.text = $"Alt: {_plane.GetAltitude():F1} m";
        aoaText.text = $"AoA: {_plane.GetAngleOfAttack():F1}°";
        throttleText.text = $"Throttle: {_plane.GetThrottle() * 100:F0}%";
        vsText.text = $"V/S: {_plane.GetVerticalSpeed():F1} m/s";

        Vector3 angles = _plane.GetAngles();
        attitudeText.text = $"R:{angles.z:F0}° P:{angles.x:F0}° Y:{angles.y:F0}°";

        gText.text = $"G: {_plane.GetGForce():F1}";
    }
}