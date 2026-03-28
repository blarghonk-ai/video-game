using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Manages all camera modes using Cinemachine.
///
/// SETUP in Unity:
///   1. Create a CinemachineCamera (GameObject > Cinemachine > Cinemachine Camera)
///      named "ChaseCamera" — set Follow and LookAt to the car
///   2. Create a second CinemachineCamera named "HoodCamera"
///      — position it at the front hood of the car as a child of the car body
///   3. Attach this script to an empty GameObject called "CameraController"
///   4. Assign both virtual cameras in the Inspector
///   5. Make sure a CinemachineBrain is on your Main Camera (added automatically)
///
/// Press C to cycle between Chase and Hood cameras.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Virtual Cameras")]
    public CinemachineCamera chaseCamera;
    public CinemachineCamera hoodCamera;

    [Header("Chase Camera Settings")]
    [Tooltip("Base priority for the active camera.")]
    public int activePriority = 10;
    public int inactivePriority = 0;

    [Header("Speed-Based Zoom")]
    [Tooltip("Car controller used to read speed.")]
    public CarController car;

    [Tooltip("How far the chase cam pulls back at top speed.")]
    public float maxPullbackDistance = 12f;
    public float basePullbackDistance = 6f;
    public float topSpeedKph = 120f;

    private enum CamMode { Chase, Hood }
    private CamMode currentMode = CamMode.Chase;

    private CinemachineFollow chaseFollow;

    void Start()
    {
        // Grab the Follow component so we can adjust distance at runtime
        if (chaseCamera != null)
            chaseFollow = chaseCamera.GetComponent<CinemachineFollow>();

        SetCamera(CamMode.Chase);
    }

    void Update()
    {
        // C key cycles camera modes
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentMode = currentMode == CamMode.Chase ? CamMode.Hood : CamMode.Chase;
            SetCamera(currentMode);
        }

        // Speed-based pull-back on the chase camera
        if (currentMode == CamMode.Chase && car != null && chaseFollow != null)
        {
            float speedRatio = Mathf.Clamp01(car.SpeedKph / topSpeedKph);
            float targetDist = Mathf.Lerp(basePullbackDistance, maxPullbackDistance, speedRatio);
            chaseFollow.FollowOffset = Vector3.Lerp(
                chaseFollow.FollowOffset,
                new Vector3(0, 2f, -targetDist),
                Time.deltaTime * 3f
            );
        }
    }

    void SetCamera(CamMode mode)
    {
        if (chaseCamera != null)
            chaseCamera.Priority = (mode == CamMode.Chase) ? activePriority : inactivePriority;

        if (hoodCamera != null)
            hoodCamera.Priority = (mode == CamMode.Hood) ? activePriority : inactivePriority;
    }
}
