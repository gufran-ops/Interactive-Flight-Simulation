using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftBodyMovements : MonoBehaviour
{
    public AircraftPhysics aircraftPhysics;
    public AirplaneController airplaneController;

    [Range(-1, 1)]
    float pitch, roll, yaw, verticalThrust;


    public Transform prop_Forward;
    public Transform prop_Front_L;
    public Transform prop_Front_R;
    public Transform prop_Back_L;
    public Transform prop_Back_R;


    public Transform aeliron_L;
    public Transform aeliron_R;
    public Transform elevator;
    public Transform rudder_L;
    public Transform rudder_R;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        pitch = airplaneController.Pitch;
        roll = airplaneController.Roll;
        yaw = airplaneController.Yaw;
        verticalThrust = aircraftPhysics.verticalThrust;
        // dronePitch = aircraftPhysics.dronePitch;
        // droneRoll = aircraftPhysics.droneRoll;
        // droneYaw = aircraftPhysics.droneYaw;



      
        aeliron_L.localRotation = Quaternion.AngleAxis(roll*20f, Vector3.forward);
        aeliron_R.localRotation = Quaternion.AngleAxis(-roll*20f, Vector3.forward);



        elevator.localRotation = Quaternion.AngleAxis(pitch*20f, Vector3.forward);

        rudder_L.localRotation = Quaternion.AngleAxis(yaw*35f, Vector3.up);
        rudder_R.localRotation = Quaternion.AngleAxis(yaw*35f, Vector3.up);

        prop_Forward.Rotate(Vector3.down, aircraftPhysics.GetThrustPercent()* Time.deltaTime * speed);


        prop_Front_L.Rotate(Vector3.up, verticalThrust* Time.deltaTime * speed);
        prop_Front_R.Rotate(Vector3.up, verticalThrust* Time.deltaTime * speed);
        prop_Back_L.Rotate(Vector3.up, verticalThrust* Time.deltaTime * speed);
        prop_Back_R.Rotate(Vector3.up, verticalThrust* Time.deltaTime * speed);



    }
}
