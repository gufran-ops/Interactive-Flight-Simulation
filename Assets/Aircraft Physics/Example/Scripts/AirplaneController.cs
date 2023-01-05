using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AirplaneController : MonoBehaviour
{
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    List<WheelCollider> wheels = null;
    [SerializeField]
    public float rollControlSensitivity = 0.2f;
    [SerializeField]
    public float pitchControlSensitivity = 0.2f;
    [SerializeField]
    public float yawControlSensitivity = 0.2f;

    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(0, 1)]
    public float Flap;
    [SerializeField]
    Text displayText = null;

    public AudioSource drone;
    public AudioSource plane;


    float thrustPercent;
    float brakesTorque;

    float vThrust;
    [Range(-1, 1)]
    float droneRoll;
    [Range(-1, 1)]
    float droneYaw;
    [Range(-1, 1)]
    float dronePitch;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    private void Start()
    {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal");
        Yaw = Input.GetAxis("Yaw");
        if(Input.GetAxis("ForwardThrust")!=0)
        thrustPercent+= 0.0007f*Input.GetAxis("ForwardThrust");
        if(thrustPercent>1)
        thrustPercent=1;
        if(thrustPercent<0)
        thrustPercent=0;
        drone.volume = thrustPercent*thrustPercent/2;
        drone.pitch = 0.7f+thrustPercent*thrustPercent/3.5f;

        //For Drone input
        dronePitch = Input.GetAxis("DronePitch");
        droneRoll = Input.GetAxis("DroneRoll");
        droneYaw = Input.GetAxis("DroneYaw");
        vThrust+= 0.0007f*Input.GetAxis("VerticalThrust");
        if(vThrust>1)
        vThrust=1;
        if(vThrust<0)
        vThrust=0;
        plane.volume = vThrust*vThrust/2;
        plane.pitch = 0.7f+vThrust*vThrust/2;



        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     thrustPercent = thrustPercent > 0 ? 0 : 1f;
        // }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            brakesTorque = brakesTorque > 0 ? 0 : 1000000000f;
        }

        displayText.text = "Absolute Velocity: " + ((int)rb.velocity.magnitude).ToString("D3") + " m/s\n";
        displayText.text += "Altitude: " + ((int)transform.position.y).ToString("D4") + " m\n";
        displayText.text += "ForwardThrust: " + (int)(thrustPercent * 100) + "%\n";
        displayText.text += "UpwardThrust: " + (int)(vThrust * 100) + "%\n";
        displayText.text += brakesTorque > 0 ? "Brakes: ON" : "B: OFF";
    }

    private void FixedUpdate()
    {
        SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);

        aircraftPhysics.SetV_Thrust(vThrust);
        aircraftPhysics.SetPitch(dronePitch);
        aircraftPhysics.SetRoll(droneRoll);
        aircraftPhysics.SetYaw(droneYaw);

        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakesTorque;
            // small torque to wake up wheel collider
            wheel.motorTorque = 0.01f;
        }
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap)
    {
        foreach (var surface in controlSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
    }
}
