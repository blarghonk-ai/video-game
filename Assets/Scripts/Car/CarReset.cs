using UnityEngine;

/// <summary>
/// Press R to reset the car to its starting position and re-attach all parts.
/// Attach to the car root alongside CarController and CarPartBreaker.
/// </summary>
public class CarReset : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;
    private CarPartBreaker partBreaker;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        partBreaker = GetComponent<CarPartBreaker>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetCar();
    }

    public void ResetCar()
    {
        // Stop all physics movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Snap back to start
        transform.position = startPosition + Vector3.up * 0.5f; // slight lift so it doesn't clip ground
        transform.rotation = startRotation;

        // Re-attach any parts that flew off
        if (partBreaker != null)
            partBreaker.ReattachAllParts();
    }
}
