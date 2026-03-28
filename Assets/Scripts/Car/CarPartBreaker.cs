using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach to the car root. Finds all child parts tagged "CarPart"
/// and connects them with ConfigurableJoints. When a part takes a
/// hard enough hit, the joint breaks and the part flies off as a
/// free Rigidbody — wheels roll away, panels tumble, etc.
///
/// SETUP:
///   1. Tag each detachable part (wheels, panels, hood, etc.) with the tag "CarPart"
///   2. Each CarPart needs its own Rigidbody component (can start as Kinematic)
///   3. Attach this script to the car root
/// </summary>
public class CarPartBreaker : MonoBehaviour
{
    [Header("Joint Settings")]
    [Tooltip("Force (Newtons) required to snap a part off. 15000 = hard hits only.")]
    public float breakForce = 15000f;

    [Tooltip("Torque (Nm) required to spin a part off.")]
    public float breakTorque = 10000f;

    [Header("Impact Detection")]
    [Tooltip("Minimum collision force to play a crash sound / trigger effects.")]
    public float minImpactForce = 500f;

    private List<ConfigurableJoint> joints = new List<ConfigurableJoint>();

    void Start()
    {
        AttachAllParts();
    }

    void AttachAllParts()
    {
        // Find every child tagged "CarPart" and joint it to this root
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child == transform) continue;
            if (!child.CompareTag("CarPart")) continue;

            Rigidbody partRb = child.GetComponent<Rigidbody>();
            if (partRb == null)
            {
                partRb = child.gameObject.AddComponent<Rigidbody>();
            }

            // Start kinematic — the joint will control it
            partRb.isKinematic = false;

            ConfigurableJoint joint = gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = partRb;

            // Lock all motion — part moves exactly with the car body
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            // The magic: set how hard a hit is needed to pop the part off
            joint.breakForce  = breakForce;
            joint.breakTorque = breakTorque;

            // Enable pre-processing for stability
            joint.enablePreprocessing = true;

            joints.Add(joint);
        }
    }

    // Called by Unity when any joint on this GameObject breaks
    void OnJointBreak(float breakMagnitude)
    {
        // Visual/audio feedback can be triggered here in Phase 4
        // For now just log it so you can see it working in the console
        Debug.Log($"Part broke off! Impact force: {breakMagnitude:F0}N");
    }

    void OnCollisionEnter(Collision col)
    {
        float force = col.impulse.magnitude / Time.fixedDeltaTime;
        if (force >= minImpactForce)
        {
            Debug.Log($"Crash! Impact: {force:F0}N");
            // Phase 4: trigger AudioManager.PlayCrash(force) here
            // Phase 4: trigger VFXManager.PlaySparks(col.contacts[0].point) here
        }
    }

    /// <summary>
    /// Reset: re-attach all parts (called when player presses R to reset the car).
    /// </summary>
    public void ReattachAllParts()
    {
        // Remove old joints
        foreach (var j in joints)
        {
            if (j != null) Destroy(j);
        }
        joints.Clear();

        // Re-attach everything
        AttachAllParts();
    }
}
