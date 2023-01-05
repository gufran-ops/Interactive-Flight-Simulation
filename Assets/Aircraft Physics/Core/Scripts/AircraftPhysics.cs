using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AircraftPhysics : MonoBehaviour
{
    const float PREDICTION_TIMESTEP_FRACTION = 0.5f;

    [SerializeField] 
    float thrust = 0;
    [SerializeField] 
    List<AeroSurface> aerodynamicSurfaces = null;

    public Transform motor_F_L;
    public Transform motor_F_R;
    public Transform motor_B_L;
    public Transform motor_B_R;


    Rigidbody rb;
    float thrustPercent=0;
    [HideInInspector]
    public float verticalThrust=0;
    float pitch= 0f;
    float roll= 0f;
    float yaw= 0f;
    BiVector3 currentForceAndTorque;

    public void SetThrustPercent(float percent)
    {
        thrustPercent = percent;
    }

    public float GetThrustPercent()
    {
        return thrustPercent;
    }

    //For drone mechanics
    public void SetV_Thrust(float percent)
    {
        verticalThrust = percent;
    }
    public void SetPitch(float percent)
    {
        pitch = percent;
    }
    public void SetRoll(float percent)
    {
        roll = percent;
    }
    public void SetYaw(float percent)
    {
        yaw = percent;
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        BiVector3 forceAndTorqueThisFrame = 
            CalculateAerodynamicForces(rb.velocity, rb.angularVelocity, Vector3.zero, 1.2f, rb.worldCenterOfMass);

        Vector3 velocityPrediction = PredictVelocity(forceAndTorqueThisFrame.p
            + transform.forward * thrust * thrustPercent + Physics.gravity * rb.mass);
        Vector3 angularVelocityPrediction = PredictAngularVelocity(forceAndTorqueThisFrame.q);

        BiVector3 forceAndTorquePrediction = 
            CalculateAerodynamicForces(velocityPrediction, angularVelocityPrediction, Vector3.zero, 1.2f, rb.worldCenterOfMass);

        currentForceAndTorque = (forceAndTorqueThisFrame + forceAndTorquePrediction) * 0.5f;
        rb.AddForce(currentForceAndTorque.p);
        rb.AddTorque(currentForceAndTorque.q);

        rb.AddForce(transform.forward * thrust * thrustPercent);

        //for drone mechanics
        //rb.AddForce(transform.up * 8000f * verticalThrust);
        rb.AddForceAtPosition(transform.up * 500f * verticalThrust, motor_F_L.position);
        rb.AddForceAtPosition(transform.up * 500f * verticalThrust, motor_F_R.position);
        rb.AddForceAtPosition(transform.up * 1000f * verticalThrust, motor_B_L.position);
        rb.AddForceAtPosition(transform.up * 1000f * verticalThrust, motor_B_R.position);
        rb.AddTorque(transform.right * pitch*-1000f*verticalThrust);
        rb.AddTorque(transform.forward * roll*1300f*verticalThrust);
        rb.AddTorque(transform.up * yaw *1000f*verticalThrust);
    }

    private BiVector3 CalculateAerodynamicForces(Vector3 velocity, Vector3 angularVelocity, Vector3 wind, float airDensity, Vector3 centerOfMass)
    {
        BiVector3 forceAndTorque = new BiVector3();
        foreach (var surface in aerodynamicSurfaces)
        {
            Vector3 relativePosition = surface.transform.position - centerOfMass;
            forceAndTorque += surface.CalculateForces(-velocity + wind-Vector3.Cross(angularVelocity,relativePosition),airDensity, relativePosition);
        }
        return forceAndTorque;
    }

    private Vector3 PredictVelocity(Vector3 force)
    {
        return rb.velocity + Time.fixedDeltaTime * PREDICTION_TIMESTEP_FRACTION * force / rb.mass;
    }

    private Vector3 PredictAngularVelocity(Vector3 torque)
    {
        Quaternion inertiaTensorWorldRotation = rb.rotation * rb.inertiaTensorRotation;
        Vector3 torqueInDiagonalSpace = Quaternion.Inverse(inertiaTensorWorldRotation) * torque;
        Vector3 angularVelocityChangeInDiagonalSpace;
        angularVelocityChangeInDiagonalSpace.x = torqueInDiagonalSpace.x / rb.inertiaTensor.x;
        angularVelocityChangeInDiagonalSpace.y = torqueInDiagonalSpace.y / rb.inertiaTensor.y;
        angularVelocityChangeInDiagonalSpace.z = torqueInDiagonalSpace.z / rb.inertiaTensor.z;

        return rb.angularVelocity + Time.fixedDeltaTime * PREDICTION_TIMESTEP_FRACTION
            * (inertiaTensorWorldRotation * angularVelocityChangeInDiagonalSpace);
    }

#if UNITY_EDITOR
    // For gizmos drawing.
    public void CalculateCenterOfLift(out Vector3 center, out Vector3 force, Vector3 displayAirVelocity, float displayAirDensity)
    {
        Vector3 com;
        BiVector3 forceAndTorque;
        if (aerodynamicSurfaces == null)
        {
            center = Vector3.zero;
            force = Vector3.zero;
            return;
        }

        if (rb == null)
        {
            com = GetComponent<Rigidbody>().worldCenterOfMass;
            forceAndTorque = CalculateAerodynamicForces(-displayAirVelocity, Vector3.zero, Vector3.zero, displayAirDensity, com);
        }
        else
        {
            com = rb.worldCenterOfMass;
            forceAndTorque = currentForceAndTorque;
        }

        force = forceAndTorque.p;
        center = com + Vector3.Cross(forceAndTorque.p, forceAndTorque.q) / forceAndTorque.p.sqrMagnitude;
    }
#endif
}


