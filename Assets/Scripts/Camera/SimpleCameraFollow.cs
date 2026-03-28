using UnityEngine;

/// <summary>
/// Phase 1 chase camera — no Cinemachine required.
/// Attach to the Main Camera. Assign the car's root Transform as the target.
/// Smoothly follows behind the car and looks at it.
/// </summary>
public class SimpleCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public float followDistance = 8f;   // how far behind the car
    public float followHeight   = 3f;   // how high above the car
    public float smoothSpeed    = 8f;   // higher = snappier

    [Header("Speed Zoom")]
    [Tooltip("Pulls camera back at high speed for a sense of velocity.")]
    public float speedZoomFactor = 0.04f; // extra distance per km/h
    public float maxExtraDistance = 5f;

    private CarController carController;
    private Vector3 currentVelocity;

    void Start()
    {
        if (target != null)
            carController = target.GetComponent<CarController>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Pull back at speed
        float extra = 0f;
        if (carController != null)
            extra = Mathf.Min(carController.SpeedKph * speedZoomFactor, maxExtraDistance);

        float dist = followDistance + extra;

        // Desired position: behind and above the car, relative to car's facing direction
        Vector3 desiredPos = target.position
                           - target.forward * dist
                           + Vector3.up * followHeight;

        // Smooth damp so the camera doesn't snap on sharp turns
        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref currentVelocity, 1f / smoothSpeed);

        // Always look at the car (slightly above center for a better angle)
        transform.LookAt(target.position + Vector3.up * 0.5f);
    }
}
