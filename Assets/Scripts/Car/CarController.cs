using UnityEngine;

/// <summary>
/// Drives the car using Unity's Wheel Collider system.
/// Attach to the car root GameObject. Assign the four WheelColliders
/// and their corresponding wheel mesh Transforms in the Inspector.
/// </summary>
public class CarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Wheel Meshes (visual only)")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Engine")]
    public float motorTorque = 400f;       // Nm — how hard the engine pushes
    public float brakeTorque = 2000f;      // Nm — how hard the brakes bite
    public float maxSteerAngle = 35f;      // degrees — how sharp the wheels turn

    [Header("Handbrake")]
    public float handbrakeTorque = 5000f;  // extra rear brake for drifting

    [Header("Anti-Roll (keeps car stable on bumps)")]
    public float antiRollForce = 5000f;

    private Rigidbody rb;
    private float currentSteer;
    private float currentMotor;
    private bool isHandbraking;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Lower center of mass so the car doesn't tip over easily
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
    }

    void Update()
    {
        // Read input every frame (Update is smoother for input than FixedUpdate)
        currentMotor  = Input.GetAxis("Vertical");   // W/S or Up/Down
        currentSteer  = Input.GetAxis("Horizontal"); // A/D or Left/Right
        isHandbraking = Input.GetKey(KeyCode.Space);
    }

    void FixedUpdate()
    {
        ApplySteering();
        ApplyMotor();
        ApplyBrakes();
        UpdateWheelMeshes();
        ApplyAntiRoll();
    }

    void ApplySteering()
    {
        float angle = currentSteer * maxSteerAngle;
        frontLeftWheel.steerAngle  = angle;
        frontRightWheel.steerAngle = angle;
    }

    void ApplyMotor()
    {
        // Rear-wheel drive
        rearLeftWheel.motorTorque  = currentMotor * motorTorque;
        rearRightWheel.motorTorque = currentMotor * motorTorque;
    }

    void ApplyBrakes()
    {
        if (isHandbraking)
        {
            // Handbrake: lock rear wheels for drifts / spin-outs
            rearLeftWheel.brakeTorque  = handbrakeTorque;
            rearRightWheel.brakeTorque = handbrakeTorque;
            frontLeftWheel.brakeTorque  = 0f;
            frontRightWheel.brakeTorque = 0f;
        }
        else if (currentMotor < -0.1f && rb.linearVelocity.magnitude > 0.5f)
        {
            // Braking: S key applies brakes to all four wheels
            float brake = Mathf.Abs(currentMotor) * brakeTorque;
            frontLeftWheel.brakeTorque  = brake;
            frontRightWheel.brakeTorque = brake;
            rearLeftWheel.brakeTorque   = brake;
            rearRightWheel.brakeTorque  = brake;
            // Cut motor while braking
            rearLeftWheel.motorTorque  = 0f;
            rearRightWheel.motorTorque = 0f;
        }
        else
        {
            frontLeftWheel.brakeTorque  = 0f;
            frontRightWheel.brakeTorque = 0f;
            rearLeftWheel.brakeTorque   = 0f;
            rearRightWheel.brakeTorque  = 0f;
        }
    }

    // Sync the visual wheel meshes to the WheelCollider positions/rotations
    void UpdateWheelMeshes()
    {
        UpdateSingleWheel(frontLeftWheel,  frontLeftMesh);
        UpdateSingleWheel(frontRightWheel, frontRightMesh);
        UpdateSingleWheel(rearLeftWheel,   rearLeftMesh);
        UpdateSingleWheel(rearRightWheel,  rearRightMesh);
    }

    void UpdateSingleWheel(WheelCollider col, Transform mesh)
    {
        if (mesh == null) return;
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }

    // Anti-roll bar: resists body lean in corners
    void ApplyAntiRoll()
    {
        ApplyAntiRollPair(frontLeftWheel, frontRightWheel);
        ApplyAntiRollPair(rearLeftWheel,  rearRightWheel);
    }

    void ApplyAntiRollPair(WheelCollider left, WheelCollider right)
    {
        float travelL = 1f, travelR = 1f;

        bool groundedL = left.GetGroundHit(out WheelHit hitL);
        if (groundedL)
            travelL = (-left.transform.InverseTransformPoint(hitL.point).y - left.radius) / left.suspensionDistance;

        bool groundedR = right.GetGroundHit(out WheelHit hitR);
        if (groundedR)
            travelR = (-right.transform.InverseTransformPoint(hitR.point).y - right.radius) / right.suspensionDistance;

        float force = (travelL - travelR) * antiRollForce;

        if (groundedL) rb.AddForceAtPosition(left.transform.up  * -force, left.transform.position);
        if (groundedR) rb.AddForceAtPosition(right.transform.up *  force, right.transform.position);
    }

    /// <summary>Speed in km/h — used by HUD and AudioManager.</summary>
    public float SpeedKph => rb.linearVelocity.magnitude * 3.6f;
}
