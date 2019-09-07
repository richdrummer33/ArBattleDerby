using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMotorController : MonoBehaviour
{
    public List<WheelCollider> wheels = new List<WheelCollider>();

    bool kbDrive = false;

    Vector3 positionStart;
    Quaternion rotStart;

    bool resetting;

    int ct = 0;

    void Start()
    {
        positionStart = transform.position;
        rotStart = transform.rotation;
    }

    private IEnumerator Reset()
    {
        resetting = true;

        yield return new WaitForSeconds(1f);

        transform.position = positionStart;
        transform.rotation = rotStart;

        resetting = false;
    }

    void FixedUpdate()
    {
        WheelHit hit;

        bool hitL = wheels[0].GetGroundHit(out hit);
        bool hitR = wheels[1].GetGroundHit(out hit);

        if (!hitL && !hitR && !resetting)
        {
            ct++;

            if (ct > 60)
            {
                StartCoroutine(Reset());
                ct = 0;
            }
        }


        if (kbDrive)
        {
            float torque = 0f;
            float steer = 0f;

            if (Input.GetKey(KeyCode.W))
            {
                torque = 30f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                torque = -30f;
            }

            if (Input.GetKey(KeyCode.A))
            {
                steer = -45f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                steer = 45f;
            }

            foreach (WheelCollider wheel in wheels)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    torque *= 1.5f;
                }
                wheel.motorTorque = torque;
                wheel.steerAngle = steer;
            }
        }
    }


    public void SteerLeft()
    {
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = 35f;
            wheel.steerAngle = -45f;
            Debug.Log("steering L now");
        }
    }

    public void SteerRight()
    {
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = 35f;
            wheel.steerAngle = 45f;
            Debug.Log("steering R now");
        }
    }

    public void SteerStraight()
    {
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = 45f;
        }
    }

    public void StopCar()
    {
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = 0f;
            wheel.steerAngle = 0f;
            Debug.Log("steering R now");
        }
    }


}
